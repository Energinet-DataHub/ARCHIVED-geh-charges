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

using GreenEnergyHub.Charges.Application.ChargeLinks.Handlers;
using GreenEnergyHub.Charges.Application.ChargeLinks.MessageHub;
using GreenEnergyHub.Charges.Domain.AvailableChargeLinksData;
using GreenEnergyHub.Charges.Domain.Dtos.DefaultChargeLinksDataAvailableNotifiedEvents;
using GreenEnergyHub.Charges.FunctionHost.Common;
using GreenEnergyHub.Charges.Infrastructure.Internal.ChargeLinkCommandAccepted;
using GreenEnergyHub.Charges.Infrastructure.Internal.DefaultChargeLinksCreated;
using GreenEnergyHub.Charges.Infrastructure.Messaging.Registration;
using SimpleInjector;

namespace GreenEnergyHub.Charges.FunctionHost.Configuration
{
    internal static class ChargeLinkDataAvailableNotifierConfiguration
    {
        internal static void ConfigureServices(Container container)
        {
            container.ReceiveProtobufMessage<ChargeLinkCommandAccepted>(
                configuration => configuration.WithParser(() => ChargeLinkCommandAccepted.Parser));

            container.SendProtobufMessage<DefaultChargeLinksCreated>();
            container.AddMessagingProtobuf().AddMessageDispatcher<DefaultChargeLinksCreatedEvent>(
                EnvironmentHelper.GetEnv(EnvironmentSettingNames.DomainEventSenderConnectionString),
                EnvironmentHelper.GetEnv(EnvironmentSettingNames.DefaultChargeLinksDataAvailableNotifiedTopicName));
            container
                .Register<IChargeLinkDataAvailableNotifierEndpointHandler,
                    ChargeLinkDataAvailableNotifierEndpointHandler>(Lifestyle.Scoped);
            container.Register<IChargeLinkDataAvailableNotifier, ChargeLinkDataAvailableNotifier>(Lifestyle.Scoped);
            container.Register<IAvailableChargeLinksDataFactory, AvailableChargeLinksDataFactory>(Lifestyle.Scoped);
        }
    }
}
