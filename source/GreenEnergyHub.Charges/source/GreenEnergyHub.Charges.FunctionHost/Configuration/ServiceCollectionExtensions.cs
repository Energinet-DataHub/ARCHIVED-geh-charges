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

using Energinet.DataHub.Core.FunctionApp.Common;
using Energinet.DataHub.Core.FunctionApp.Common.Abstractions.Actor;
using Energinet.DataHub.Core.FunctionApp.Common.Abstractions.Identity;
using Energinet.DataHub.Core.FunctionApp.Common.Identity;
using Energinet.DataHub.Core.FunctionApp.Common.Middleware;
using GreenEnergyHub.Charges.FunctionHost.Common;
using GreenEnergyHub.Charges.Infrastructure;
using GreenEnergyHub.Charges.Infrastructure.Core.Authentication;
using Microsoft.Extensions.DependencyInjection;

namespace GreenEnergyHub.Charges.FunctionHost.Configuration
{
    public static class ContainerExtensions
    {
        /// <summary>
        /// Adds registrations of JwtTokenMiddleware and corresponding dependencies.
        /// </summary>
        /// <param name="serviceCollection">ServiceCollection container</param>
        public static void AddJwtTokenSecurity(this IServiceCollection serviceCollection)
        {
            var tenantId = EnvironmentHelper.GetEnv(EnvironmentSettingNames.B2CTenantId);
            var audience = EnvironmentHelper.GetEnv(EnvironmentSettingNames.BackendServiceAppId);
            var metadataAddress = $"https://login.microsoftonline.com/{tenantId}/v2.0/.well-known/openid-configuration";

            serviceCollection.AddScoped<JwtTokenMiddleware>();
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
            serviceCollection.AddScoped<JwtTokenWrapperMiddleware>();
            serviceCollection.AddScoped<ActorMiddleware>();
            serviceCollection.AddScoped<IActorContext, ActorContext>();
            serviceCollection.AddScoped<IActorProvider, ActorProvider>();
        }
    }
}
