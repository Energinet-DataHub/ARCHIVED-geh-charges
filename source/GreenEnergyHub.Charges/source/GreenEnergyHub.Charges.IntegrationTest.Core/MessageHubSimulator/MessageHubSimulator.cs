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
using GreenEnergyHub.Charges.Infrastructure.Core.MessageMetaData;
using GreenEnergyHub.Charges.IntegrationTest.Core.TestCommon;
using GreenEnergyHub.Charges.IntegrationTest.Core.TestHelpers;
using NodaTime;

namespace GreenEnergyHub.Charges.IntegrationTest.Core.MessageHubSimulator
{
    public class MessageHubSimulator : IAsyncDisposable
    {
        private const int SecondsToWaitForIntegrationEvents = 150;

        //private readonly IBundleCreatorProvider _bundleCreatorProvider = new BundleCreatorProvider();
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

        public MessageHubSimulator(
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
        ///     Clears the state between simulations.
        /// </summary>
        public void Clear()
        {
            _notifications.Clear();
        }

        public async ValueTask DisposeAsync()
        {
            /*await _dataAvailableReceiver.DisposeAsync().ConfigureAwait(false);
            await _messageBusFactory.DisposeAsync().ConfigureAwait(false);*/
            await Task.CompletedTask;
        }

        /// <summary>
        ///     Waits for the specified correlation ids to arrive on the dataavailable queue
        ///     and adds their notifications to the current simulation.
        ///     Can throw a TimeoutException or TaskCanceledException.
        /// </summary>
        /// <param name="correlationId">correlation id to wait for</param>
        /// <param name="noOfMessageTypes"></param>
        public async Task WaitForNotificationsInDataAvailableQueueAsync(string correlationId, int noOfMessageTypes)
        {
            using var eventualAvailableDataEvent = await _messageHubDataAvailableServiceBusTestListener
                .ListenForEventsAsync(correlationId, noOfMessageTypes)
                .ConfigureAwait(false);

            var isAvailableDataEventReceived = eventualAvailableDataEvent.CountdownEvent!
                .Wait(TimeSpan.FromSeconds(SecondsToWaitForIntegrationEvents));

            isAvailableDataEventReceived.Should().BeTrue();
            eventualAvailableDataEvent.EventualServiceBusMessages.Count.Should().Be(noOfMessageTypes);
            eventualAvailableDataEvent.EventualServiceBusMessages.Select(x => x.Body).Should().NotBeNull();

            _notifications.AddRange(
                eventualAvailableDataEvent.EventualServiceBusMessages.Select(x =>
                _dataAvailableNotificationParser.Parse(x.Body!.ToArray())));
        }

        public async Task<PeekSimulatorResponseDto> PeekAsync(string correlationId)
        {
            var requestId = Guid.NewGuid();
            var idempotencyId = correlationId; // TODO: not sure if this is correct?

            if (_notifications.Count == 0)
            {
                throw new InvalidOperationException(
                    $"{nameof(MessageHubSimulator)}: No dataavailable was provided for Peek");
            }

            var messageType = _notifications.Select(x => x.MessageType.Value).Distinct().Single();
            var dataAvailableNotificationIds = _notifications.Select(x => x.Uuid);
            var dataAvailableNotificationReferenceId = idempotencyId.ToString();

            await AddDataAvailableNotificationIdsToStorageAsync(
                    dataAvailableNotificationReferenceId, dataAvailableNotificationIds).ConfigureAwait(false);

            var request = new DataBundleRequestDto(
                requestId,
                dataAvailableNotificationReferenceId,
                idempotencyId.ToString(),
                new MessageTypeDto(messageType),
                ResponseFormat.Xml,
                1.0);

            // var newPeekResponse = CreateDataBundle(request, correlationId);
            var peekResponse = await RequestDataBundleAsync(request, correlationId).ConfigureAwait(false);

            /*// Not in use but must be valid
            const DomainOrigin domainOrigin = DomainOrigin.Charges;

            var peekResponse = await _dataBundleRequestSender
                .SendAsync(request, domainOrigin)
                .ConfigureAwait(false);
*/
            if (peekResponse == null)
            {
                throw new TimeoutException("MessageHubSimulation: Waiting for Peek reply timed out");
            }

            return peekResponse.IsErrorResponse
                ? new PeekSimulatorResponseDto()
                : new PeekSimulatorResponseDto(
                    requestId, dataAvailableNotificationReferenceId, new AzureBlobContentDto(peekResponse.ContentUri));
        }

        public async Task<string> DownLoadPeekResultAsync(PeekSimulatorResponseDto peekSimulationResponseDto)
        {
            ArgumentNullException.ThrowIfNull(peekSimulationResponseDto);
            ArgumentNullException.ThrowIfNull(peekSimulationResponseDto.Content!.Path);

            var uri = peekSimulationResponseDto.Content.Path;
            var availableDataReferenceId = uri.Segments.Last().TrimEnd('/');

            var blobClient = _blobContainerClient.GetBlobClient(availableDataReferenceId);
            BlobDownloadResult downloadResult = await blobClient.DownloadContentAsync();
            return downloadResult.Content.ToString();

            /*
            // Alternative implementation:
            BlobDownloadInfo download = await blobClient.DownloadAsync();
            var result = new byte[download.ContentLength];
            await download.Content.ReadAsync(result.AsMemory(0, (int)download.ContentLength));

            return Encoding.UTF8.GetString(result);*/
        }

        private async Task AddDataAvailableNotificationIdsToStorageAsync(
            string dataAvailableNotificationReferenceId, IEnumerable<Guid> dataAvailableNotificationIds)
        {
            try
            {
                await using var memoryStream = new MemoryStream();
                foreach (var guid in dataAvailableNotificationIds)
                    memoryStream.Write(guid.ToByteArray());
                memoryStream.Position = 0;

                await _blobContainerClient.UploadBlobAsync(
                    dataAvailableNotificationReferenceId, memoryStream).ConfigureAwait(false);
            }
            catch (RequestFailedException e)
            {
                throw new MessageHubStorageException("Error uploading file to storage", e);
            }
        }

        /*private object CreateDataBundle(DataBundleRequestDto request, string correlationId)
        {
            var bundleCreator = _bundleCreatorProvider.Get(request);
        }*/

        private async Task<DataBundleResponseDto?> RequestDataBundleAsync(
            DataBundleRequestDto dataBundleRequestDto, string correlationId)
        {
            var eventualMessageHubReply = await _messageHubReplyServiceBusTestListener
                .ListenForMessageAsync(correlationId).ConfigureAwait(false);

            var bytes = _requestBundleParser.Parse(dataBundleRequestDto);
            var sessionId = dataBundleRequestDto.RequestId.ToString();
            var requestServiceBusMessage = CreateRequestMessageHubServiceBusMessage(correlationId, bytes, sessionId);

            await MockTelemetryClient.WrappedOperationWithTelemetryDependencyInformationAsync(
                () => _messageHubRequestQueueResource.SenderClient.SendMessageAsync(requestServiceBusMessage), correlationId);

            var isMessageHubReplyReceived = eventualMessageHubReply
                .MessageAwaiter!
                .Wait(TimeSpan.FromSeconds(SecondsToWaitForIntegrationEvents));

            isMessageHubReplyReceived.Should().BeTrue();
            eventualMessageHubReply.Body.Should().NotBeNull();

            var messageBody = eventualMessageHubReply.Body!;
            var dataBundleResponseDto = _responseBundleParser.Parse(messageBody.ToArray());
            return dataBundleResponseDto;
        }

        /*private ServiceBusMessage CreateReplyMessageHubServiceBusMessage(string correlationId, byte[] bytes, string sessionId)
        {
            // TODO: Which bytes?
            var contractBytes = _responseBundleParser.Parse(dataBundleResponseDto);
            var serviceBusReplyMessage = new ServiceBusMessage(contractBytes)
            {
                SessionId = sessionId,
            }.AddDataBundleResponseIntegrationEvents(dataBundleResponseDto.RequestIdempotencyId);

            return serviceBusReplyMessage;
        }*/

        private ServiceBusMessage CreateRequestMessageHubServiceBusMessage(string correlationId, byte[] bytes, string sessionId)
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
            }; //.AddRequestDataBundleIntegrationEvents(dataBundleRequestDto.IdempotencyId);
            return serviceBusMessage;
        }
    }
}
