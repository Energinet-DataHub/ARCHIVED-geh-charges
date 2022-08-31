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

using System.IdentityModel.Tokens.Jwt;
using Energinet.DataHub.Core.App.Common.Abstractions.Identity;
using Energinet.DataHub.Core.App.Common.Abstractions.Security;
using Energinet.DataHub.Core.App.Common.Identity;
using Energinet.DataHub.Core.App.Common.Security;
using Energinet.DataHub.Core.App.WebApp.Middleware;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
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
        /// <param name="serviceCollection">ServiceCollection container</param>
        /// <param name="metadataAddress">OpenID Configuration URL used for acquiring metadata</param>
        /// <param name="audience">Audience used for validation of JWT token</param>
        public static void AddJwtTokenSecurity(this IServiceCollection serviceCollection, string metadataAddress, string audience)
        {
            serviceCollection.TryAddSingleton<ISecurityTokenValidator, JwtSecurityTokenHandler>();
            serviceCollection.TryAddSingleton<IConfigurationManager<OpenIdConnectConfiguration>>(_ =>
                new ConfigurationManager<OpenIdConnectConfiguration>(
                    metadataAddress,
                    new OpenIdConnectConfigurationRetriever()));

            serviceCollection.TryAddScoped<IJwtTokenValidator>(sp =>
                new JwtTokenValidator(
                    sp.GetRequiredService<ISecurityTokenValidator>(),
                    sp.GetRequiredService<IConfigurationManager<OpenIdConnectConfiguration>>(),
                    audience));

            serviceCollection.TryAddScoped<IClaimsPrincipalAccessor, ClaimsPrincipalAccessor>();
            serviceCollection.TryAddScoped<ClaimsPrincipalContext>();

            serviceCollection.TryAddScoped<JwtTokenMiddleware>();
        }
    }
}
