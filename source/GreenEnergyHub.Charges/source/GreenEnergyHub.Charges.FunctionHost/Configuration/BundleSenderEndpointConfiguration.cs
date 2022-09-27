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

using Energinet.DataHub.MessageHub.Model.Peek;
using GreenEnergyHub.Charges.MessageHub.Infrastructure.Bundling;
using GreenEnergyHub.Charges.MessageHub.Infrastructure.Cim;
using GreenEnergyHub.Charges.MessageHub.Infrastructure.Cim.Bundles.ChargeLinkReceipt;
using GreenEnergyHub.Charges.MessageHub.Infrastructure.Cim.Bundles.ChargeLinks;
using GreenEnergyHub.Charges.MessageHub.Infrastructure.Cim.Bundles.ChargeReceipt;
using GreenEnergyHub.Charges.MessageHub.Infrastructure.Cim.Bundles.Charges;
using GreenEnergyHub.Charges.MessageHub.MessageHub;
using GreenEnergyHub.Charges.MessageHub.Models.AvailableChargeData;
using GreenEnergyHub.Charges.MessageHub.Models.AvailableChargeLinksData;
using GreenEnergyHub.Charges.MessageHub.Models.AvailableChargeLinksReceiptData;
using GreenEnergyHub.Charges.MessageHub.Models.AvailableChargeReceiptData;
using Microsoft.Extensions.DependencyInjection;

namespace GreenEnergyHub.Charges.FunctionHost.Configuration
{
    internal static class BundleSenderEndpointConfiguration
    {
        internal static void ConfigureServices(IServiceCollection serviceCollection)
        {
            // Common for all bundles
            serviceCollection.AddScoped<IBundleCreatorProvider, BundleCreatorProvider>();
            serviceCollection.AddScoped<IBundleSender, BundleSender>();
            serviceCollection.AddScoped<IBundleReplier, BundleReplier>();
            serviceCollection.AddScoped<IRequestBundleParser, RequestBundleParser>();

            // Charge bundles
            serviceCollection.AddScoped<IBundleCreator, BundleCreator<AvailableChargeData>>();
            serviceCollection.AddScoped<ICimSerializer<AvailableChargeData>, ChargeCimSerializer>();
            serviceCollection.AddScoped<IBundleCreator, BundleCreator<AvailableChargeReceiptData>>();
            serviceCollection.AddScoped<ICimSerializer<AvailableChargeReceiptData>, ChargeReceiptCimSerializer>();
            serviceCollection.AddScoped<IBundleCreator, BundleCreator<AvailableChargePriceData>>();
            serviceCollection.AddScoped<ICimSerializer<AvailableChargePriceData>, ChargePriceCimSerializer>();

            serviceCollection.AddScoped<ICimJsonSerializer<AvailableChargeData>, ChargeCimJsonSerializer>();
            serviceCollection.AddScoped<ICimJsonSerializer<AvailableChargePriceData>, ChargePriceCimJsonSerializer>();

            // Charge link bundles
            serviceCollection.AddScoped<IBundleCreator, BundleCreator<AvailableChargeLinksData>>();
            serviceCollection.AddScoped<ICimSerializer<AvailableChargeLinksData>, ChargeLinkCimSerializer>();
            serviceCollection.AddScoped<IBundleCreator, BundleCreator<AvailableChargeLinksReceiptData>>();
            serviceCollection.AddScoped<ICimSerializer<AvailableChargeLinksReceiptData>, ChargeLinksReceiptCimSerializer>();
        }
    }
}
