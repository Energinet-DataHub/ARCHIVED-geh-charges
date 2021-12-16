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

using Energinet.DataHub.Core.Messaging.Protobuf;
using GreenEnergyHub.Charges.Application.ChargeLinks.Handlers;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeLinkCreatedEvents;
using GreenEnergyHub.Charges.FunctionHost.Common;
using GreenEnergyHub.Charges.Infrastructure.Core.MessagingExtensions.Registration;
using GreenEnergyHub.Charges.Infrastructure.Integration.ChargeLinkCreated;
using GreenEnergyHub.Charges.Infrastructure.Internal.ChargeLinkCommandAccepted;
using Microsoft.Extensions.DependencyInjection;

namespace GreenEnergyHub.Charges.FunctionHost.Configuration
{
    internal static class ChargeLinkEventPublisherConfiguration
    {
        internal static void ConfigureServices(IServiceCollection serviceCollection)
        {
            serviceCollection.AddScoped<IChargeLinkCreatedEventFactory, ChargeLinkCreatedEventFactory>();
            serviceCollection.AddScoped<IChargeLinkEventPublishHandler, ChargeLinkEventPublishHandler>();

            serviceCollection.ReceiveProtobufMessage<ChargeLinkCommandAccepted>(
                configuration => configuration.WithParser(() => ChargeLinkCommandAccepted.Parser));

            serviceCollection.SendProtobuf<ChargeLinkCreatedContract>();
            serviceCollection.AddMessagingProtobuf().AddMessageDispatcher<ChargeLinkCreatedEvent>(
                EnvironmentHelper.GetEnv(EnvironmentSettingNames.DataHubSenderConnectionString),
                EnvironmentHelper.GetEnv(EnvironmentSettingNames.ChargeLinkCreatedTopicName));
        }
    }
}
