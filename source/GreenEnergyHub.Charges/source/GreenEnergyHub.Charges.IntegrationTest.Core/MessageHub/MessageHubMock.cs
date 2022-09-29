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
            using var eventualAvailableDataEvent = await _messageHubDataAvailableServiceBusTestListener
                .ListenForEventsAsync(correlationId, noOfDataAvailableNotifications)
                .ConfigureAwait(false);

            var isAvailableDataEventReceived = eventualAvailableDataEvent.CountdownEvent!
                .Wait(TimeSpan.FromSeconds(SecondsToWaitForIntegrationEvents));

            isAvailableDataEventReceived.Should().BeTrue();
            eventualAvailableDataEvent.EventualServiceBusMessages.Count.Should().Be(noOfDataAvailableNotifications);
            eventualAvailableDataEvent.EventualServiceBusMessages.Select(x => x.Body).Should().NotBeNull();

            _notifications.AddRange(
                eventualAvailableDataEvent.EventualServiceBusMessages.Select(x =>
                _dataAvailableNotificationParser.Parse(x.Body!.ToArray())));
        }

        public async Task<List<PeekSimulatorResponseDto>> PeekAsync()
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
                await BuildDataBundleRequestAndRequestDataBundleAsync(message, sessionId, dataBundleRequestDtos);

            var eventualMessageHubReply = await ReceiveEventualServiceBusEvents(dataBundleRequestDtos);

            foreach (var eventualServiceBusEvent in eventualMessageHubReply.EventualServiceBusMessages)
                ParseDataBundleResponse(eventualServiceBusEvent, dataBundleRequestDtos, peekSimulatorResponseDtos);

            _messageHubReplyServiceBusTestListener.Reset();

            return peekSimulatorResponseDtos;
        }

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

        private async Task BuildDataBundleRequestAndRequestDataBundleAsync(
            DistinctMessage message,
            string sessionId,
            ICollection<DataBundleRequestDto> dataBundleRequestDtos)
        {
            var requestId = Guid.NewGuid();
            var blobName = $"{message.MessageType}_{message.Recipient:N}_{requestId:N}";
            await AddDataAvailableNotificationIdsToStorageAsync(blobName, message).ConfigureAwait(false);

            var correlationId = BuildDataBundleRequestDto(message, requestId, blobName, out var request);

            await RequestDataBundleAsync(sessionId, request, correlationId).ConfigureAwait(false);

            dataBundleRequestDtos.Add(request);
        }

        private static string BuildDataBundleRequestDto(
            DistinctMessage message, Guid requestId, string blobName, out DataBundleRequestDto request)
        {
            var correlationId = CorrelationIdGenerator.Create();
            request = new DataBundleRequestDto(
                RequestId: requestId,
                DataAvailableNotificationReferenceId: blobName,
                IdempotencyId: correlationId,
                new MessageTypeDto(message.MessageType),
                ResponseFormat.Xml,
                1.0);
            return correlationId;
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

        private async Task AddDataAvailableNotificationIdsToStorageAsync(string blobName, DistinctMessage message)
        {
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
        }

        private async Task RequestDataBundleAsync(
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
