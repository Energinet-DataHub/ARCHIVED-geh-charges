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

using GreenEnergyHub.Charges.Domain.Dtos.ChargeLinksRejectionEvents;
using GreenEnergyHub.Charges.Infrastructure.Core.MessagingExtensions.Registration;
using GreenEnergyHub.Charges.Infrastructure.Internal.ChargeLinksCommandRejected;
using GreenEnergyHub.Charges.MessageHub.Application.ChargeLinks;
using GreenEnergyHub.Charges.MessageHub.Application.MessageHub;
using GreenEnergyHub.Charges.MessageHub.Models.AvailableChargeLinkReceiptData;
using GreenEnergyHub.Charges.MessageHub.Models.AvailableData;
using Microsoft.Extensions.DependencyInjection;

namespace GreenEnergyHub.Charges.FunctionHost.Configuration
{
    internal static class ChargeLinksRejectionDataAvailableNotifierEndpointConfiguration
    {
        internal static void ConfigureServices(IServiceCollection serviceCollection)
        {
            serviceCollection.ReceiveProtobufMessage<ChargeLinksCommandRejected>(
                configuration => configuration.WithParser(() => ChargeLinksCommandRejected.Parser));
            serviceCollection
                .AddScoped<IAvailableDataNotifier<AvailableChargeLinkReceiptData, ChargeLinksRejectedEvent>,
                    AvailableDataNotifier<AvailableChargeLinkReceiptData, ChargeLinksRejectedEvent>>();
            serviceCollection
                .AddScoped<IAvailableDataFactory<AvailableChargeLinkReceiptData, ChargeLinksRejectedEvent>,
                    AvailableChargeLinksRejectionDataFactory>();
            serviceCollection
                .AddScoped<IAvailableDataNotificationFactory<AvailableChargeLinkReceiptData>,
                    AvailableDataNotificationFactory<AvailableChargeLinkReceiptData>>();
            serviceCollection
                .AddScoped<BundleSpecification<AvailableChargeLinkReceiptData, ChargeLinksRejectedEvent>,
                    ChargeLinksRejectionBundleSpecification>();
        }
    }
}
