// Copyright 2020 Energinet DataHub A/S
//
// Licensed under the Apache License, Version 2.0 (the "License2");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Azure;
using Azure.Messaging.ServiceBus;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Energinet.DataHub.Core.FunctionApp.TestCommon.ServiceBus.ResourceProvider;
using Energinet.DataHub.MessageHub.Model.DataAvailable;
using Energinet.DataHub.MessageHub.Model.Exceptions;
using Energinet.DataHub.MessageHub.Model.IntegrationEvents;
using Energinet.DataHub.MessageHub.Model.Model;
using Energinet.DataHub.MessageHub.Model.Peek;
using FluentAssertions;
using GreenEnergyHub.Charges.Domain.Charges.Exceptions;
using GreenEnergyHub.Charges.Infrastructure.Core.MessageMetaData;
using GreenEnergyHub.Charges.IntegrationTest.Core.TestCommon;
using GreenEnergyHub.Charges.IntegrationTest.Core.TestHelpers;
using NodaTime;

namespace GreenEnergyHub.Charges.IntegrationTest.Core.MessageHub
{
    public class MessageHubMock : IAsyncDisposable
    {
        private const int SecondsToWaitForIntegrationEvents = 25;

        private readonly ServiceBusTestListener _messageHubDataAvailableServiceBusTestListener;
        private readonly ServiceBusTestListener _messageHubReplyServiceBusTestListener;
        private readonly QueueResource _messageHubRequestQueueResource;
        private readonly List<DataAvailableNotificationDto> _notifications = new();
        private readonly string _replyToQueueName;
        private readonly BlobContainerClient _blobContainerClient;
        private readonly IRequestBundleParser _requestBundleParser = new RequestBundleParser();
        private readonly IResponseBundleParser _responseBundleParser = new ResponseBundleParser();
        private readonly IDataAvailableNotificationParser _dataAvailableNotificationParser =
            new DataAvailableNotificationParser();

        public MessageHubMock(
            ServiceBusTestListener messageHubDataAvailableServiceBusTestListener,
            QueueResource messageHubRequestQueueResource,
            ServiceBusTestListener messageHubReplyServiceBusTestListener,
            string replyToQueueName,
            BlobContainerClient blobContainerClient)
        {
            _messageHubDataAvailableServiceBusTestListener = messageHubDataAvailableServiceBusTestListener;
            _messageHubRequestQueueResource = messageHubRequestQueueResource;
            _messageHubReplyServiceBusTestListener = messageHubReplyServiceBusTestListener;
            _replyToQueueName = replyToQueueName;
            _blobContainerClient = blobContainerClient;
        }

        /// <summary>
        /// Clears the state between simulations.
        /// </summary>
        public void Reset()
        {
            _notifications.Clear();
            _messageHubDataAvailableServiceBusTestListener.Reset();
            _messageHubReplyServiceBusTestListener.Reset();
        }

        public async ValueTask DisposeAsync()
        {
            await _messageHubDataAvailableServiceBusTestListener.DisposeAsync().ConfigureAwait(false);
            await _messageHubRequestQueueResource.DisposeAsync().ConfigureAwait(false);
            await _messageHubReplyServiceBusTestListener.DisposeAsync().ConfigureAwait(false);

            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///     Waits for the specified correlation ids to arrive on the dataavailable queue
        ///     and adds their notifications to the current simulation.
        ///     Can throw a TimeoutException or TaskCanceledException.
        /// </summary>
        /// <param name="correlationId">correlation id to wait for</param>
        /// <param name="noOfDataAvailableNotifications"></param>
        public async Task WaitForNotificationsInDataAvailableQueueAsync(
            string correlationId, int noOfDataAvailableNotifications)
        {
            using var eventualDataAvailableEvent = await _messageHubDataAvailableServiceBusTestListener
                .ListenForEventsAsync(correlationId, noOfDataAvailableNotifications)
                .ConfigureAwait(false);

            var isDataAvailableEventReceived = eventualDataAvailableEvent.CountdownEvent!
                .Wait(TimeSpan.FromSeconds(SecondsToWaitForIntegrationEvents));

            isDataAvailableEventReceived.Should().BeTrue();
            eventualDataAvailableEvent.EventualServiceBusMessages.Count.Should().Be(noOfDataAvailableNotifications);
            eventualDataAvailableEvent.EventualServiceBusMessages.Select(x => x.Body).Should().NotBeNull();

            _notifications.AddRange(
                eventualDataAvailableEvent.EventualServiceBusMessages.Select(x =>
                _dataAvailableNotificationParser.Parse(x.Body!.ToArray())));
        }

        /// <summary>
        /// Requests data bundle on the charges bundle request queue and waits for document uri's to be returned
        /// on the charges bundle reply queue.
        /// </summary>
        /// <returns>List of <see cref="PeekSimulatorResponseDto"/></returns>
        public async Task<List<PeekSimulatorResponseDto>> PeekAsync(ResponseFormat responseFormat)
        {
            ExtendedInvalidOperationException.ThrowIfNullOrNoElements(
                _notifications, $"{nameof(MessageHubMock)}: No dataavailable was provided for Peek");

            var dataBundleRequestDtos = new List<DataBundleRequestDto>();
            var peekSimulatorResponseDtos = new List<PeekSimulatorResponseDto>();
            var distinctMessages = _notifications
                .Select(x => new DistinctMessage(x.MessageType.Value, x.Recipient.Value))
                .Distinct();

            var sessionId = Guid.NewGuid().ToString("N");
            foreach (var message in distinctMessages)
            {
                var requestId = Guid.NewGuid();
                var blobName = await AddDataAvailableNotificationIdsToStorageAsync(requestId, message).ConfigureAwait(false);

                await CreateAndSendDataBundleRequestAsync(message, sessionId, dataBundleRequestDtos, blobName, requestId, responseFormat);
            }

            var eventualMessageHubReply = await ReceiveEventualServiceBusEvents(dataBundleRequestDtos);

            foreach (var eventualServiceBusEvent in eventualMessageHubReply.EventualServiceBusMessages)
                ParseDataBundleResponse(eventualServiceBusEvent, dataBundleRequestDtos, peekSimulatorResponseDtos);

            _messageHubReplyServiceBusTestListener.Reset();

            return peekSimulatorResponseDtos;
        }

        /// <summary>
        /// Downloads all bundled document in the <see cref="peekSimulationResponseDto"/>.
        /// </summary>
        /// <param name="peekSimulationResponseDto"></param>
        /// <returns>Bundled document as string</returns>
        public async Task<string> DownloadPeekResultAsync(PeekSimulatorResponseDto peekSimulationResponseDto)
        {
            ArgumentNullException.ThrowIfNull(peekSimulationResponseDto);
            ArgumentNullException.ThrowIfNull(peekSimulationResponseDto.Content!.Path, "Content path");

            var uri = peekSimulationResponseDto.Content.Path;
            var availableDataReferenceId = uri.Segments.Last().TrimEnd('/');

            var blobClient = _blobContainerClient.GetBlobClient(availableDataReferenceId);
            BlobDownloadResult downloadResult = await blobClient.DownloadContentAsync();
            return downloadResult.Content.ToString();
        }

        private async Task CreateAndSendDataBundleRequestAsync(
            DistinctMessage message,
            string sessionId,
            ICollection<DataBundleRequestDto> dataBundleRequestDtos,
            string blobName,
            Guid requestId,
            ResponseFormat responseFormat)
        {
            var correlationId = CorrelationIdGenerator.Create();
            var request = BuildDataBundleRequestDto(correlationId, message, requestId, blobName, responseFormat);

            await SendDataBundleAsync(sessionId, request, correlationId).ConfigureAwait(false);

            dataBundleRequestDtos.Add(request);
        }

        private static DataBundleRequestDto BuildDataBundleRequestDto(
            string correlationId, DistinctMessage message, Guid requestId, string blobName, ResponseFormat responseFormat)
        {
            var request = new DataBundleRequestDto(
                RequestId: requestId,
                DataAvailableNotificationReferenceId: blobName,
                IdempotencyId: correlationId,
                new MessageTypeDto(message.MessageType),
                responseFormat,
                1.0);
            return request;
        }

        private async Task<EventualServiceBusEvents> ReceiveEventualServiceBusEvents(
            IReadOnlyCollection<DataBundleRequestDto> dataBundleRequestDtos)
        {
            var eventualMessageHubReply = await _messageHubReplyServiceBusTestListener
                .ListenForEventsAsync(
                    dataBundleRequestDtos.Select(x => x.IdempotencyId).ToList(),
                    dataBundleRequestDtos.Count)
                .ConfigureAwait(false);

            var isMessageHubReplyReceived = eventualMessageHubReply
                .CountdownEvent!
                .Wait(TimeSpan.FromSeconds(SecondsToWaitForIntegrationEvents));

            if (isMessageHubReplyReceived == false)
            {
                // For debugging, set breakpoint here, to view differences in state of dataBundleRequestDtos vs. eventualMessageHubReply
                isMessageHubReplyReceived.Should().BeTrue();
            }

            return eventualMessageHubReply;
        }

        private async Task<string> AddDataAvailableNotificationIdsToStorageAsync(Guid requestId, DistinctMessage message)
        {
            var blobName = $"{message.MessageType}_{message.Recipient:N}_{requestId:N}";

            var dataAvailableNotificationDtosForMessageType = _notifications.Where(x =>
                    x.MessageType.Value == message.MessageType &&
                    x.Recipient.Value == message.Recipient)
                .ToList();

            try
            {
                await using var memoryStream = new MemoryStream();
                foreach (var dataAvailableNotificationDto in dataAvailableNotificationDtosForMessageType)
                    memoryStream.Write(dataAvailableNotificationDto.Uuid.ToByteArray());
                memoryStream.Position = 0;

                await _blobContainerClient.UploadBlobAsync(blobName, memoryStream).ConfigureAwait(false);
            }
            catch (RequestFailedException e)
            {
                throw new MessageHubStorageException("Error uploading file to storage", e);
            }

            return blobName;
        }

        private async Task SendDataBundleAsync(
            string sessionId,
            DataBundleRequestDto dataBundleRequestDto,
            string correlationId)
        {
            var bytes = _requestBundleParser.Parse(dataBundleRequestDto);
            var requestServiceBusMessage = CreateRequestMessageHubServiceBusMessage(correlationId, bytes, sessionId);

            await MockTelemetryClient.WrappedOperationWithTelemetryDependencyInformationAsync(
                () => _messageHubRequestQueueResource.SenderClient.SendMessageAsync(requestServiceBusMessage), correlationId);
        }

        private void ParseDataBundleResponse(
            EventualServiceBusMessage eventualServiceBusEvent,
            IEnumerable<DataBundleRequestDto> dataBundleRequestDtos,
            ICollection<PeekSimulatorResponseDto> peekSimulatorResponseDtos)
        {
            eventualServiceBusEvent.Body.Should().NotBeNull();
            var messageBody = eventualServiceBusEvent.Body!;
            var dataBundleRequestDto = dataBundleRequestDtos
                .Single(x => x.IdempotencyId == eventualServiceBusEvent.CorrelationId);

            var peekResponse = _responseBundleParser.Parse(messageBody.ToArray());

            peekSimulatorResponseDtos.Add(peekResponse.IsErrorResponse
                ? new PeekSimulatorResponseDto()
                : new PeekSimulatorResponseDto(
                    dataBundleRequestDto.RequestId,
                    dataBundleRequestDto.IdempotencyId,
                    new AzureBlobContentDto(peekResponse.ContentUri)));
        }

        private ServiceBusMessage CreateRequestMessageHubServiceBusMessage(
            string correlationId, byte[] bytes, string sessionId)
        {
            var serviceBusMessage = new ServiceBusMessage(bytes)
            {
                SessionId = sessionId,
                ReplyToSessionId = sessionId,
                ReplyTo = _replyToQueueName,
                TimeToLive = TimeSpan.FromSeconds(SecondsToWaitForIntegrationEvents),
                ApplicationProperties =
                {
                    new KeyValuePair<string, object>(
                        MessageMetaDataConstants.OperationTimestamp,
                        SystemClock.Instance.GetCurrentInstant().ToString(null, CultureInfo.InvariantCulture)),
                    new KeyValuePair<string, object>(MessageMetaDataConstants.CorrelationId, correlationId),
                    new KeyValuePair<string, object>(MessageMetaDataConstants.MessageVersion, 1),
                    new KeyValuePair<string, object>(
                        MessageMetaDataConstants.MessageType,
                        IntegrationEventsMessageType.RequestDataBundle.ToString()),
                    new KeyValuePair<string, object>(MessageMetaDataConstants.EventIdentification, Guid.NewGuid()),
                },
            };
            return serviceBusMessage;
        }

        private record DistinctMessage(string MessageType, Guid Recipient)
        {
            public override string ToString()
            {
                return $"{{ MessageType = {MessageType}, Recipient = {Recipient} }}";
            }
        }
    }
}
