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
using System.Linq;
using Energinet.DataHub.Core.App.Common.Identity;
using Energinet.DataHub.Core.App.Common.Security;
using Energinet.DataHub.Core.App.WebApp.Middleware;
using GreenEnergyHub.Charges.WebApi;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace GreenEnergyHub.Charges.IntegrationTests.Fixtures.WebApi
{
    public class WebApiFactory : WebApplicationFactory<Startup>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            builder.ConfigureServices(services =>
            {
                // This can be used for changing registrations in the container (e.g. for mocks).
               // RemoveJwtTokenSecurity(services);
            });
        }

        /// <summary>
        /// Removes registrations of JwtTokenMiddleware and corresponding dependencies.
        /// </summary>
        /// <param name="serviceCollection">ServiceCollection container</param>
        private static void RemoveJwtTokenSecurity(IServiceCollection serviceCollection)
        {
            var jwtTokenMiddleware = serviceCollection.First(x => x.ImplementationType == typeof(JwtTokenMiddleware));
            serviceCollection.Remove(jwtTokenMiddleware);

            var jwtTokenValidator = serviceCollection.First(x => x.ImplementationType == typeof(JwtTokenValidator));
            serviceCollection.Remove(jwtTokenValidator);

            var claimsPrincipalAccessor = serviceCollection.First(x => x.ImplementationType == typeof(ClaimsPrincipalAccessor));
            serviceCollection.Remove(claimsPrincipalAccessor);

            var claimsPrincipalContext = serviceCollection.First(x => x.ImplementationType == typeof(ClaimsPrincipalContext));
            serviceCollection.Remove(claimsPrincipalContext);
        }
    }
}
