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
using System.Net;
using System.Threading.Tasks;
using Energinet.DataHub.Core.FunctionApp.TestCommon;
using FluentAssertions;
using GreenEnergyHub.Charges.IntegrationTest.Core.Fixtures.FunctionApp;
using GreenEnergyHub.Charges.IntegrationTest.Core.TestFiles.Charges;
using GreenEnergyHub.Charges.IntegrationTest.Core.TestHelpers;
using GreenEnergyHub.Charges.IntegrationTests.Fixtures;
using Xunit;
using Xunit.Abstractions;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.IntegrationTests.DomainTests
{
    [IntegrationTest]
    public class ChargeIngestionTests
    {
        private const string EndpointUrl = "api/ChargeIngestion";
        private const int SecondsToWaitForIntegrationEvents = 15;

        [Collection(nameof(ChargesFunctionAppCollectionFixture))]
        public class RunAsync : FunctionAppTestBase<ChargesFunctionAppFixture>, IAsyncLifetime
        {
            private readonly AuthenticatedHttpRequestGenerator _authenticatedHttpRequestGenerator;
            private readonly HttpRequestGenerator _httpRequestGenerator;

            public RunAsync(ChargesFunctionAppFixture fixture, ITestOutputHelper testOutputHelper)
                : base(fixture, testOutputHelper)
            {
                _httpRequestGenerator = new HttpRequestGenerator();
                TestDataGenerator.GenerateDataForIntegrationTests(fixture);
                _authenticatedHttpRequestGenerator = new AuthenticatedHttpRequestGenerator(
                    _httpRequestGenerator,
                    fixture.AuthorizationConfiguration);
            }

            public Task InitializeAsync()
            {
                return Task.CompletedTask;
            }

            public Task DisposeAsync()
            {
                Fixture.ChargeCreatedListener.Reset();
                Fixture.ChargePricesUpdatedListener.Reset();
                Fixture.MessageHubMock.Clear();
                return Task.CompletedTask;
            }

            [Fact]
            public async Task When_RequestIsUnauthenticated_Then_AHttp401UnauthorizedIsReturned()
            {
                var (request, _) = _httpRequestGenerator
                    .CreateHttpPostRequest(EndpointUrl, ChargeDocument.TariffBundleWithValidAndInvalid);

                var actual = await Fixture.HostManager.HttpClient.SendAsync(request);

                actual.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            }

            [Fact]
            public async Task When_ChargeIsReceived_Then_AHttp202ResponseIsReturned()
            {
                var request = await _authenticatedHttpRequestGenerator
                    .CreateAuthenticatedHttpPostRequestAsync(EndpointUrl, ChargeDocument.AnyValid);

                var actualResponse = await Fixture.HostManager.HttpClient.SendAsync(request.Request);

                actualResponse.StatusCode.Should().Be(HttpStatusCode.Accepted);
            }

            [Fact]
            public async Task When_InvalidChargeIsReceived_Then_AHttp400ResponseIsReturned()
            {
                var request = await _authenticatedHttpRequestGenerator
                    .CreateAuthenticatedHttpPostRequestAsync(EndpointUrl, ChargeDocument.TariffInvalidSchema);
                var actualResponse = await Fixture.HostManager.HttpClient.SendAsync(request.Request);
                actualResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            }

            [Fact]
            public async Task When_ChargeIsReceived_Then_ChargeCreatedIntegrationEventIsPublished()
            {
                // Arrange
                var (request, correlationId) = await _authenticatedHttpRequestGenerator
                    .CreateAuthenticatedHttpPostRequestAsync(EndpointUrl, ChargeDocument.AnyValid);
                using var eventualChargeCreatedEvent = await Fixture
                    .ChargeCreatedListener
                    .ListenForMessageAsync(correlationId)
                    .ConfigureAwait(false);

                // Act
                await Fixture.HostManager.HttpClient.SendAsync(request);

                // Assert
                var isChargeCreatedReceived = eventualChargeCreatedEvent
                    .MessageAwaiter!
                    .Wait(TimeSpan.FromSeconds(SecondsToWaitForIntegrationEvents));
                isChargeCreatedReceived.Should().BeTrue();
            }

            [Fact]
            public async Task When_ChargeBundleWithChargesIncludingPriceIsReceived_Then_ChargePricesUpdatedIntegrationEventsArePublished()
            {
                // Arrange
                var (request, correlationId) = await _authenticatedHttpRequestGenerator
                    .CreateAuthenticatedHttpPostRequestAsync(EndpointUrl, ChargeDocument.TariffBundleWithValidAndInvalid);
                using var eventualChargePriceUpdatedEvent = await Fixture
                    .ChargePricesUpdatedListener
                    .ListenForEventsAsync(correlationId, expectedCount: 2)
                    .ConfigureAwait(false);

                // Act
                await Fixture.HostManager.HttpClient.SendAsync(request);

                // Assert
                var isChargePricesUpdatedReceived = eventualChargePriceUpdatedEvent
                    .CountdownEvent!
                    .Wait(TimeSpan.FromSeconds(SecondsToWaitForIntegrationEvents));
                isChargePricesUpdatedReceived.Should().BeTrue();
            }

            [Fact]
            public async Task Given_NewTaxBundleTariffWithPrices_When_GridAccessProviderPeeks_Then_MessageHubReceivesReply()
            {
                // Arrange
                var (request, correlationId) = await _authenticatedHttpRequestGenerator
                    .CreateAuthenticatedHttpPostRequestAsync(EndpointUrl, ChargeDocument.TariffBundleWithValidAndInvalid);

                // Act
                await Fixture.HostManager.HttpClient.SendAsync(request);

                // Act and assert
                // We expect three peaks, one for the charge and one for the receipt and one rejection
                await Fixture.MessageHubMock.AssertPeekReceivesReplyAsync(correlationId, 3);
            }
        }
    }
}
