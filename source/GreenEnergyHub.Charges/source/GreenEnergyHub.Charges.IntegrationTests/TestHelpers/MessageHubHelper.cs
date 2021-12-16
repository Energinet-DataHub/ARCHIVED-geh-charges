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
using Energinet.DataHub.MessageHub.IntegrationTesting;

namespace GreenEnergyHub.Charges.IntegrationTests.TestHelpers
{
    public static class MessageHubHelper
    {
        /// <summary>
        /// Initiates a peek and assert that a reply is received.
        /// </summary>
        public static async Task AssertPeekReceivesReplyAsync(
            this MessageHubSimulation messageHub,
            string correlationId,
            int noOfMessageTypes = 1)
        {
            var correlationIds = Enumerable.Repeat(correlationId, noOfMessageTypes).ToArray();

            // Throws if expected data available message (by correlation ID) is not received
            await messageHub.WaitForNotificationsInDataAvailableQueueAsync(correlationIds);

            // Invokes the domain and ensures that a reply to the peek request is received for each message type
            for (var i = 0; i < noOfMessageTypes; i++)
            {
                await messageHub.PeekAsync(); // Throws if corresponding peek reply is not received
            }
        }
    }
}
