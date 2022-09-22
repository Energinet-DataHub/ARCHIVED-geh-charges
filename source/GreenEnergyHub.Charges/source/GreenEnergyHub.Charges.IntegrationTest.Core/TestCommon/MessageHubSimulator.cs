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
using System.Linq;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Energinet.DataHub.MessageHub.IntegrationTesting;
using FluentAssertions;
using GreenEnergyHub.Charges.Infrastructure.Core.InternalMessaging;

namespace GreenEnergyHub.Charges.IntegrationTest.Core.TestCommon
{
    public class MessageHubSimulator : IAsyncDisposable
    {
        private const int SecondsToWaitForIntegrationEvents = 15;
        private readonly string _serviceBusResourceProviderCorrectionString;
        private readonly ServiceBusTestListener _messageHubDataAvailableServiceBusTestListener;
        private readonly string _messageHubRequestQueueName;
        private readonly string _messageHubReplyQueueName;
        private readonly string _messageHubStorageConnectionString;
        private readonly string _messageHubStorageContainerName;

        public MessageHubSimulator(
            string serviceBusResourceProviderCorrectionString,
            ServiceBusTestListener messageHubDataAvailableServiceBusTestListener,
            string messageHubRequestQueueName,
            string messageHubReplyQueueName,
            string messageHubStorageConnectionString,
            string messageHubStorageContainerName)
        {
            _serviceBusResourceProviderCorrectionString = serviceBusResourceProviderCorrectionString;
            _messageHubDataAvailableServiceBusTestListener = messageHubDataAvailableServiceBusTestListener;
            _messageHubRequestQueueName = messageHubRequestQueueName;
            _messageHubReplyQueueName = messageHubReplyQueueName;
            _messageHubStorageConnectionString = messageHubStorageConnectionString;
            _messageHubStorageContainerName = messageHubStorageContainerName;
        }

        /// <summary>
        /// Waits for the specified correlation ids to arrive on the dataavailable queue
        /// and adds their notifications to the current simulation.
        /// Can throw a TimeoutException or TaskCanceledException.
        /// </summary>
        /// <param name="correlationId">correlation id to wait for</param>
        public async Task WaitForNotificationsInDataAvailableQueueAsync(string correlationId)
        {
            using var eventualAvailableDataEvent = await _messageHubDataAvailableServiceBusTestListener
                .ListenForMessageAsync(correlationId)
                .ConfigureAwait(false);

            var isAvailableDataEventReceived = eventualAvailableDataEvent
                .MessageAwaiter!
                .Wait(TimeSpan.FromSeconds(SecondsToWaitForIntegrationEvents));
            isAvailableDataEventReceived.Should().BeTrue();
        }

        public static async Task<string> DownloadPeekResult(PeekSimulationResponseDto peekSimulationResponseDto)
        {
            ArgumentNullException.ThrowIfNull(peekSimulationResponseDto);
            ArgumentNullException.ThrowIfNull(peekSimulationResponseDto.Content!.Path);

            var uri = peekSimulationResponseDto.Content.Path;
            var availableDataReferenceId = uri.Segments.Last().TrimEnd('/');

            const string connectionString = ChargesServiceBusResourceNames.MessageHubStorageConnectionString;
            const string blobContainerName = ChargesServiceBusResourceNames.MessageHubStorageContainerName;

            var blobServiceClient = new BlobServiceClient(connectionString);
            var blobClient = blobServiceClient
                .GetBlobContainerClient(blobContainerName)
                .GetBlobClient(availableDataReferenceId);

            BlobDownloadResult downloadResult = await blobClient.DownloadContentAsync();
            return downloadResult.Content.ToString();
        }

        public Task<PeekSimulationResponseDto> PeekAsync()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Clears the state between simulations.
        /// </summary>
        public void Clear()
        {
            //_notifications.Clear();
        }

        public async ValueTask DisposeAsync()
        {
            /*await _dataAvailableReceiver.DisposeAsync().ConfigureAwait(false);
            await _messageBusFactory.DisposeAsync().ConfigureAwait(false);*/
            await Task.CompletedTask;
        }
    }
}
