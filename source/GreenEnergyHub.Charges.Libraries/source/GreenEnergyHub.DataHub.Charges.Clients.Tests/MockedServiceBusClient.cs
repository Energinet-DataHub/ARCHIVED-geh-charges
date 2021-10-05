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
using System.Threading;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;

namespace GreenEnergyHub.DataHub.Charges.Clients.CreateDefaultChargeLink.Tests
{
    internal sealed class MockedServiceBusClient : ServiceBusClient
    {
        private readonly string _queue;
        private readonly string _replyQueue;
        private readonly ServiceBusSender _serviceBusSender;
        private readonly ServiceBusSessionReceiver _serviceBusSessionReceiver;

        public MockedServiceBusClient(
            string queue,
            string replyQueue,
            ServiceBusSender serviceBusSender,
            ServiceBusSessionReceiver serviceBusSessionReceiver)
        {
            _queue = queue;
            _replyQueue = replyQueue;
            _serviceBusSender = serviceBusSender;
            _serviceBusSessionReceiver = serviceBusSessionReceiver;
        }

        public override ServiceBusSender CreateSender(string queueOrTopicName)
        {
            if (queueOrTopicName == _queue)
                return _serviceBusSender;

            throw new ArgumentException($"{nameof(queueOrTopicName)}: '{queueOrTopicName}' did not match configured queue: '{_queue}'", nameof(queueOrTopicName));
        }

        public override async ValueTask DisposeAsync()
        {
            try
            {
                await base.DisposeAsync().ConfigureAwait(false);
            }
            catch (NullReferenceException)
            {
                // Mocked ServiceBusClient does not have a Connection, which crashes DisposeAsync().
            }
        }
    }
}
