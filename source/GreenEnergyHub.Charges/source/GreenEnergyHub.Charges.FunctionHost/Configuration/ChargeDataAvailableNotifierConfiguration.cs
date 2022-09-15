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

using GreenEnergyHub.Charges.Application.Charges.Events;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommandAcceptedEvents;
using GreenEnergyHub.Charges.Infrastructure.Core.MessagingExtensions.Serialization;
using GreenEnergyHub.Charges.MessageHub.BundleSpecification;
using GreenEnergyHub.Charges.MessageHub.BundleSpecification.Charges;
using GreenEnergyHub.Charges.MessageHub.MessageHub;
using GreenEnergyHub.Charges.MessageHub.Models.AvailableChargeData;
using GreenEnergyHub.Charges.MessageHub.Models.AvailableData;
using Microsoft.Extensions.DependencyInjection;

namespace GreenEnergyHub.Charges.FunctionHost.Configuration
{
    internal static class ChargeDataAvailableNotifierConfiguration
    {
        internal static void ConfigureServices(IServiceCollection serviceCollection)
        {
            serviceCollection.AddScoped<IAvailableDataNotifier<AvailableChargeData, ChargeInformationCommandAcceptedEvent>,
                AvailableDataNotifier<AvailableChargeData, ChargeInformationCommandAcceptedEvent>>();
            serviceCollection.AddScoped<IAvailableDataFactory<AvailableChargeData, ChargeInformationCommandAcceptedEvent>,
                AvailableChargeDataFactory>();
            serviceCollection.AddScoped<IAvailableDataNotificationFactory<AvailableChargeData>,
                AvailableDataNotificationFactory<AvailableChargeData>>();
            serviceCollection
                .AddScoped<BundleSpecification<AvailableChargeData, ChargeInformationCommandAcceptedEvent>,
                    ChargeBundleSpecification>();

            serviceCollection.AddScoped<IAvailableDataNotifier<AvailableChargePriceData, ChargePriceOperationsConfirmedEvent>,
                AvailableDataNotifier<AvailableChargePriceData, ChargePriceOperationsConfirmedEvent>>();
            serviceCollection.AddScoped<IAvailableDataFactory<AvailableChargePriceData, ChargePriceOperationsConfirmedEvent>,
                AvailableChargePriceDataFactory>();
            serviceCollection.AddScoped<IAvailableDataNotificationFactory<AvailableChargePriceData>,
                AvailableDataNotificationFactory<AvailableChargePriceData>>();

            serviceCollection
                .AddScoped<BundleSpecification<AvailableChargePriceData, ChargePriceOperationsConfirmedEvent>,
                    ChargePriceBundleSpecification>();

            serviceCollection.AddScoped<JsonMessageDeserializer<ChargeInformationCommandAcceptedEvent>>();
            serviceCollection.AddScoped<JsonMessageDeserializer<ChargePriceOperationsConfirmedEvent>>();
        }
    }
}
