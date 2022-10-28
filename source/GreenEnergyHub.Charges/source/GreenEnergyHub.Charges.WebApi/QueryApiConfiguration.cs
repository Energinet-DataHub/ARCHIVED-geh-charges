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

using System;
using GreenEnergyHub.Charges.QueryApi;
using GreenEnergyHub.Charges.QueryApi.Model;
using GreenEnergyHub.Charges.QueryApi.QueryServices;
using GreenEnergyHub.Iso8601;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GreenEnergyHub.Charges.WebApi
{
    public static class QueryApiConfiguration
    {
        public static IServiceCollection AddQueryApi(
            this IServiceCollection serviceCollection,
            IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString(EnvironmentSettingNames.ChargeDbConnectionString);
            if (connectionString == null)
                throw new ArgumentNullException(EnvironmentSettingNames.ChargeDbConnectionString, "does not exist in configuration settings");

            serviceCollection.AddDbContext<QueryDbContext>(
                options => options.UseSqlServer(connectionString));

            serviceCollection.AddScoped<IData, Data>();
            serviceCollection.AddScoped<IChargesQueryService, ChargesQueryService>();
            serviceCollection.AddScoped<IMarketParticipantQueryService, MarketParticipantQueryService>();
            serviceCollection.AddScoped<IChargePriceQueryService, ChargePriceQueryService>();
            serviceCollection.AddScoped<IChargeMessageQueryService, ChargeMessageQueryService>();

            ConfigureIso8601Services(serviceCollection, configuration);

            return serviceCollection;
        }

        private static void ConfigureIso8601Services(IServiceCollection serviceCollection, IConfiguration configuration)
        {
            var timeZoneId = configuration.GetValue<string>(EnvironmentSettingNames.LocalTimeZoneName);
            var timeZoneConfiguration = new Iso8601ConversionConfiguration(timeZoneId);
            serviceCollection.AddSingleton<IIso8601ConversionConfiguration>(timeZoneConfiguration);
            serviceCollection.AddSingleton<IIso8601Durations, Iso8601Durations>();
        }
    }
}
