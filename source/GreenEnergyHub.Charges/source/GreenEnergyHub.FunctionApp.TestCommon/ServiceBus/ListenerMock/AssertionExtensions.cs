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
using Azure.Messaging.ServiceBus;

namespace GreenEnergyHub.FunctionApp.TestCommon.ServiceBus.ListenerMock
{
    /// <summary>
    /// Actually service bus listener mock extensions, but we want to split those up
    /// into categories, so here we will place the AssertReceived extensions.
    /// </summary>
    public static class AssertionExtensions
    {
        /// <summary>
        /// Assert we have received expected message exactly once.
        /// </summary>
        public static void AssertReceived(this ServiceBusListenerMock mock, Func<ServiceBusReceivedMessage, bool> matcher)
        {
            AssertReceived(mock, expectedCount: 1, matcher);
        }

        /// <summary>
        /// Assert we have received expected message a number of times.
        /// </summary>
        public static void AssertReceived(this ServiceBusListenerMock mock, int expectedCount, Func<ServiceBusReceivedMessage, bool> matcher)
        {
            var count = mock.ReceivedMessages.Count(matcher);
            if (count != expectedCount)
            {
                var nonMatchingCount = mock.ReceivedMessages.Count(ctx => !matcher(ctx));
                throw new AssertionException($"Expected to receive {expectedCount} matching messages. Actually received {count} matching messages and {nonMatchingCount} non matching messages.");
            }
        }
    }
}
