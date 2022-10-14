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
using Energinet.DataHub.Charges.Clients.Charges;
using Energinet.DataHub.Charges.Clients.CreateDefaultChargeLink.Tests;
using Energinet.DataHub.Charges.Clients.Registration.Charges.ServiceCollectionExtensions;
using FluentAssertions;
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
        public void AddChargesClient_WhenAdded_IsRegistered()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddScoped<IHttpContextAccessor>(_ => new HttpContextAccessorMock("fake token"));

            // Act
            serviceCollection.AddChargesClient(new Uri("http://charges-test.com/"));
            var provider = serviceCollection.BuildServiceProvider();
            var chargeClient = provider.GetRequiredService<IChargesClient>();

            // Assert
            serviceCollection.Should().ContainSingle(x => x.ServiceType == typeof(IChargesClient) && x.Lifetime == ServiceLifetime.Scoped);
            chargeClient.Should().NotBeNull();
            chargeClient.Should().BeOfType<ChargesClient>();
        }
    }
}
