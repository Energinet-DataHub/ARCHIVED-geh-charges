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
using System.Threading.Tasks;
using Energinet.DataHub.MessageHub.IntegrationTesting;

namespace GreenEnergyHub.Charges.IntegrationTest.Core.TestHelpers
{
    public static class MessageHubHelper
    {
        /// <summary>
        /// Listen for dataAvailable events, initiates a peek and assert that a reply is received.
        /// </summary>
        public static async Task AssertPeekReceivesRepliesAsync(
            this MessageHubSimulation messageHub,
            string correlationId,
            int noOfMessageTypes = 1)
        {
            var noOfReceivedMessages = 0;

            for (var i = 0; i < noOfMessageTypes; i++)
            {
                try
                {
                    // Throws if expected data available message (by correlation ID) is not received
                    await messageHub.WaitForNotificationsInDataAvailableQueueAsync(correlationId);

                    // Invokes the domain and ensures that a reply to the peek request is received for each message type
                    await messageHub.PeekAsync(); // Throws if corresponding peek reply is not received
                    noOfReceivedMessages++;
                }
                catch (Exception ex) when (ex is TaskCanceledException or TimeoutException)
                {
                    var error = $"MessageHub received only {noOfReceivedMessages} of {noOfMessageTypes} expected messages!";
                    throw new InvalidOperationException(error, ex);
                }
                finally
                {
                    messageHub.Clear();
                }
            }
        }
    }
}
