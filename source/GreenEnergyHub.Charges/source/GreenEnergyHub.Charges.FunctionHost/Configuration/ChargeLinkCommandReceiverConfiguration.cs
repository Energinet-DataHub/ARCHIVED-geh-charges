﻿// Copyright 2020 Energinet DataHub A/S
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
using GreenEnergyHub.Charges.Domain.ChargeLinks;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeLinksAcceptedEvents;
using GreenEnergyHub.Charges.FunctionHost.Common;
using GreenEnergyHub.Charges.Infrastructure.Internal.ChargeLinkCommandAccepted;
using GreenEnergyHub.Charges.Infrastructure.Internal.ChargeLinkCommandReceived;
using GreenEnergyHub.Charges.Infrastructure.Messaging.Registration;
using GreenEnergyHub.Charges.Infrastructure.Repositories;
using GreenEnergyHub.Messaging.Protobuf;
using Microsoft.Extensions.DependencyInjection;

namespace GreenEnergyHub.Charges.FunctionHost.Configuration
{
    internal static class ChargeLinkCommandReceiverConfiguration
    {
        internal static void ConfigureServices(IServiceCollection serviceCollection)
        {
            serviceCollection.AddScoped<IChargeLinksCommandReceivedHandler, ChargeLinksCommandReceivedHandler>();
            serviceCollection.AddScoped<IChargeLinkFactory, ChargeLinkFactory>();
            serviceCollection.AddSingleton<IChargeLinksAcceptedEventFactory, ChargeLinksAcceptedEventFactory>();

            serviceCollection.ReceiveProtobufMessage<ChargeLinkCommandReceived>(
                configuration => configuration.WithParser(() => ChargeLinkCommandReceived.Parser));
            serviceCollection.SendProtobuf<ChargeLinkCommandAccepted>();
            serviceCollection.AddMessagingProtobuf().AddMessageDispatcher<ChargeLinksAcceptedEvent>(
                EnvironmentHelper.GetEnv(EnvironmentSettingNames.DomainEventSenderConnectionString),
                EnvironmentHelper.GetEnv(EnvironmentSettingNames.ChargeLinkAcceptedTopicName));

            serviceCollection.AddScoped<IChargeLinkRepository, ChargeLinkRepository>();
        }
    }
}
