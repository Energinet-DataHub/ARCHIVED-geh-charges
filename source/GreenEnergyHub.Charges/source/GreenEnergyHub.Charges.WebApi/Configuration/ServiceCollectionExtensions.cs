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

using System.Linq;
using Energinet.DataHub.Core.App.Common.Abstractions.Identity;
using Energinet.DataHub.Core.App.Common.Abstractions.Security;
using Energinet.DataHub.Core.App.Common.Identity;
using Energinet.DataHub.Core.App.Common.Security;
using Energinet.DataHub.Core.App.WebApp.Middleware;
using GreenEnergyHub.Charges.Infrastructure.Core.Registration;
using Microsoft.Extensions.DependencyInjection;

namespace GreenEnergyHub.Charges.WebApi.Configuration
{
    internal static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds registrations of JwtTokenMiddleware and corresponding dependencies.
        /// </summary>
        /// <param name="serviceCollection">ServiceCollection container</param>
        public static void AddJwtTokenSecurity(this IServiceCollection serviceCollection)
        {
            var address = EnvironmentHelper.GetEnv(EnvironmentSettingNames.FrontEndOpenIdUrl);
            var audience = EnvironmentHelper.GetEnv(EnvironmentSettingNames.FrontEndServiceAppId);

            serviceCollection.AddScoped<JwtTokenMiddleware>();
            serviceCollection.AddScoped<IJwtTokenValidator, JwtTokenValidator>();
            serviceCollection.AddScoped<IClaimsPrincipalAccessor, ClaimsPrincipalAccessor>();
            serviceCollection.AddScoped<ClaimsPrincipalContext>();
            serviceCollection.AddScoped(_ => new OpenIdSettings(address, audience));
        }
    }
}
