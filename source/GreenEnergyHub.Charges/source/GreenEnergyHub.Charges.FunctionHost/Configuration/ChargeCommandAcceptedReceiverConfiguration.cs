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

using GreenEnergyHub.Charges.Application.Charges.Acknowledgement;
using GreenEnergyHub.Charges.Application.Charges.Factories;
using GreenEnergyHub.Charges.Application.Charges.Handlers;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommandAcceptedEvents;
using GreenEnergyHub.Charges.FunctionHost.Common;
using GreenEnergyHub.Charges.Infrastructure.Core.MessagingExtensions;
using GreenEnergyHub.Charges.Infrastructure.Core.MessagingExtensions.Registration;
using GreenEnergyHub.Charges.Infrastructure.Core.MessagingExtensions.Serialization;
using GreenEnergyHub.Charges.Infrastructure.Core.Registration;
using Microsoft.Extensions.DependencyInjection;

namespace GreenEnergyHub.Charges.FunctionHost.Configuration
{
    internal static class ChargeIntegrationEventsPublisherEndpointConfiguration
    {
        internal static void ConfigureServices(IServiceCollection serviceCollection)
        {
            serviceCollection.AddScoped<MessageExtractor<ChargeInformationCommandAcceptedEvent>>();
            serviceCollection.AddScoped<IChargeCreatedEventFactory, ChargeCreatedEventFactory>();
            serviceCollection.AddScoped<IChargePublisher, ChargePublisher>();
            serviceCollection.AddScoped<IChargeIntegrationEventsPublisher, ChargeIntegrationEventsPublisher>();
            serviceCollection.AddScoped<JsonMessageDeserializer<ChargeInformationCommandAcceptedEvent>>();

            serviceCollection.AddMessagingProtobuf()
                .AddExternalMessageDispatcher<ChargeCreatedEvent>(
                    EnvironmentHelper.GetEnv(EnvironmentSettingNames.DataHubSenderConnectionString),
                    EnvironmentHelper.GetEnv(EnvironmentSettingNames.ChargeCreatedTopicName));
        }
    }
}
