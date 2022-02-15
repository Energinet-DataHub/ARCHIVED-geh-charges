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
using System.Net.Http;
using Energinet.DataHub.Charges.Clients.ChargeLinks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Energinet.DataHub.Charges.Clients.Registration.ChargeLinks.ServiceCollectionExtensions
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// ServiceCollection extension for registering the ChargeLinkClient.
        /// </summary>
        /// <param name="services">Service collection</param>
        /// <param name="baseUri">The base URL for the Charges domain</param>
        /// <returns>Service Collection with ChargeLinkClient</returns>
        public static IServiceCollection AddChargeLinksClient(this IServiceCollection services, Uri baseUri)
        {
            services.AddHttpContextAccessor();
            services.AddScoped<IChargeLinksClient>(x => new ChargeLinksClientFactory(
                    x.GetRequiredService<IHttpClientFactory>(),
                    x.GetRequiredService<IHttpContextAccessor>())
                .CreateClient(baseUri));

            if (services.Any(x => x.ServiceType == typeof(IHttpClientFactory)))
                return services;

            services.AddHttpClient();

            return services;
        }
    }
}
