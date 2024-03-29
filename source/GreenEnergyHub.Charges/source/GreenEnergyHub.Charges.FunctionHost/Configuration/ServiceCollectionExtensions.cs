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
using Azure.Messaging.ServiceBus;
using Energinet.DataHub.Core.App.Common;
using Energinet.DataHub.Core.App.Common.Abstractions.Actors;
using Energinet.DataHub.Core.App.Common.Abstractions.Identity;
using Energinet.DataHub.Core.App.Common.Abstractions.Security;
using Energinet.DataHub.Core.App.Common.Identity;
using Energinet.DataHub.Core.App.Common.Security;
using Energinet.DataHub.Core.App.FunctionApp.Middleware;
using GreenEnergyHub.Charges.Application.Messaging;
using GreenEnergyHub.Charges.FunctionHost.Common;
using GreenEnergyHub.Charges.Infrastructure;
using GreenEnergyHub.Charges.Infrastructure.Core.InternalMessaging;
using GreenEnergyHub.Charges.Infrastructure.Core.MessagingExtensions;
using GreenEnergyHub.Charges.Infrastructure.Core.Registration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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
        public static IServiceCollection AddJwtTokenSecurity(this IServiceCollection services)
        {
            services.AddSingleton<ISecurityTokenValidator, JwtSecurityTokenHandler>();
            services.AddSingleton<IConfigurationManager<OpenIdConnectConfiguration>>(_ =>
            {
                var tenantId = EnvironmentHelper.GetEnv(EnvironmentSettingNames.B2CTenantId);
                var metadataAddress =
                    $"https://login.microsoftonline.com/{tenantId}/v2.0/.well-known/openid-configuration";
                return new ConfigurationManager<OpenIdConnectConfiguration>(
                    metadataAddress,
                    new OpenIdConnectConfigurationRetriever());
            });

            services.AddScoped<IJwtTokenValidator>(sp =>
            {
                var audience = EnvironmentHelper.GetEnv(EnvironmentSettingNames.BackendServiceAppId);
                return new JwtTokenValidator(
                    sp.GetRequiredService<ILogger<JwtTokenValidator>>(),
                    sp.GetRequiredService<ISecurityTokenValidator>(),
                    sp.GetRequiredService<IConfigurationManager<OpenIdConnectConfiguration>>(),
                    audience);
            });

            services.AddScoped<ClaimsPrincipalContext>();
            services.AddScoped<IClaimsPrincipalAccessor, ClaimsPrincipalAccessor>();

            services.AddScoped(sp =>
                new JwtTokenMiddleware(
                    sp.GetRequiredService<ClaimsPrincipalContext>(),
                    sp.GetRequiredService<IJwtTokenValidator>(),
                    _functionNamesToExclude));

            return services;
        }

        /// <summary>
        /// Adds registration of ActorMiddleware, ActorContext and ActorProvider.
        /// </summary>
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

        public static void AddDomainEventPublishing(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();

            // Must be a singleton as per documentation of ServiceBusClient and ServiceBusSender
            serviceCollection.AddSingleton<IServiceBusDispatcher>(
                sp =>
                {
                    var topicName = EnvironmentHelper.GetEnv(EnvironmentSettingNames.ChargesDomainEventTopicName);
                    var serviceBusClient = sp.GetRequiredService<ServiceBusClient>();
                    return new ServiceBusDispatcher(serviceBusClient, topicName);
                });
        }
    }
}
