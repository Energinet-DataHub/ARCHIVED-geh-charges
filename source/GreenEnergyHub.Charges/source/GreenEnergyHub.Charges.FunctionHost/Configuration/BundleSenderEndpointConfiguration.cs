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
using GreenEnergyHub.Charges.Application.MessageHub;
using GreenEnergyHub.Charges.Domain.AvailableChargeData;
using GreenEnergyHub.Charges.Domain.AvailableChargeLinkReceiptData;
using GreenEnergyHub.Charges.Domain.AvailableChargeLinksData;
using GreenEnergyHub.Charges.Infrastructure.ChargeBundle.Cim;
using GreenEnergyHub.Charges.Infrastructure.ChargeLinkBundle.Cim;
using GreenEnergyHub.Charges.Infrastructure.ChargeLinkReceiptBundle.Cim;
using GreenEnergyHub.Charges.Infrastructure.Cim;
using GreenEnergyHub.Charges.Infrastructure.MessageHub;
using Microsoft.Extensions.DependencyInjection;
using SimpleInjector;

namespace GreenEnergyHub.Charges.FunctionHost.Configuration
{
    internal static class BundleSenderEndpointConfiguration
    {
        internal static void ConfigureServices(Container container)
        {
            // Common for all bundles
            container.Register<IBundleCreatorProvider, BundleCreatorProvider>(Lifestyle.Scoped);
            container.Register<IBundleSender, BundleSender>(Lifestyle.Scoped);
            container.Register<IBundleReplier, BundleReplier>(Lifestyle.Scoped);
            container.Register<IRequestBundleParser, RequestBundleParser>(Lifestyle.Scoped);

            // Charge bundles
            container.Register<IBundleCreator, BundleCreator<AvailableChargeData>>(Lifestyle.Scoped);
            container.Register<ICimSerializer<AvailableChargeData>, ChargeCimSerializer>(Lifestyle.Scoped);

            // Charge link bundles
            container.Register<IBundleCreator, BundleCreator<AvailableChargeLinksData>>(Lifestyle.Scoped);
            container.Register<ICimSerializer<AvailableChargeLinksData>, ChargeLinkCimSerializer>(Lifestyle.Scoped);
            container.Register<IBundleCreator, BundleCreator<AvailableChargeLinkReceiptData>>(Lifestyle.Scoped);
            container.Register<ICimSerializer<AvailableChargeLinkReceiptData>, ChargeLinkReceiptCimSerializer>(Lifestyle.Scoped);
        }
    }
}
