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
using GreenEnergyHub.Charges.Application.ChargeLinks.MessageHub;
using GreenEnergyHub.Charges.Application.ChargeLinks.MessageHub.Infrastructure;
using GreenEnergyHub.Charges.Application.Charges.MessageHub;
using GreenEnergyHub.Charges.Application.Charges.MessageHub.Infrastructure;
using GreenEnergyHub.Charges.Infrastructure.ChargeBundle.Cim;
using GreenEnergyHub.Charges.Infrastructure.ChargeBundle.MessageHub;
using GreenEnergyHub.Charges.Infrastructure.ChargeLinkBundle.Cim;
using GreenEnergyHub.Charges.Infrastructure.ChargeLinkBundle.MessageHub;
using GreenEnergyHub.Charges.Infrastructure.MessageHub;
using Microsoft.Extensions.DependencyInjection;

namespace GreenEnergyHub.Charges.FunctionHost.Configuration
{
    internal static class BundleSenderEndpointConfiguration
    {
        internal static void ConfigureServices(IServiceCollection serviceCollection)
        {
            serviceCollection.AddScoped<IBundleRequestDispatcher, BundleRequestDispatcher>();

            serviceCollection.AddScoped<IChargeBundleSender, ChargeBundleSender>();
            serviceCollection.AddScoped<IChargeBundleCreator, ChargeBundleCreator>();
            serviceCollection.AddScoped<IChargeBundleReplier, ChargeBundleReplier>();
            serviceCollection.AddScoped<IChargeCimSerializer, ChargeCimSerializer>();

            serviceCollection.AddScoped<IChargeLinkBundleSender, ChargeLinkBundleSender>();
            serviceCollection.AddScoped<IChargeLinkBundleCreator, ChargeLinkBundleCreator>();
            serviceCollection.AddScoped<IChargeLinkBundleReplier, ChargeLinkBundleReplier>();
            serviceCollection.AddScoped<IChargeLinkCimSerializer, ChargeLinkCimSerializer>();

            serviceCollection.AddScoped<IRequestBundleParser, RequestBundleParser>();
        }
    }
}
