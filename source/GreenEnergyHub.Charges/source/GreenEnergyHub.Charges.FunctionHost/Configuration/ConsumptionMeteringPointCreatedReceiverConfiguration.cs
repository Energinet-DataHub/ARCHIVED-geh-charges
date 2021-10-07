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

using Energinet.DataHub.MeteringPoints.IntegrationEventContracts;
using GreenEnergyHub.Charges.Application.MeteringPoints.Handlers;
using GreenEnergyHub.Charges.Infrastructure.Messaging.Registration;
using GreenEnergyHub.MeteringPoints.IntegrationEventContracts;
using Microsoft.Extensions.DependencyInjection;

namespace GreenEnergyHub.Charges.FunctionHost.Configuration
{
    internal static class ConsumptionMeteringPointCreatedReceiverConfiguration
    {
        internal static void ConfigureServices(IServiceCollection serviceCollection)
        {
            serviceCollection.ReceiveProtobufMessage<ConsumptionMeteringPointCreated>(
                configuration => configuration.WithParser(() => ConsumptionMeteringPointCreated.Parser));

            serviceCollection.AddScoped<IConsumptionMeteringPointCreatedEventHandler, ConsumptionMeteringPointCreatedEventHandler>();
        }
    }
}
