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

using Energinet.DataHub.Core.App.WebApp.Authentication;
using Energinet.DataHub.Core.App.WebApp.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GreenEnergyHub.Charges.WebApi.Configuration
{
    internal static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds registrations of JwtTokenMiddleware and corresponding dependencies.
        /// </summary>
        /// ///
        /// <param name="services">ServiceCollection container</param>
        /// <param name="configuration">Configuration containing application properties</param>
        public static IServiceCollection AddJwtTokenSecurity(this IServiceCollection services, IConfiguration configuration)
        {
            var externalOpenIdUrl = configuration.GetValue<string>(EnvironmentSettingNames.ExternalOpenIdUrl);
            var internalOpenIdUrl = configuration.GetValue<string>(EnvironmentSettingNames.InternalOpenIdUrl);
            var backendAppId = configuration.GetValue<string>(EnvironmentSettingNames.BackendAppId);

            services.AddJwtBearerAuthentication(externalOpenIdUrl, internalOpenIdUrl, backendAppId);
            services.AddPermissionAuthorization();

            return services;
        }
    }
}
