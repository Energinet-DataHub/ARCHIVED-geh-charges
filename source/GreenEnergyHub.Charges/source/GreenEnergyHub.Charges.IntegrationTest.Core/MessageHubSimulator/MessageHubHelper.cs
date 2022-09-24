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
using System.Linq;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Energinet.DataHub.MessageHub.IntegrationTesting;
using FluentAssertions;
using GreenEnergyHub.Charges.Infrastructure.Core.InternalMessaging;

namespace GreenEnergyHub.Charges.IntegrationTest.Core.MessageHubSimulator
{
    public static class MessageHubHelper
    {
        /// <summary>
        /// Listen for dataAvailable events, initiates a peek and assert that a reply is received.
        /// </summary>
        public static async Task<List<string>> AssertPeekReceivesRepliesDeprecatedAsync(
            this MessageHubSimulation messageHub,
            string correlationId,
            int noOfMessageTypes = 1)
        {
            var peekResults = new List<string>();

            var expected = $"MessageHub received all {noOfMessageTypes} expected messages.";
            var actual = expected;

            for (var i = 0; i < noOfMessageTypes; i++)
            {
                try
                {
                    peekResults.Add(await WaitForDataAvailableAndPeekDeprecated(messageHub, correlationId));
                }
                catch (Exception ex) when (ex is TaskCanceledException or TimeoutException)
                {
                    actual = $"MessageHub received only {i} of {noOfMessageTypes} expected messages!";
                }
                finally
                {
                    messageHub.Clear();
                }
            }

            actual.Should().Be(expected);

            return peekResults;
        }

        /// <summary>
        /// Listen for dataAvailable events, initiates a peek and assert that a reply is received.
        /// </summary>
        public static async Task<List<string>> AssertPeekReceivesRepliesAsync(
            this MessageHubSimulator messageHubSimulator,
            string correlationId,
            int noOfMessageTypes = 1)
        {
            var peekResults = new List<string>();
            var expected = $"MessageHub received all {noOfMessageTypes} expected messages.";
            var actual = expected;

            try
            {
                // Throws if expected data available message (by correlation ID) is not received
                await messageHubSimulator.WaitForNotificationsInDataAvailableQueueAsync(correlationId, noOfMessageTypes);
            }
            catch (Exception ex) when (ex is TaskCanceledException or TimeoutException)
            {
                actual = $"MessageHub received fewer that the {noOfMessageTypes} expected messages!";
            }

            // Invokes the domain and ensures that a reply to the peek request is received for each message type
            var peekSimulationResponseDto = await messageHubSimulator.PeekAsync(correlationId); // Throws if corresponding peek reply is not received
            peekResults.Add(await DownloadPeekResult(messageHubSimulator, peekSimulationResponseDto));

            messageHubSimulator.Clear();

            actual.Should().Be(expected);

            return peekResults;
        }

        private static async Task<string> WaitForDataAvailableAndPeekDeprecated(
            MessageHubSimulation messageHub, string correlationId)
        {
            // Throws if expected data available message (by correlation ID) is not received
            await messageHub.WaitForNotificationsInDataAvailableQueueAsync(correlationId);

            // Invokes the domain and ensures that a reply to the peek request is received for each message type
            var peekSimulationResponseDto = await messageHub.PeekAsync(); // Throws if corresponding peek reply is not received
            return await DownloadPeekResultDeprecated(peekSimulationResponseDto);
        }

        private static async Task<string> DownloadPeekResult(
            MessageHubSimulator messageHubSimulator,
            PeekSimulatorResponseDto peekSimulationResponseDto)
        {
            var downloadResult = await messageHubSimulator.DownLoadPeekResultAsync(peekSimulationResponseDto);

            /*var uri = peekSimulationResponseDto.Content.Path;
            var availableDataReferenceId = uri.Segments.Last().TrimEnd('/');

            const string connectionString = ChargesServiceBusResourceNames.MessageHubStorageConnectionString;
            const string blobContainerName = ChargesServiceBusResourceNames.MessageHubStorageContainerName;

            var blobServiceClient = new BlobServiceClient(connectionString);
            var blobClient = blobServiceClient
                .GetBlobContainerClient(blobContainerName)
                .GetBlobClient(availableDataReferenceId);

            BlobDownloadResult downloadResult = await blobClient.DownloadContentAsync();
            return downloadResult.Content.ToString();*/

            return downloadResult;
        }

        private static async Task<string> DownloadPeekResultDeprecated(PeekSimulationResponseDto peekSimulationResponseDto)
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
    }
}
