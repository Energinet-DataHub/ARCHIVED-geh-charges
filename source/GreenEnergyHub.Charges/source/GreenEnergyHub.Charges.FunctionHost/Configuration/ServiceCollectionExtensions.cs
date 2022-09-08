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

using Azure.Messaging.ServiceBus;
using Energinet.DataHub.Core.App.Common;
using Energinet.DataHub.Core.App.Common.Abstractions.Actor;
using Energinet.DataHub.Core.App.Common.Abstractions.Identity;
using Energinet.DataHub.Core.App.Common.Abstractions.Security;
using Energinet.DataHub.Core.App.Common.Identity;
using Energinet.DataHub.Core.App.Common.Security;
using Energinet.DataHub.Core.App.FunctionApp.Middleware;
using GreenEnergyHub.Charges.Application.Charges.Events;
using GreenEnergyHub.Charges.Application.Messaging;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommandAcceptedEvents;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommandReceivedEvents;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommandRejectedEvents;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeLinksAcceptedEvents;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeLinksDataAvailableNotifiedEvents;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeLinksReceivedEvents;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeLinksRejectionEvents;
using GreenEnergyHub.Charges.Domain.Dtos.ChargePriceCommandReceivedEvents;
using GreenEnergyHub.Charges.FunctionHost.Common;
using GreenEnergyHub.Charges.Infrastructure;
using GreenEnergyHub.Charges.Infrastructure.Core.InternalMessaging;
using GreenEnergyHub.Charges.Infrastructure.Core.MessagingExtensions;
using GreenEnergyHub.Charges.Infrastructure.Core.Registration;
using Microsoft.Extensions.DependencyInjection;

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
        /// <param name="serviceCollection">ServiceCollection container</param>
        public static void AddJwtTokenSecurity(this IServiceCollection serviceCollection)
        {
            var tenantId = EnvironmentHelper.GetEnv(EnvironmentSettingNames.B2CTenantId);
            var audience = EnvironmentHelper.GetEnv(EnvironmentSettingNames.BackendServiceAppId);
            var metadataAddress = $"https://login.microsoftonline.com/{tenantId}/v2.0/.well-known/openid-configuration";

            serviceCollection.AddScoped<JwtTokenMiddleware>(_ => new JwtTokenMiddleware(
                _.GetRequiredService<ClaimsPrincipalContext>(),
                _.GetRequiredService<IJwtTokenValidator>(),
                _functionNamesToExclude));
            serviceCollection.AddScoped<IJwtTokenValidator, JwtTokenValidator>();
            serviceCollection.AddScoped<IClaimsPrincipalAccessor, ClaimsPrincipalAccessor>();
            serviceCollection.AddScoped<ClaimsPrincipalContext>();
            serviceCollection.AddScoped(_ => new OpenIdSettings(metadataAddress, audience));
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

        public static void AddEventPublishing(this IServiceCollection serviceCollection, ServiceBusClient serviceBusClient)
        {
            var mapper = new ServiceBusEventMapper();
            mapper.Add(typeof(ChargeCommandReceivedEvent), EnvironmentHelper.GetEnv(EnvironmentSettingNames.CommandReceivedTopicName));
            mapper.Add(typeof(ChargeCommandAcceptedEvent), EnvironmentHelper.GetEnv(EnvironmentSettingNames.CommandAcceptedTopicName));
            mapper.Add(typeof(ChargeCommandRejectedEvent), EnvironmentHelper.GetEnv(EnvironmentSettingNames.CommandRejectedTopicName));
            mapper.Add(typeof(ChargeLinksAcceptedEvent), EnvironmentHelper.GetEnv(EnvironmentSettingNames.ChargeLinksAcceptedTopicName));
            mapper.Add(typeof(ChargeLinksReceivedEvent), EnvironmentHelper.GetEnv(EnvironmentSettingNames.ChargeLinksReceivedTopicName));
            mapper.Add(typeof(ChargeLinksRejectedEvent), EnvironmentHelper.GetEnv(EnvironmentSettingNames.ChargeLinksRejectedTopicName));
            mapper.Add(typeof(ChargePriceCommandReceivedEvent), EnvironmentHelper.GetEnv(EnvironmentSettingNames.PriceCommandReceivedTopicName));
            mapper.Add(typeof(ChargeLinksDataAvailableNotifiedEvent), EnvironmentHelper.GetEnv(EnvironmentSettingNames.DefaultChargeLinksDataAvailableNotifiedTopicName));
            mapper.Add(typeof(PriceConfirmedEvent), EnvironmentHelper.GetEnv(EnvironmentSettingNames.ChargePriceConfirmedTopicName));
            mapper.Add(typeof(PriceRejectedEvent), EnvironmentHelper.GetEnv(EnvironmentSettingNames.ChargePriceRejectedTopicName));

            serviceCollection.AddScoped<IInternalEventDispatcher, InternalEventDispatcher>();
            serviceCollection.AddScoped<IServiceBusDispatcher, ServiceBusDispatcher>();

            // Must be a singleton as per documentation of ServiceBusClient and ServiceBusSender
            serviceCollection.AddSingleton<IServiceBusDispatcher>(
                _ => new ServiceBusDispatcher(serviceBusClient, mapper));
        }
    }
}
