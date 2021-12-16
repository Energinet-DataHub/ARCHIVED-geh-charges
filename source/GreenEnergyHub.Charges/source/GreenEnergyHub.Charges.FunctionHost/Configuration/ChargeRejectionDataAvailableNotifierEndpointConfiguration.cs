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

using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommandRejectedEvents;
using GreenEnergyHub.Charges.Infrastructure.Core.MessagingExtensions.Registration;
using GreenEnergyHub.Charges.Infrastructure.Internal.ChargeCommandRejected;
using GreenEnergyHub.Charges.MessageHub.Application.Charges;
using GreenEnergyHub.Charges.MessageHub.Application.MessageHub;
using GreenEnergyHub.Charges.MessageHub.Models.AvailableChargeReceiptData;
using GreenEnergyHub.Charges.MessageHub.Models.AvailableData;
using Microsoft.Extensions.DependencyInjection;

namespace GreenEnergyHub.Charges.FunctionHost.Configuration
{
    internal static class ChargeRejectionDataAvailableNotifierEndpointConfiguration
    {
        internal static void ConfigureServices(IServiceCollection serviceCollection)
        {
            serviceCollection.ReceiveProtobufMessage<ChargeCommandRejectedContract>(
                configuration => configuration.WithParser(() => ChargeCommandRejectedContract.Parser));
            serviceCollection
                .AddScoped<IAvailableDataNotifier<AvailableChargeReceiptData, ChargeCommandRejectedEvent>,
                    AvailableDataNotifier<AvailableChargeReceiptData, ChargeCommandRejectedEvent>>();
            serviceCollection
                .AddScoped<IAvailableDataFactory<AvailableChargeReceiptData, ChargeCommandRejectedEvent>,
                    AvailableChargeRejectionDataFactory>();
            serviceCollection
                .AddScoped<IAvailableDataNotificationFactory<AvailableChargeReceiptData>,
                    AvailableDataNotificationFactory<AvailableChargeReceiptData>>();
            serviceCollection
                .AddScoped<BundleSpecification<AvailableChargeReceiptData, ChargeCommandRejectedEvent>,
                    ChargeRejectionBundleSpecification>();
        }
    }
}
