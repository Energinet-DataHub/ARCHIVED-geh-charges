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

using System.IdentityModel.Tokens.Jwt;
using Energinet.DataHub.Core.App.Common.Abstractions.Identity;
using Energinet.DataHub.Core.App.Common.Abstractions.Security;
using Energinet.DataHub.Core.App.Common.Identity;
using Energinet.DataHub.Core.App.Common.Security;
using Energinet.DataHub.Core.App.WebApp.Middleware;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

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
            services.AddSingleton<ISecurityTokenValidator, JwtSecurityTokenHandler>();
            services.AddSingleton<IConfigurationManager<OpenIdConnectConfiguration>>(_ =>
                {
                    var metadataAddress = configuration.GetValue<string>(EnvironmentSettingNames.FrontEndOpenIdUrl);
                    return new ConfigurationManager<OpenIdConnectConfiguration>(
                        metadataAddress,
                        new OpenIdConnectConfigurationRetriever());
                });

            services.AddScoped<IJwtTokenValidator>(sp =>
            {
                var audience = configuration.GetValue<string>(EnvironmentSettingNames.FrontEndServiceAppId);
                return new JwtTokenValidator(
                    sp.GetRequiredService<ILogger<JwtTokenValidator>>(),
                    sp.GetRequiredService<ISecurityTokenValidator>(),
                    sp.GetRequiredService<IConfigurationManager<OpenIdConnectConfiguration>>(),
                    audience);
            });

            services.AddScoped<ClaimsPrincipalContext>();
            services.AddScoped<IClaimsPrincipalAccessor, ClaimsPrincipalAccessor>();

            services.AddScoped<JwtTokenMiddleware>();

            return services;
        }
    }
}
