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
using GreenEnergyHub.Charges.Domain.ChargeLinkCreatedEvents;
using GreenEnergyHub.Charges.Infrastructure.Integration.ChargeLinkCreated;
using GreenEnergyHub.Charges.Infrastructure.Internal.ChargeLinkCommandAccepted;
using GreenEnergyHub.Charges.Infrastructure.Messaging.Registration;
using GreenEnergyHub.Messaging.Protobuf;
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
                EnvironmentHelper.GetEnv("INTEGRATIONEVENT_SENDER_CONNECTION_STRING"),
                EnvironmentHelper.GetEnv("CHARGE_LINK_CREATED_TOPIC_NAME"));
        }
    }
}
