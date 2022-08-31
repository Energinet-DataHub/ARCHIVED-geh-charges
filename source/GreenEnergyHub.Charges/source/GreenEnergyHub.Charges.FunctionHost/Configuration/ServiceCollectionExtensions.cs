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
using Energinet.DataHub.Core.App.Common;
using Energinet.DataHub.Core.App.Common.Abstractions.Actor;
using Energinet.DataHub.Core.App.Common.Abstractions.Identity;
using Energinet.DataHub.Core.App.Common.Abstractions.Security;
using Energinet.DataHub.Core.App.Common.Identity;
using Energinet.DataHub.Core.App.Common.Security;
using Energinet.DataHub.Core.App.FunctionApp.Middleware;
using GreenEnergyHub.Charges.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

namespace GreenEnergyHub.Charges.FunctionHost.Configuration
{
    internal static class ServiceCollectionExtensions
    {
        private static readonly string[] _functionNamesToExclude =
        {
            "HealthCheck",
        };

        /// <summary>
        /// Adds registrations of JwtTokenMiddleware and corresponding dependencies.
        /// </summary>
        /// <param name="services">ServiceCollection container</param>
        /// <param name="metadataAddress">OpenID Configuration URL used for acquiring metadata</param>
        /// <param name="audience">Audience used for validation of JWT token</param>
        public static IServiceCollection AddJwtTokenSecurity(this IServiceCollection services, string metadataAddress, string audience)
        {
            services.TryAddSingleton<ISecurityTokenValidator, JwtSecurityTokenHandler>();
            services.TryAddSingleton<IConfigurationManager<OpenIdConnectConfiguration>>(_ =>
                new ConfigurationManager<OpenIdConnectConfiguration>(
                    metadataAddress,
                    new OpenIdConnectConfigurationRetriever()));

            services.TryAddScoped<IJwtTokenValidator>(sp =>
                new JwtTokenValidator(
                    sp.GetRequiredService<ISecurityTokenValidator>(),
                    sp.GetRequiredService<IConfigurationManager<OpenIdConnectConfiguration>>(),
                    audience));

            services.TryAddScoped<ClaimsPrincipalContext>();
            services.TryAddScoped<IClaimsPrincipalAccessor, ClaimsPrincipalAccessor>();

            services.TryAddScoped<JwtTokenMiddleware>(sp =>
                new JwtTokenMiddleware(
                    sp.GetRequiredService<ClaimsPrincipalContext>(),
                    sp.GetRequiredService<IJwtTokenValidator>(),
                    _functionNamesToExclude));

            return services;
        }

        /// <summary>
        /// Adds registration of ActorMiddleware, ActorContext and ActorProvider.
        /// </summary>
        /// <param name="serviceCollection">ServiceCollection container</param>
        public static void AddActorContext(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddScoped(_ => new ActorMiddleware(
                _.GetRequiredService<IClaimsPrincipalAccessor>(),
                _.GetRequiredService<IActorProvider>(),
                _.GetRequiredService<IActorContext>(),
                _functionNamesToExclude));
            serviceCollection.AddScoped<IActorContext, ActorContext>();
            serviceCollection.AddScoped<IActorProvider, ActorProvider>();
        }
    }
}
