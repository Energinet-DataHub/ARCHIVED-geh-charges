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

using Azure.Messaging.ServiceBus;
using Energinet.DataHub.Charges.Libraries.DefaultChargeLink;
using Energinet.DataHub.Charges.Libraries.DefaultChargeLinkMessages;
using Energinet.DataHub.Charges.Libraries.Providers;
using GreenEnergyHub.Charges.Domain.Dtos.DefaultChargeLinksDataAvailableNotifiedEvents;
using GreenEnergyHub.Charges.FunctionHost.Common;
using GreenEnergyHub.Charges.Infrastructure.Internal.DefaultChargeLinksDataAvailableNotifiedLinkCommandReceived;
using GreenEnergyHub.Charges.Infrastructure.Messaging.Registration;
using GreenEnergyHub.Messaging.Protobuf;
using Microsoft.Extensions.DependencyInjection;

namespace GreenEnergyHub.Charges.FunctionHost.Configuration
{
    internal static class CreateDefaultChargeLinksClientConfiguration
    {
        internal static void ConfigureServices(IServiceCollection serviceCollection)
        {
            var serviceBusConnectionString =
                EnvironmentHelper.GetEnv(EnvironmentSettingNames.DataHubSenderConnectionString);
            var serviceBusClient = new ServiceBusClient(serviceBusConnectionString);

            serviceCollection.SendProtobuf<DefaultChargeLinksDataAvailableNotifiedLinkCommandReceived>();
            serviceCollection.AddMessagingProtobuf().AddMessageDispatcher<DefaultChargeLinkDataAvailableNotifierEvent>(
                EnvironmentHelper.GetEnv(EnvironmentSettingNames.DomainEventSenderConnectionString),
                EnvironmentHelper.GetEnv(EnvironmentSettingNames.DefaultChargeLinksDataAvailableNotifiedTopicName));

            AddDefaultChargeLinkClient(serviceCollection, serviceBusClient);
            AddDefaultChargeLinkMessagesClient(serviceCollection, serviceBusClient);
        }

        private static void AddDefaultChargeLinkMessagesClient(
            IServiceCollection serviceCollection,
            ServiceBusClient client)
        {
            var replyToQueueName = EnvironmentHelper.GetEnv(EnvironmentSettingNames.CreateLinkMessagesReplyQueueName);
            var defaultChargeLinkClientServiceBusRequestSenderProvider =
                new DefaultChargeLinkMessagesClientServiceBusRequestSenderProvider(client, replyToQueueName);
            serviceCollection.AddSingleton<IDefaultChargeLinkMessagesClient>(_ =>
                new DefaultChargeLinkMessagesClient(defaultChargeLinkClientServiceBusRequestSenderProvider));
        }

        private static void AddDefaultChargeLinkClient(
            IServiceCollection serviceCollection,
            ServiceBusClient client)
        {
            var replyToQueueName = EnvironmentHelper.GetEnv(EnvironmentSettingNames.CreateLinkReplyQueueName);

            var defaultChargeLinkClientServiceBusRequestSenderProvider =
                new DefaultChargeLinkClientServiceBusRequestSenderProvider(client, replyToQueueName);
            serviceCollection.AddSingleton<IDefaultChargeLinkClient>(_ =>
                new DefaultChargeLinkClient(defaultChargeLinkClientServiceBusRequestSenderProvider));
        }
    }
}
