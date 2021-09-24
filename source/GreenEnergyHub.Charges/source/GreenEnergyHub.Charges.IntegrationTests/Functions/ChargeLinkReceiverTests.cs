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

using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using FluentAssertions.Execution;
using GreenEnergyHub.Charges.IntegrationTests.Functions.Fixtures;
using GreenEnergyHub.FunctionApp.TestCommon;
using Xunit;
using Xunit.Abstractions;

namespace GreenEnergyHub.Charges.IntegrationTests.Functions
{
    /// <summary>
    /// Proof-of-concept on integration testing a function.
    /// </summary>
    public class ChargeLinkReceiverTests
    {
        [Collection(nameof(ChargeLinkReceiverFunctionAppCollectionFixture))]
        public class GetHealthAsync : FunctionAppTestBase<ChargeLinkReceiverFunctionAppFixture>
        {
            public GetHealthAsync(ChargeLinkReceiverFunctionAppFixture fixture, ITestOutputHelper testOutputHelper)
                : base(fixture, testOutputHelper)
            {
            }

            // .NET 5.0 => ChargeLinkReceiver
            [Fact]
            public async Task When_Net50RequestingHealthStatus_Then_ReturnStatusOKAndHealthy()
            {
                // Arrange
                var requestUri = "api/HealthStatus";

                // Act
                var actualResponse = await Fixture.HostManager.HttpClient.GetAsync(requestUri);

                // Assert
                using var assertionScope = new AssertionScope();
                actualResponse.StatusCode.Should().Be(HttpStatusCode.OK);

                var context = await actualResponse.Content.ReadAsStringAsync();
                context.Should().Contain("\"FunctionAppIsAlive\": true");
            }
        }
    }
}
