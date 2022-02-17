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
using System.Net.Http;
using Energinet.DataHub.Charges.Clients.ChargeLinks;
using FluentAssertions;
using GreenEnergyHub.Charges.TestCore;
using GreenEnergyHub.Charges.TestCore.Attributes;
using Xunit;

namespace Energinet.DataHub.Charges.Clients.CreateDefaultChargeLink.Tests.ChargeLinks
{
    public class ChargeLinksClientFactoryTests
    {
        [Theory]
        [InlineAutoMoqData]
        public void CreateClient_WithHttpClient_ContainingAuthorizationHeaderFromHttpContextAccessor(
            IHttpClientFactory httpClientFactory)
        {
            // Arrange
            var httpContextAccessor = new HttpContextAccessorMock("fake token");
            var sut = new ChargeLinksClientFactory(httpClientFactory, httpContextAccessor);

            // Act
            var result = sut.CreateClient(new HttpClient());

            // Assert
            result.Should().NotBeNull();
        }

        [Theory]
        [InlineAutoMoqData]
        public void CreateClient_WithUri_ContainingAuthorizationHeaderFromHttpContextAccessor(
            IHttpClientFactory httpClientFactory)
        {
            // Arrange
            var httpContextAccessor = new HttpContextAccessorMock("fake token");
            var sut = new ChargeLinksClientFactory(httpClientFactory, httpContextAccessor);

            // Act
            var result = sut.CreateClient(new Uri("http://some.uri"));

            // Assert
            result.Should().NotBeNull();
        }
    }
}
