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
using GreenEnergyHub.Charges.FunctionHost.Common;
using GreenEnergyHub.Charges.Infrastructure.Core.MessagingExtensions.Registration;
using GreenEnergyHub.Charges.Infrastructure.Core.Registration;
using GreenEnergyHub.Charges.MessageHub.BundleSpecification;
using GreenEnergyHub.Charges.MessageHub.BundleSpecification.Charges;
using GreenEnergyHub.Charges.MessageHub.MessageHub;
using GreenEnergyHub.Charges.MessageHub.Models.AvailableChargeReceiptData;
using GreenEnergyHub.Charges.MessageHub.Models.AvailableData;
using GreenEnergyHub.Charges.MessageHub.Models.AvailableOperationReceiptData;
using Microsoft.Extensions.DependencyInjection;

namespace GreenEnergyHub.Charges.FunctionHost.Configuration
{
    internal static class ChargePriceConfirmedDataAvailableNotifierEndpointConfiguration
    {
        internal static void ConfigureServices(IServiceCollection serviceCollection)
        {
            serviceCollection.AddScoped<IAvailableDataNotifier<AvailableChargeReceiptData, PriceConfirmedEvent>,
                AvailableDataNotifier<AvailableChargeReceiptData, PriceConfirmedEvent>>();
            serviceCollection.AddScoped<IAvailableDataFactory<AvailableChargeReceiptData, PriceConfirmedEvent>,
                AvailableChargePriceOperationConfirmationsFactory>();
            serviceCollection.AddScoped<BundleSpecification<AvailableChargeReceiptData, PriceConfirmedEvent>,
                ChargePriceConfirmationBundleSpecification>();

            serviceCollection
                .AddMessaging()
                .AddInternalEventDispatcher<PriceConfirmedEvent>(
                    EnvironmentHelper.GetEnv(EnvironmentSettingNames.DomainEventSenderConnectionString),
                    EnvironmentHelper.GetEnv(EnvironmentSettingNames.ChargePriceConfirmedTopicName));
        }
    }
}
