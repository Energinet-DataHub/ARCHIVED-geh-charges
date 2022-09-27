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
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Energinet.DataHub.Core.FunctionApp.TestCommon;
using Energinet.DataHub.Core.TestCommon.AutoFixture.Attributes;
using FluentAssertions;
using FluentAssertions.Execution;
using GreenEnergyHub.Charges.IntegrationTest.Core.Fixtures.FunctionApp;
using GreenEnergyHub.Charges.IntegrationTest.Core.MessageHub;
using GreenEnergyHub.Charges.IntegrationTest.Core.TestFiles.Charges;
using GreenEnergyHub.Charges.IntegrationTest.Core.TestHelpers;
using GreenEnergyHub.Charges.IntegrationTests.Fixtures;
using Xunit;
using Xunit.Abstractions;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.IntegrationTests.DomainTests
{
    [IntegrationTest]
    public class ChargeIngestionTestChargeInformation
    {
        private const string EndpointUrl = "api/ChargeIngestion";
        private const int SecondsToWaitForIntegrationEvents = 15;

        [Collection(nameof(ChargesFunctionAppCollectionFixture))]
        public class RunAsync : FunctionAppTestBase<ChargesFunctionAppFixture>, IAsyncLifetime
        {
            public RunAsync(ChargesFunctionAppFixture fixture, ITestOutputHelper testOutputHelper)
                : base(fixture, testOutputHelper)
            {
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

                // We need to clear host log after each test is done to ensure that we can assert on function executed
                // on each test run because we only check on function name.
                Fixture.HostManager.ClearHostLog();
                return Task.CompletedTask;
            }

            [Fact]
            public async Task When_ChargeIsReceived_Then_AHttp202ResponseWithEmptyBodyIsReturned()
            {
                var request = Fixture.AsGridAccessProvider.PrepareHttpPostRequestWithAuthorization(
                    EndpointUrl, ChargeDocument.AnyValid);

                var actualResponse = await Fixture.HostManager.HttpClient.SendAsync(request.Request);
                var responseBody = await actualResponse.Content.ReadAsStringAsync();

                actualResponse.StatusCode.Should().Be(HttpStatusCode.Accepted);
                responseBody.Should().BeEmpty();
            }

            [Fact]
            public async Task When_ChargeInformationIsReceived_Then_ChargeCreatedIntegrationEventIsPublished()
            {
                // Arrange
                var (request, correlationId) = Fixture.AsSystemOperator.PrepareHttpPostRequestWithAuthorization(
                    EndpointUrl, ChargeDocument.TariffBundleWithCreateAndUpdate);
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
            public async Task Given_NewMessage_When_SenderIdDoesNotMatchAuthenticatedId_Then_ShouldReturnErrorMessage()
            {
                // Arrange
                var (request, _) = Fixture.AsGridAccessProvider.PrepareHttpPostRequestWithAuthorization(
                    EndpointUrl, ChargeDocument.ChargeDocumentWhereSenderIdDoNotMatchAuthorizedActorId);

                // Act
                var actual = await Fixture.HostManager.HttpClient.SendAsync(request);

                // Assert
                actual.StatusCode.Should().Be(HttpStatusCode.BadRequest);
                var errorMessage = await actual.Content.ReadAsStreamAsync();
                var document = await XDocument.LoadAsync(errorMessage, LoadOptions.None, CancellationToken.None);
                document.Element("Message")?.Value.Should().Be(ErrorMessageConstants.ActorIsNotWhoTheyClaimToBeErrorMessage);
            }

            [Fact(Skip = "Used for debugging ChargeCommandReceivedEventHandler")]
            public async Task Given_UpdateRequest_When_GridAccessProviderPeeks_Then_MessageHubReceivesReply()
            {
                // Arrange - Create
                var (createReq, correlationId) = Fixture.AsGridAccessProvider.PrepareHttpPostRequestWithAuthorization(
                    EndpointUrl, ChargeDocument.CreateTariff);

                var response = await Fixture.HostManager.HttpClient.SendAsync(createReq);
                response.StatusCode.Should().Be(HttpStatusCode.Accepted);
                await Fixture.MessageHubMock.AssertPeekReceivesRepliesAsync(correlationId);

                // Arrange - Update
                var (updateReq, updateCorrelationId) =
                    Fixture.AsGridAccessProvider.PrepareHttpPostRequestWithAuthorization(
                        EndpointUrl, ChargeDocument.UpdateTariff);

                // Act
                var actual = await Fixture.HostManager.HttpClient.SendAsync(updateReq);

                // Assert
                using var assertionScope = new AssertionScope();
                actual.StatusCode.Should().Be(HttpStatusCode.Accepted);

                // * Expect one for the confirmation (update)
                var peekResults = await Fixture.MessageHubMock.AssertPeekReceivesRepliesAsync(updateCorrelationId);
                peekResults.Should().ContainMatch("*ConfirmRequestChangeOfPriceList_MarketDocument*");
            }

            [Theory]
            [InlineAutoMoqData(ChargeDocument.ChargeInformationSubscriptionMonthlySample, 3, 1)]
            [InlineAutoMoqData(ChargeDocument.ChargeInformationFeeMonthlySample, 3, 1)]
            [InlineAutoMoqData(ChargeDocument.ChargeInformationTariffHourlySample, 3, 1)]
            [InlineAutoMoqData(ChargeDocument.BundledChargeInformationSample, 9, 3)]
            public async Task Given_ChargeInformationSampleFile_When_GridAccessProviderPeeks_Then_MessageHubReceivesReply(
                string testFilePath, int noOfDataAvailableNotificationsExpected, int noOfConfirmedActivityRecordsExpected)
            {
                // Arrange
                var (request, correlationId) =
                    Fixture.AsGridAccessProvider.PrepareHttpPostRequestWithAuthorization(EndpointUrl, testFilePath);

                // Act
                var actual = await Fixture.HostManager.HttpClient.SendAsync(request);

                // Assert
                actual.StatusCode.Should().Be(HttpStatusCode.Accepted);

                using var assertionScope = new AssertionScope();

                // We expect 3 peek results:
                // * 1 confirmation
                // * 2 data available to energy suppliers
                var peekResults = await Fixture.MessageHubMock
                    .AssertPeekReceivesRepliesAsync(correlationId, noOfDataAvailableNotificationsExpected);
                peekResults.Count.Should().Be(3);
                peekResults.Should().ContainMatch("*ConfirmRequestChangeOfPriceList_MarketDocument*");
                peekResults.Should().ContainMatch("*Notify*");
                peekResults.Should().NotContainMatch("*Reject*");

                var peekResult = peekResults.Single(x => x.Contains("ConfirmRequestChangeOfPriceList_MarketDocument"));
                var operations = CIMXmlReader.GetActivityRecords(peekResult);
                operations.Count.Should().Be(noOfConfirmedActivityRecordsExpected);
            }

            [Fact]
            public async Task Given_BundleWithMultipleOperationsForSameCharge_When_GridAccessProviderPeeks_Then_MessageHubReceivesReply()
            {
                // Arrange
                var (request, correlationId) = Fixture.AsGridAccessProvider.PrepareHttpPostRequestWithAuthorization(
                    EndpointUrl, ChargeDocument.BundleWithMultipleOperationsForSameTariff);

                // Act
                var actual = await Fixture.HostManager.HttpClient.SendAsync(request);

                // Assert
                actual.StatusCode.Should().Be(HttpStatusCode.Accepted);

                // We expect four peeks:
                // * one for the create confirmation (create)
                // * one for the create confirmation (update)
                // * one for the create confirmation (stop)
                // * one for the create confirmation (cancel stop)
                using var assertionScope = new AssertionScope();
                var peekResults = await Fixture.MessageHubMock.AssertPeekReceivesRepliesAsync(correlationId, 4);
                peekResults.Should().ContainMatch("*ConfirmRequestChangeOfPriceList_MarketDocument*");
                peekResults.Should().NotContainMatch("*Reject*");
            }

            [Fact]
            public async Task Given_BundleWithTwoOperationsForSameTariffSecondOpViolatingVR903_When_GridAccessProviderPeeks_Then_MessageHubReceivesReply()
            {
                // Arrange
                var (request, correlationId) = Fixture.AsSystemOperator.PrepareHttpPostRequestWithAuthorization(
                    EndpointUrl, ChargeDocument.BundleWithTwoOperationsForSameTariffSecondOpViolatingVr903);

                // Act
                var actual = await Fixture.HostManager.HttpClient.SendAsync(request);

                // Assert
                actual.StatusCode.Should().Be(HttpStatusCode.Accepted);

                // We expect 4 peeks:
                // * 1 for the confirmation (first operation)
                // * 1 for the rejection (second operation violating VR.903)
                // * 2 data available to energy suppliers
                using var assertionScope = new AssertionScope();
                var peekResults = await Fixture.MessageHubMock.AssertPeekReceivesRepliesAsync(correlationId, 4);
                peekResults.Should().ContainMatch("*ConfirmRequestChangeOfPriceList_MarketDocument*");
                peekResults.Should().ContainMatch("*RejectRequestChangeOfPriceList_MarketDocument*");
                peekResults.Should().ContainMatch("*It is not allowed to change the tax indicator to Tax for charge*");
            }

            [Fact]
            public async Task When_TaxTaxIsCreatedBySystemOperator_Then_ANotificationShouldBeReceivedByActiveGridAccessProviders()
            {
                var (request, correlationId) = Fixture.AsSystemOperator.PrepareHttpPostRequestWithAuthorization(
                    EndpointUrl, ChargeDocument.TariffSystemOperatorCreate);

                // Act
                await Fixture.HostManager.HttpClient.SendAsync(request);

                // Assert
                using var assertionScope = new AssertionScope();
                var peekResults = await Fixture.MessageHubMock.AssertPeekReceivesRepliesAsync(correlationId, 6);
                peekResults.Should().NotContainMatch("*RejectRequestChangeOfPriceList_MarketDocument*");
                peekResults.Should().ContainMatch("*NotifyPriceList_MarketDocument*");
                peekResults.Should().ContainMatch("*8100000000030*");
                peekResults.Should().ContainMatch("*8100000000016*");
                peekResults.Should().ContainMatch("*8100000000023*");
                peekResults.Should().NotContainMatch("*8900000000005*");
                peekResults.Should().ContainMatch("*<cim:process.processType>D18</cim:process.processType>*");
                peekResults.Should().NotContainMatch("*<cim:process.processType>D08</cim:process.processType>*");
            }
        }
    }
}
