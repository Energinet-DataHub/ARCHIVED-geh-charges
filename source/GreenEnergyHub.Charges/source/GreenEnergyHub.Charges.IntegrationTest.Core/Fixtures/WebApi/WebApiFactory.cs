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
using System.Security.Claims;
using Energinet.DataHub.Core.App.Common.Abstractions.Security;
using GreenEnergyHub.Charges.WebApi;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace GreenEnergyHub.Charges.IntegrationTest.Core.Fixtures.WebApi
{
    public class WebApiFactory : WebApplicationFactory<Startup>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            // This can be used for changing registrations in the container (e.g. for mocks).
            builder.ConfigureServices(services =>
            {
                UnregisterService<IJwtTokenValidator>(services);
                services.AddScoped<IJwtTokenValidator>(sp =>
                {
                    var jwtTokenValidatorMock = new Mock<IJwtTokenValidator>();

                    var claims = new ClaimsPrincipal();
                    jwtTokenValidatorMock
                        .Setup(m => m.ValidateTokenAsync(It.IsAny<string>()))
                        .ReturnsAsync((IsValid: true, ClaimsPrincipal: claims));

                    return jwtTokenValidatorMock.Object;
                });

                // var sp = services.BuildServiceProvider();
                // using var scope = sp.CreateScope();
                // var scopedServices = scope.ServiceProvider;
            });
        }

        private static void UnregisterService<TService>(IServiceCollection services)
        {
            var descriptor = services.Single(d => d.ServiceType == typeof(TService));
            services.Remove(descriptor);
        }
    }
}
