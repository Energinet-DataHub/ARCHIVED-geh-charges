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
using GreenEnergyHub.Charges.TestCore.Attributes;
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
                Fixture.ChargeCreatedListener.Reset();
                Fixture.ChargePricesUpdatedListener.Reset();
                Fixture.MessageHubMock.Clear();
                return Task.CompletedTask;
            }

            [Fact]
            public async Task When_RequestIsUnauthenticated_Then_AHttp401UnauthorizedIsReturned()
            {
                var (request, _) = HttpRequestGenerator.CreateHttpPostRequest(EndpointUrl, ChargeDocument.TariffBundleWithValidAndInvalid);

                var actual = await Fixture.HostManager.HttpClient.SendAsync(request);

                actual.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            }

            [Fact]
            public async Task When_ChargeIsReceived_Then_AHttp202ResponseWithEmptyBodyIsReturned()
            {
                var request = await _authenticatedHttpRequestGenerator
                    .CreateAuthenticatedHttpPostRequestAsync(EndpointUrl, ChargeDocument.AnyValid);

                var actualResponse = await Fixture.HostManager.HttpClient.SendAsync(request.Request);
                var responseBody = await actualResponse.Content.ReadAsStringAsync();

                actualResponse.StatusCode.Should().Be(HttpStatusCode.Accepted);
                responseBody.Should().BeEmpty();
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
                    .CreateAuthenticatedHttpPostRequestAsync(EndpointUrl, ChargeDocument.TariffBundleWithCreateAndUpdate);
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
                var actual = await Fixture.HostManager.HttpClient.SendAsync(request);

                // Assert
                actual.StatusCode.Should().Be(HttpStatusCode.Accepted);

                // We expect three peeks:
                // * one for the two confirmations
                // * one for the notification (tax)
                // * one for the rejection (ChargeIdLengthValidation)
                await Fixture.MessageHubMock.AssertPeekReceivesReplyAsync(correlationId, 3);
            }

            [Fact]
            public async Task Given_NewMessage_When_SenderIdDoesNotMatchAuthenticatedId_Then_ShouldReturnErrorMessage()
            {
                // Arrange
                var (request, correlationId) = await _authenticatedHttpRequestGenerator
                    .CreateAuthenticatedHttpPostRequestAsync(EndpointUrl, ChargeDocument.ChargeDocumentWithWhereSenderIdDoNotMatchAuthorizedActorId);

                // Act
                var actual = await Fixture.HostManager.HttpClient.SendAsync(request);

                // Assert
                actual.StatusCode.Should().Be(HttpStatusCode.BadRequest);
                var errorMessage = await actual.Content.ReadAsStringAsync();
                errorMessage.Should().Be("The sender organization provided in the request body does not match the organization in the bearer token.");
            }

            [Theory]
            [InlineAutoMoqData(ChargeDocument.SubscriptionMonthlyPriceSample)]
            [InlineAutoMoqData(ChargeDocument.FeeMonthlyPriceSample)]
            [InlineAutoMoqData(ChargeDocument.TariffHourlyPricesSample)]
            public async Task Given_ChargeExampleFileWithPrices_When_GridAccessProviderPeeks_Then_MessageHubReceivesReply(
                string testFilePath)
            {
                // Arrange
                var (request, correlationId) = await _authenticatedHttpRequestGenerator
                    .CreateAuthenticatedHttpPostRequestAsync(EndpointUrl, testFilePath);

                // Act
                await Fixture.HostManager.HttpClient.SendAsync(request);

                // Act and assert
                await Fixture.MessageHubMock.AssertPeekReceivesReplyAsync(correlationId);
            }

            [Fact]
            public async Task Given_BundleWithMultipleOperationsForSameCharge_When_GridAccessProviderPeeks_Then_MessageHubReceivesReply()
            {
                // Arrange
                var (request, correlationId) = await _authenticatedHttpRequestGenerator
                    .CreateAuthenticatedHttpPostRequestAsync(EndpointUrl, ChargeDocument.BundleWithMultipleOperationsForSameTariff);

                // Act
                var actual = await Fixture.HostManager.HttpClient.SendAsync(request);

                // Assert
                actual.StatusCode.Should().Be(HttpStatusCode.Accepted);

                // We expect four peeks:
                // * one for the create confirmation (create)
                // * one for the create confirmation (update)
                // * one for the create confirmation (stop)
                // * one for the create confirmation (cancel stop)
                await Fixture.MessageHubMock.AssertPeekReceivesReplyAsync(correlationId, 4);
            }

            [Fact]
            public async Task Given_BundleWithTwoOperationsForSameTariffSecondOpViolatingVR903_When_GridAccessProviderPeeks_Then_MessageHubReceivesReply()
            {
                // Arrange
                var (request, correlationId) = await _authenticatedHttpRequestGenerator
                    .CreateAuthenticatedHttpPostRequestAsync(EndpointUrl, ChargeDocument.BundleWithTwoOperationsForSameTariffSecondOpViolatingVr903);

                // Act
                var actual = await Fixture.HostManager.HttpClient.SendAsync(request);

                // Assert
                actual.StatusCode.Should().Be(HttpStatusCode.Accepted);

                // We expect two peeks:
                // * one for the confirmation (first operation)
                // * one for the rejection (second operation violating VR.903)
                await Fixture.MessageHubMock.AssertPeekReceivesReplyAsync(correlationId, 2);
            }

            [Fact(Skip = "Used for debugging ChargeCommandReceivedEventHandler")]
            public async Task Given_UpdateRequest_When_GridAccessProviderPeeks_Then_MessageHubReceivesReply()
            {
                // Arrange - Create
                var (createReq, correlationId) = await _authenticatedHttpRequestGenerator
                    .CreateAuthenticatedHttpPostRequestAsync(EndpointUrl, ChargeDocument.CreateTariff);
                var response = await Fixture.HostManager.HttpClient.SendAsync(createReq);
                response.StatusCode.Should().Be(HttpStatusCode.Accepted);
                await Fixture.MessageHubMock.AssertPeekReceivesReplyAsync(correlationId);

                // Arrange - Update
                var (updateReq, updateCorrelationId) = await _authenticatedHttpRequestGenerator
                    .CreateAuthenticatedHttpPostRequestAsync(EndpointUrl, ChargeDocument.UpdateTariff);

                // Act
                var actual = await Fixture.HostManager.HttpClient.SendAsync(updateReq);

                // Assert
                actual.StatusCode.Should().Be(HttpStatusCode.Accepted);

                // * Expect one for the confirmation (update)
                await Fixture.MessageHubMock.AssertPeekReceivesReplyAsync(updateCorrelationId);
            }
        }
    }
}
