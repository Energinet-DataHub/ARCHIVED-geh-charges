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
using Energinet.DataHub.Core.FunctionApp.TestCommon;
using FluentAssertions;
using GreenEnergyHub.Charges.IntegrationTest.Core.Fixtures.FunctionApp;
using GreenEnergyHub.Charges.IntegrationTest.Core.TestFiles.ChargeLinks;
using GreenEnergyHub.Charges.IntegrationTest.Core.TestHelpers;
using GreenEnergyHub.Charges.IntegrationTests.Fixtures;
using Xunit;
using Xunit.Abstractions;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.IntegrationTests.DomainTests
{
    [IntegrationTest]
    public class ChargeLinkIngestionTests
    {
        [Collection(nameof(ChargesFunctionAppCollectionFixture))]
        public class RunAsync : FunctionAppTestBase<ChargesFunctionAppFixture>, IAsyncLifetime
        {
            private const string EndpointUrl = "api/ChargeLinksIngestion";
            private readonly AuthenticatedHttpRequestGenerator _authenticatedHttpRequestGenerator;

            public RunAsync(ChargesFunctionAppFixture fixture, ITestOutputHelper testOutputHelper)
                : base(fixture, testOutputHelper)
            {
                _authenticatedHttpRequestGenerator = new AuthenticatedHttpRequestGenerator(fixture.AuthorizationConfiguration);
            }

            public Task InitializeAsync()
            {
                return Task.CompletedTask;
            }

            public Task DisposeAsync()
            {
                Fixture.MessageHubMock.Clear();
                return Task.CompletedTask;
            }

            [Fact]
            public async Task When_RequestIsUnauthenticated_Then_AHttp401UnauthorizedIsReturned()
            {
                var (request, _) = HttpRequestGenerator.CreateHttpPostRequest(EndpointUrl, ChargeLinkDocument.AnyValid);

                var actual = await Fixture.HostManager.HttpClient.SendAsync(request);

                actual.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            }

            [Fact]
            public async Task Given_NewMessage_When_SenderIdDoesNotMatchAuthenticatedId_Then_ShouldReturnErrorMessage()
            {
                // Arrange
                var (request, _) = await _authenticatedHttpRequestGenerator
                    .CreateAuthenticatedHttpPostRequestAsync(
                        EndpointUrl, ChargeLinkDocument.ChargeLinkDocumentWhereSenderIdDoNotMatchAuthorizedActorId);

                // Act
                var actual = await Fixture.HostManager.HttpClient.SendAsync(request);

                // Assert
                actual.StatusCode.Should().Be(HttpStatusCode.BadRequest);
                var errorMessage = await actual.Content.ReadAsStringAsync();
                errorMessage.Should().Be("The sender organization provided in the request body does not match the organization in the bearer token.");
            }

            [Fact]
            public async Task When_ChargeLinkIsReceived_Then_AHttp202ResponseWithEmptyBodyIsReturned()
            {
                var result = await _authenticatedHttpRequestGenerator.CreateAuthenticatedHttpPostRequestAsync(
                    EndpointUrl, ChargeLinkDocument.AnyValid);

                var actualResponse = await Fixture.HostManager.HttpClient.SendAsync(result.Request);
                var responseBody = await actualResponse.Content.ReadAsStringAsync();

                actualResponse.StatusCode.Should().Be(HttpStatusCode.Accepted);
                responseBody.Should().BeEmpty();
            }

            [Fact]
            public async Task When_InvalidChargeLinkIsReceived_Then_AHttp400ResponseIsReturned()
            {
                // Arrange
                var result = await _authenticatedHttpRequestGenerator.CreateAuthenticatedHttpPostRequestAsync(
                    EndpointUrl, ChargeLinkDocument.InvalidSchema);

                // Act
                var actualResponse = await Fixture.HostManager.HttpClient.SendAsync(result.Request);

                // Assert
                actualResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            }

            [Fact]
            public async Task Given_NewTaxChargeLinkMessage_When_GridAccessProviderPeeks_Then_MessageHubReceivesReply()
            {
                // Arrange
                var (request, correlationId) = await _authenticatedHttpRequestGenerator.CreateAuthenticatedHttpPostRequestAsync(
                    EndpointUrl, ChargeLinkDocument.TaxWithCreateAndUpdateDueToOverLappingPeriod);

                // Act
                await Fixture.HostManager.HttpClient.SendAsync(request);

                // Assert
                // We expect 3 message types in the MessageHub, one for the receipt,
                // one for the charge link itself and one rejected
                await Fixture.MessageHubMock.AssertPeekReceivesReplyAsync(correlationId, 3);
            }
        }
    }
}
