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

using GreenEnergyHub.Charges.FunctionHost.Common;
using GreenEnergyHub.Charges.FunctionHost.Configuration;
using GreenEnergyHub.Charges.Infrastructure.MarketParticipantRegistry;
using GreenEnergyHub.Charges.Infrastructure.MarketParticipantRegistry.GridAreaLinksSynchronization;
using GreenEnergyHub.Charges.Infrastructure.MarketParticipantRegistry.GridAreasSynchronization;
using GreenEnergyHub.Charges.Infrastructure.MarketParticipantRegistry.MarketParticipantsSynchronization;
using GreenEnergyHub.Charges.Infrastructure.MarketParticipantRegistry.Persistence;
using GreenEnergyHub.Charges.Infrastructure.Core.Registration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace GreenEnergyHub.Charges.FunctionHost.System
{
    internal static class MarketParticipantRegistryEndpointConfiguration
    {
        internal static void ConfigureServices(IServiceCollection serviceCollection)
        {
            serviceCollection.AddScoped<IMarketParticipantRegistrySynchronizer, MarketParticipantRegistrySynchronizer>();
            serviceCollection.AddScoped<IMarketParticipantsSynchronizer, MarketParticipantsSynchronizer>();
            serviceCollection.AddScoped<IGridAreasSynchronizer, GridAreasSynchronizer>();
            serviceCollection.AddScoped<IGridAreaLinksSynchronizer, GridAreaLinksSynchronizer>();

            var connectionString = EnvironmentHelper.GetEnv(
                EnvironmentSettingNames.MarketParticipantRegistryDbConnectionString);
            serviceCollection.AddDbContext<MarketParticipantRegistry>(
                options => options.UseSqlServer(connectionString));
            serviceCollection.AddScoped<IMarketParticipantRegistry, MarketParticipantRegistry>();
        }
    }
}
