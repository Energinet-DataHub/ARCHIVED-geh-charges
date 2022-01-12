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

using System;
using GreenEnergyHub.Charges.IntegrationTests.Fixtures.Database;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace GreenEnergyHub.Charges.IntegrationTests.Fixtures
{
    public class WebApiFactory : WebApplicationFactory<WebApi.Startup>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            builder.ConfigureServices(services =>
            {
                // This can be used for changing registrations in the container (e.g. for mocks).

                // var sp = services.BuildServiceProvider();
                // using var scope = sp.CreateScope();
                // var scopedServices = scope.ServiceProvider;
            });
        }
    }
}
