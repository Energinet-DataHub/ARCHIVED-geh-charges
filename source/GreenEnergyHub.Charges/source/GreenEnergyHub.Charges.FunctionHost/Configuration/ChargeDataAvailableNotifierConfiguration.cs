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

using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommandAcceptedEvents;
using GreenEnergyHub.Charges.MessageHub.Application.Charges;
using GreenEnergyHub.Charges.MessageHub.Application.MessageHub;
using GreenEnergyHub.Charges.MessageHub.Models.AvailableChargeData;
using GreenEnergyHub.Charges.MessageHub.Models.AvailableData;
using Microsoft.Extensions.DependencyInjection;

namespace GreenEnergyHub.Charges.FunctionHost.Configuration
{
    internal static class ChargeDataAvailableNotifierConfiguration
    {
        internal static void ConfigureServices(IServiceCollection serviceCollection)
        {
            serviceCollection.AddScoped<IAvailableDataNotifier<AvailableChargeData, ChargeCommandAcceptedEvent>,
                AvailableDataNotifier<AvailableChargeData, ChargeCommandAcceptedEvent>>();
            serviceCollection.AddScoped<IAvailableDataFactory<AvailableChargeData, ChargeCommandAcceptedEvent>,
                AvailableChargeDataFactory>();
            serviceCollection.AddScoped<IAvailableDataNotificationFactory<AvailableChargeData>,
                AvailableDataNotificationFactory<AvailableChargeData>>();
            serviceCollection
                .AddScoped<BundleSpecification<AvailableChargeData, ChargeCommandAcceptedEvent>,
                    ChargeBundleSpecification>();
        }
    }
}
