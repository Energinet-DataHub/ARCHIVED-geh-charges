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
using Energinet.DataHub.Charges.Clients.ChargeLinks;
using Energinet.DataHub.Charges.Clients.Registration.ChargeLinks.ServiceCollectionExtensions;
using FluentAssertions;
using GreenEnergyHub.Charges.TestCore;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.Charges.Clients.RegistrationTests.ChargeLinks
{
    [UnitTest]
    public class ServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddChargeLinksClient_WhenAdded_IsRegistered()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddScoped<IHttpContextAccessor>(_ => new HttpContextAccessorMock("fake token"));

            // Act
            serviceCollection.AddChargeLinksClient(new Uri("http://chargelinks-test.com/"));
            var provider = serviceCollection.BuildServiceProvider();
            var chargeLinkClient = provider.GetRequiredService<IChargeLinksClient>();

            // Assert
            serviceCollection.Should().ContainSingle(x => x.ServiceType == typeof(IChargeLinksClient) && x.Lifetime == ServiceLifetime.Scoped);
            chargeLinkClient.Should().NotBeNull();
            chargeLinkClient.Should().BeOfType<ChargeLinksClient>();
        }
    }
}
