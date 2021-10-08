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
using Azure.Messaging.ServiceBus;

namespace GreenEnergyHub.FunctionApp.TestCommon.ServiceBus.ListenerMock
{
    public class DoProvider
    {
        internal DoProvider(ServiceBusListenerMock parent, Func<ServiceBusReceivedMessage, bool> messageMatcher)
        {
            Parent = parent;
            MessageMatcher = messageMatcher;
        }

        private ServiceBusListenerMock Parent { get; }

        private Func<ServiceBusReceivedMessage, bool> MessageMatcher { get; }

        /// <summary>
        /// Add message handler.
        /// </summary>
        public async Task<ServiceBusListenerMock> DoAsync(Func<ServiceBusReceivedMessage, Task> messageHandler)
        {
            await Parent.AddMessageHandlerAsync(MessageMatcher, messageHandler).ConfigureAwait(false);
            return Parent;
        }
    }
}
