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

using System.Collections.Generic;
using System.Threading.Tasks;
using Energinet.DataHub.MessageHub.Model.Model;

namespace GreenEnergyHub.Charges.IntegrationTest.Core.MessageHub
{
    public static class MessageHubHelper
    {
        /// <summary>
        /// Listen for dataAvailable events, initiates a peek and assert that a reply is received.
        /// </summary>
        public static async Task<List<string>> AssertPeekReceivesRepliesAsync(
            this MessageHubMock messageHubMock,
            string correlationId,
            ResponseFormat responseFormat,
            int noOfDataAvailableNotifications = 1)
        {
            await messageHubMock.WaitForNotificationsInDataAvailableQueueAsync(correlationId, noOfDataAvailableNotifications);

            // Invokes the domain and ensures that a reply to the peek request is received for each message type
            // Throws if corresponding peek reply is not received
            var peekSimulatorResponseDtos = await messageHubMock.PeekAsync(responseFormat);

            var peekResults = new List<string>();
            foreach (var peekSimulatorResponseDto in peekSimulatorResponseDtos)
            {
                peekResults.Add(await messageHubMock.DownloadPeekResultAsync(peekSimulatorResponseDto));
            }

            messageHubMock.Reset();
            return peekResults;
        }
    }
}
