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

using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Squadron;
using Xunit;
using Xunit.Abstractions;

namespace GreenEnergyHub.FunctionApp.TestCommon.Tests.Fixtures
{
    public class ServiceBusListenerMockFixture : IAsyncLifetime
    {
        public ServiceBusListenerMockFixture(IMessageSink messageSink)
        {
            ServiceBusResource = new AzureCloudServiceBusResource<ServiceBusListenerMockServiceBusOptions>(messageSink);

            ////QueueName = CreateUserPrefixedName("queue");
            ////QueueManager = new ServiceBusQueueManager(ConnectionString, QueueName);
            ////QueueSenderClient = QueueManager.CreateSenderClient();

            ////TopicName = CreateUserPrefixedName("topic");
            SubscriptionName = ServiceBusListenerMockServiceBusOptions.SubscriptionName;
            ////TopicManager = new ServiceBusTopicManager(ConnectionString, TopicName);
            ////TopicSenderClient = TopicManager.CreateSenderClient();

            ////SubscriptionManager = TopicManager.CreateSubscriptionManager(SubscriptionName);
        }

        public string ConnectionString { get; private set; }
            = string.Empty;

        public string QueueName { get; private set; }
            = string.Empty;

        [NotNull]
        public ServiceBusSender? QueueSenderClient { get; private set; }

        public string TopicName { get; private set; }
            = string.Empty;

        public string SubscriptionName { get; private set; }
            = string.Empty;

        [NotNull]
        public ServiceBusSender? TopicSenderClient { get; private set; }

        private AzureCloudServiceBusResource<ServiceBusListenerMockServiceBusOptions> ServiceBusResource { get; }

        [NotNull]
        private ServiceBusClient? Client { get; set; }

        public async Task InitializeAsync()
        {
            // It seem like when you close a MessageReceiver it sometimes still take the message of queue with out completing it.
            // So to avoid test fail we need delivery count to more than 1.
            // LockDuration is 5 sec. so when the message is taking by a closed receiver we only have to wait 5 sec. before it is released again.
            // Note that for debugging you maybe want to increase the time to more than 5 sec.
            ////await QueueManager.CreateQueueAsync(2, TimeSpan.FromSeconds(5));

            ////await TopicManager.CreateTopicAsync();
            ////await SubscriptionManager.CreateSubscriptionAsync(2, TimeSpan.FromSeconds(5));

            await ServiceBusResource.InitializeAsync();

            ConnectionString = ServiceBusResource.ConnectionString;

            Client = new ServiceBusClient(ConnectionString);

            QueueName = ServiceBusListenerMockServiceBusOptions.QueueName;
            QueueSenderClient = Client.CreateSender(QueueName);

            TopicName = ServiceBusListenerMockServiceBusOptions.TopicName;
            TopicSenderClient = Client.CreateSender(TopicName);
        }

        public async Task DisposeAsync()
        {
            await QueueSenderClient!.CloseAsync();

            ////await QueueManager.DeleteQueueAsync();
            ////await QueueManager.DisposeAsync();

            await TopicSenderClient!.CloseAsync();

            ////await TopicManager.DeleteTopicAsync();
            ////await TopicManager.DisposeAsync();

            ////await SubscriptionManager.DisposeAsync();

            await Client.DisposeAsync();

            await ServiceBusResource.DisposeAsync();
        }
    }
}
