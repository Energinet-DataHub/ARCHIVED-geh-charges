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

using GreenEnergyHub.Charges.Application.Charges.MessageHub;
using GreenEnergyHub.Charges.Application.MessageHub;
using GreenEnergyHub.Charges.Domain.AvailableChargeReceiptData;
using GreenEnergyHub.Charges.Domain.AvailableData;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommandAcceptedEvents;
using GreenEnergyHub.Charges.Infrastructure.ChargeReceiptBundle.Cim;
using GreenEnergyHub.Charges.Infrastructure.Cim;
using GreenEnergyHub.Charges.Infrastructure.Internal.ChargeCommandAccepted;
using GreenEnergyHub.Charges.Infrastructure.Messaging.Registration;
using Microsoft.Extensions.DependencyInjection;

namespace GreenEnergyHub.Charges.FunctionHost.Configuration
{
    internal static class ChargeConfirmationDataAvailableNotifierEndpointConfiguration
    {
        internal static void ConfigureServices(IServiceCollection serviceCollection)
        {
            /*serviceCollection.ReceiveProtobufMessage<ChargeCommandAcceptedContract>(
                configuration => configuration.WithParser(() => ChargeCommandAcceptedContract.Parser));

            serviceCollection
                .AddScoped<IAvailableDataNotifier<AvailableChargeReceiptData, ChargeCommandAcceptedEvent>,
                    AvailableDataNotifier<AvailableChargeReceiptData, ChargeCommandAcceptedEvent>>();
            serviceCollection
                .AddScoped<IAvailableDataFactory<AvailableChargeReceiptData, ChargeCommandAcceptedEvent>,
                    AvailableChargeConfirmationDataFactory>();
            serviceCollection
                .AddScoped<IAvailableDataNotificationFactory<AvailableChargeReceiptData>,
                    AvailableDataNotificationFactory<AvailableChargeReceiptData>>();
            serviceCollection
                .AddScoped<BundleSpecification<AvailableChargeReceiptData, ChargeCommandAcceptedEvent>,
                    ChargeConfirmationBundleSpecification>();
            serviceCollection.AddScoped<ICimSerializer<AvailableChargeReceiptData>, ChargeReceiptCimSerializer>();*/
        }
    }
}
