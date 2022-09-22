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
using FluentAssertions;
using FluentAssertions.Execution;
using GreenEnergyHub.Charges.Core.DateTime;
using GreenEnergyHub.Charges.Domain.Charges;
using GreenEnergyHub.Charges.FunctionHost.Charges;
using GreenEnergyHub.Charges.Infrastructure.Persistence;
using GreenEnergyHub.Charges.IntegrationTest.Core.Fixtures.FunctionApp;
using GreenEnergyHub.Charges.IntegrationTest.Core.TestCommon;
using GreenEnergyHub.Charges.IntegrationTest.Core.TestFiles.Charges;
using GreenEnergyHub.Charges.IntegrationTest.Core.TestHelpers;
using GreenEnergyHub.Charges.IntegrationTests.Fixtures;
using GreenEnergyHub.Charges.TestCore;
using GreenEnergyHub.Charges.TestCore.Attributes;
using GreenEnergyHub.Iso8601;
using NodaTime.Testing;
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
            public async Task When_RequestIsUnauthenticated_Then_AHttp401UnauthorizedIsReturned()
            {
                var request = HttpRequestGenerator.CreateHttpPostRequest(
                    EndpointUrl, ChargeDocument.TariffBundleWithValidAndInvalid, GetZonedDateTimeService());

                var actual = await Fixture.HostManager.HttpClient.SendAsync(request);

                actual.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
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
            public async Task When_ChargePricesAreReceived_Then_ChargePricesUpdatedIntegrationEventsArePublished()
            {
                // Arrange
                var (request, correlationId) = Fixture.AsGridAccessProvider.PrepareHttpPostRequestWithAuthorization(
                    EndpointUrl, ChargeDocument.TariffPriceSeries);
                using var eventualChargePriceUpdatedEvent = await Fixture
                    .ChargePricesUpdatedListener
                    .ListenForEventsAsync(correlationId, expectedCount: 1)
                    .ConfigureAwait(false);

                // Act
                await Fixture.HostManager.HttpClient.SendAsync(request);

                // Assert
                var isChargePricesUpdatedReceived = eventualChargePriceUpdatedEvent
                    .CountdownEvent!
                    .Wait(TimeSpan.FromSeconds(SecondsToWaitForIntegrationEvents));
                isChargePricesUpdatedReceived.Should().BeTrue();
            }

            // TODO: Let this test evolve in step with the price flow expansion (and change the name accordingly)
            [Fact]
            public async Task When_ChargePricesAreReceived_Then_ChargePriceCommandReceiverEndpointIsTriggered()
            {
                // Arrange
                var (request, correlationId) = Fixture.AsGridAccessProvider.PrepareHttpPostRequestWithAuthorization(
                    EndpointUrl, ChargeDocument.TariffPriceSeries);
                using var eventualChargePriceUpdatedEvent = await Fixture
                    .ChargePricesUpdatedListener
                    .ListenForEventsAsync(correlationId, expectedCount: 1)
                    .ConfigureAwait(false);

                // Act
                await Fixture.HostManager.HttpClient.SendAsync(request);

                // Assert
                await FunctionAsserts.AssertHasExecutedAsync(
                    Fixture.HostManager, nameof(ChargePriceCommandReceiverEndpoint)).ConfigureAwait(false);
            }

            [Fact]
            public async Task Given_NewTaxBundleTariffWithPrices_When_GridAccessProviderPeeks_Then_MessageHubReceivesReply()
            {
                // Arrange
                var (request, correlationId) = Fixture.AsSystemOperator.PrepareHttpPostRequestWithAuthorization(
                    EndpointUrl, ChargeDocument.TariffBundleWithValidAndInvalid);

                // Act
                var actual = await Fixture.HostManager.HttpClient.SendAsync(request);

                // Assert
                actual.StatusCode.Should().Be(HttpStatusCode.Accepted);

                // We expect 20 peek results:
                // * 2 confirmations
                // * 1 rejection (ChargeIdLengthValidation)
                // * 2 x 7 data available to energy suppliers
                // * 3 data available to grid access providers
                using var assertionScope = new AssertionScope();
                var peekResults = await Fixture.MessageHubMock.AssertPeekReceivesRepliesAsync(correlationId, 20);
                peekResults.Should().ContainMatch("*ConfirmRequestChangeOfPriceList_MarketDocument*");
                peekResults.Should().ContainMatch("*RejectRequestChangeOfPriceList_MarketDocument*");
                peekResults.Should().ContainMatch("*NotifyPriceList_MarketDocument*");
            }

            [Theory]
            [InlineAutoMoqData(ChargeDocument.ChargeInformationSubscriptionMonthlySample)]
            [InlineAutoMoqData(ChargeDocument.ChargeInformationFeeMonthlySample)]
            [InlineAutoMoqData(ChargeDocument.ChargeInformationTariffHourlySample)]
            [InlineAutoMoqData(ChargeDocument.BundledChargeInformationSample)]
            public async Task Given_ChargeInformationSampleFile_When_GridAccessProviderPeeks_Then_MessageHubReceivesReply(
                string testFilePath)
            {
                // Arrange
                var (request, correlationId) =
                    Fixture.AsGridAccessProvider.PrepareHttpPostRequestWithAuthorization(EndpointUrl, testFilePath);

                // Act
                var actual = await Fixture.HostManager.HttpClient.SendAsync(request);

                // Assert
                actual.StatusCode.Should().Be(HttpStatusCode.Accepted);

                using var assertionScope = new AssertionScope();

                // We expect 8 peek results:
                // * 1 confirmation
                // * 7 data available to energy suppliers
                var peekResults = await Fixture.MessageHubMock.AssertPeekReceivesRepliesAsync(correlationId, 8);
                peekResults.Should().ContainMatch("*ConfirmRequestChangeOfPriceList_MarketDocument*");
                peekResults.Should().NotContainMatch("*Reject*");
            }

            [Theory]
            [InlineAutoMoqData(ChargeDocument.ChargePriceSeriesSubscriptionMonthlySample)]
            [InlineAutoMoqData(ChargeDocument.ChargePriceSeriesFeeMonthlySample)]
            [InlineAutoMoqData(ChargeDocument.ChargePriceSeriesTariffHourlySample)]
            [InlineAutoMoqData(ChargeDocument.BundledChargePriceSeriesSample)]
            public async Task Given_ChargePriceSample_When_GridAccessProviderPeeks_Then_MessageHubReceivesReply(
                string testFilePath)
            {
                // Arrange
                var (request, correlationId) =
                    Fixture.AsGridAccessProvider.PrepareHttpPostRequestWithAuthorization(EndpointUrl, testFilePath);

                // Act
                var actual = await Fixture.HostManager.HttpClient.SendAsync(request);

                // Assert
                actual.StatusCode.Should().Be(HttpStatusCode.Accepted);

                using var assertionScope = new AssertionScope();

                // We expect 8 peek results:
                // * 1 confirmation
                // * 7 data available to energy suppliers
                var peekResults = await Fixture.MessageHubMock.AssertPeekReceivesRepliesAsync(correlationId, 8);
                peekResults.Should().ContainMatch("*ConfirmRequestChangeOfPriceList_MarketDocument*");
                peekResults.Should().NotContainMatch("*Reject*");
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

                // We expect two peeks:
                // * one for the confirmation (first operation)
                // * one for the rejection (second operation violating VR.903)
                using var assertionScope = new AssertionScope();
                var peekResults = await Fixture.MessageHubMock.AssertPeekReceivesRepliesAsync(correlationId, 2);
                peekResults.Should().ContainMatch("*ConfirmRequestChangeOfPriceList_MarketDocument*");
                peekResults.Should().ContainMatch("*RejectRequestChangeOfPriceList_MarketDocument*");
                peekResults.Should().ContainMatch("*It is not allowed to change the tax indicator to Tax for charge*");
            }

            [Fact]
            public async Task Given_ChargeExampleFileWithInvalidBusinessReasonCode_When_GridAccessProviderSendsMessage_Then_CorrectSynchronousErrorIsReturned()
            {
                // Arrange
                var (request, _) = Fixture.AsGridAccessProvider.PrepareHttpPostRequestWithAuthorization(
                    EndpointUrl, ChargeDocument.TariffPriceSeriesWithInvalidBusinessReasonCode);

                // Act
                var response = await Fixture.HostManager.HttpClient.SendAsync(request);

                // Act and assert
                response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
                var responseAsString = await response.Content.ReadAsStringAsync();
                responseAsString.Should().Contain("<Code>B2B-005</Code>");
                responseAsString.Should().Contain("process.processType' element is invalid - The value 'A99' is invalid according to its datatype");
            }

            [Fact]
            public async Task Given_ChargePriceMessageWithChargeInformationData_WhenPosted_ThenChargeInformationDataIsIgnored()
            {
                // Arrange
                const string senderProvidedChargeId = "EA-001";
                const ChargeType chargeType = ChargeType.Tariff;
                var ownerId = SeededData.MarketParticipants.SystemOperator.Id;

                await using var chargesReadDatabaseContext = Fixture.ChargesDatabaseManager.CreateDbContext();
                var expected = GetCharge(chargesReadDatabaseContext, senderProvidedChargeId, ownerId, chargeType);
                var (request, _) = Fixture.AsSystemOperator.PrepareHttpPostRequestWithAuthorization(
                    EndpointUrl, ChargeDocument.TaxTariffPriceSeriesWithInformationToBeIgnored);

                // Act
                var actualResponse = await Fixture.HostManager.HttpClient.SendAsync(request);

                // Assert
                actualResponse.StatusCode.Should().Be(HttpStatusCode.Accepted);
                await FunctionAsserts.AssertHasExecutedAsync(
                    Fixture.HostManager, nameof(ChargePriceCommandReceiverEndpoint)).ConfigureAwait(false);

                await using var chargesAssertDatabaseContext = Fixture.ChargesDatabaseManager.CreateDbContext();
                var actual = GetCharge(chargesAssertDatabaseContext, senderProvidedChargeId, ownerId, chargeType);
                actual.Should().BeEquivalentTo(expected, x => x.Excluding(y => y!.Points));
                actual.Points.Count.Should().Be(24);
            }

            [Fact]
            public async Task Given_ChargePriceMessageWithChargeInformationData_WhenPosted_ThenChargePriceDataAvailableNotificationToGridAccessProviderContainsMandatoryChargeInformationOnly()
            {
                // Arrange
                var (request, correlationId) = Fixture.AsSystemOperator.PrepareHttpPostRequestWithAuthorization(
                    EndpointUrl, ChargeDocument.TaxTariffPriceSeriesWithInformationToBeIgnored);

                // Act
                var actualResponse = await Fixture.HostManager.HttpClient.SendAsync(request);

                // Assert
                actualResponse.StatusCode.Should().Be(HttpStatusCode.Accepted);

                using var assertionScope = new AssertionScope();

                // We expect 11 peek results:
                // 1 confirmation
                // 7 data available to energy suppliers
                // 3 data available to grid access providers
                var peekResult = await Fixture.MessageHubMock.AssertPeekReceivesRepliesAsync(correlationId, 11);
                var notification = peekResult.First(s =>
                    s.Contains("NotifyPriceList_MarketDocument")
                    && s.Contains("<cim:receiver_MarketParticipant.marketRole.type>DDM"));
                notification.Should().Contain("<cim:process.processType>D08</cim:process.processType>");
                notification.Should().Contain("<cim:chargeTypeOwner_MarketParticipant.mRID codingScheme=\"A10\">5790000432752");
                notification.Should().Contain("<cim:type>D03</cim:type>");
                notification.Should().Contain("<cim:mRID>EA-001</cim:mRID>");
                notification.Should().Contain("<cim:effectiveDate>");
                notification.Should().NotContain("<cim:name>");
                notification.Should().NotContain("<cim:description>");
                notification.Should().NotContain("<cim:VATPayer>");
                notification.Should().NotContain("<cim:transparentInvoicing>");
                notification.Should().NotContain("<cim:taxIndicator>");
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
                actual.StatusCode.Should().Be(HttpStatusCode.Accepted);

                using var assertionScope = new AssertionScope();
                // * Expect one for the confirmation (update)
                var peekResults = await Fixture.MessageHubMock.AssertPeekReceivesRepliesAsync(updateCorrelationId);
                peekResults.Should().ContainMatch("*ConfirmRequestChangeOfPriceList_MarketDocument*");
            }

            [Fact]
            public async Task When_ChargePriceRequestFailsDocumentValidation_Then_ARejectionShouldBeSent()
            {
                // Arrange
                var (request, correlationId) = Fixture.AsGridAccessProvider.PrepareHttpPostRequestWithAuthorization(
                    EndpointUrl, ChargeDocument.TariffPriceSeriesWithInvalidRecipientType);

                // Act
                var actualResponse = await Fixture.HostManager.HttpClient.SendAsync(request);

                // Assert
                using var assertionScope = new AssertionScope();
                actualResponse.StatusCode.Should().Be(HttpStatusCode.Accepted);

                // We expect 1 peek results:
                // 1 rejection due to role (Must be DDZ)
                var peekResult = await Fixture.MessageHubMock.AssertPeekReceivesRepliesAsync(correlationId);
                peekResult.Should().ContainMatch("*RejectRequestChangeOfPriceList_MarketDocument*");
                peekResult.Should().NotContainMatch("*NotifyPriceList_MarketDocument*");
                peekResult.Should().ContainMatch("*<cim:process.processType>D08</cim:process.processType>*");
                peekResult.Should().NotContainMatch("*<cim:process.processType>D18</cim:process.processType>*");
                peekResult.Should().ContainMatch("*<cim:code>E55</cim:code>*");
            }

            [Theory]
            [InlineData(ChargeDocument.TariffPriceSeriesInvalidMaximumPrice, "*<cim:code>E90</cim:code>*")]
            [InlineData(ChargeDocument.TariffPriceSeriesInvalidNumberOfPoints, "*<cim:code>E87</cim:code>*")]
            [InlineData(ChargeDocument.TariffPriceSeriesInvalidPointsStart, "*<cim:code>E86</cim:code>*")]
            public async Task When_ChargePriceRequestFailsInputValidation_Then_ARejectionShouldBeSent(
                string testFilePath,
                string expectedErrorCode)
            {
                // Arrange
                var (request, correlationId) =
                    Fixture.AsGridAccessProvider.PrepareHttpPostRequestWithAuthorization(EndpointUrl, testFilePath);

                // Act
                var actual = await Fixture.HostManager.HttpClient.SendAsync(request);

                // Assert
                using var assertionScope = new AssertionScope();
                actual.StatusCode.Should().Be(HttpStatusCode.Accepted);

                // We expect 1 peek results:
                // 1 rejection due to invalid price
                var peekResult = await Fixture.MessageHubMock.AssertPeekReceivesRepliesAsync(correlationId);
                peekResult.Should().ContainMatch("*RejectRequestChangeOfPriceList_MarketDocument*");
                peekResult.Should().NotContainMatch("*NotifyPriceList_MarketDocument*");
                peekResult.Should().ContainMatch("*<cim:process.processType>D08</cim:process.processType>*");
                peekResult.Should().NotContainMatch("*<cim:process.processType>D18</cim:process.processType>*");
                peekResult.Should().ContainMatch(expectedErrorCode);
            }

            [Fact]
            public async Task When_BundledChargePriceRequestForSameCharge_FirstOperationFailsInputValidation_Then_ARejectionIsAlsoSentForSubsequentOperation()
            {
                // Arrange
                var (request, correlationId) = Fixture.AsGridAccessProvider.PrepareHttpPostRequestWithAuthorization(
                    EndpointUrl, ChargeDocument.BundledTariffPriceSeriesFirstOperationInvalidMaximumPrice);

                // Act
                var actual = await Fixture.HostManager.HttpClient.SendAsync(request);

                // Assert
                using var assertionScope = new AssertionScope();

                actual.StatusCode.Should().Be(HttpStatusCode.Accepted);
                // We expect 2 peek results:
                // 2 rejections the first due to invalid price and the second due to the first error
                var peekResult = await Fixture.MessageHubMock.AssertPeekReceivesRepliesAsync(correlationId, 2);
                foreach (var result in peekResult)
                {
                    result.Should().Contain("RejectRequestChangeOfPriceList_MarketDocument");
                    result.Should().Contain("<cim:process.processType>D08</cim:process.processType>");
                }

                peekResult[0].Should().Contain("<cim:code>E90</cim:code>");
                peekResult[1].Should().Contain("<cim:code>D14</cim:code>");
            }

            [Fact]
            public async Task When_BundledChargePriceRequestWhere2ndOperationHasMismatchingOwner_Then_2ndOperationIsRejected_And_1stAnd3rdOperationAccepted()
            {
                // Arrange
                var (request, correlationId) = Fixture.AsGridAccessProvider.PrepareHttpPostRequestWithAuthorization(
                    EndpointUrl, ChargeDocument.BundledSubscriptionPriceSeriesSecondOperationChargeOwnerMismatch);

                // Act
                var actual = await Fixture.HostManager.HttpClient.SendAsync(request);

                // Assert
                using var assertionScope = new AssertionScope();
                actual.StatusCode.Should().Be(HttpStatusCode.Accepted);

                // We expect 17 peek results:
                // * 2 confirmation
                // * 2 x 7 data available to energy suppliers
                // * 1 rejection (for 2nd operation due mismatching charge owner)
                var peekResult = await Fixture.MessageHubMock.AssertPeekReceivesRepliesAsync(correlationId, 17);

                peekResult.Count(s => s.Contains("ConfirmRequestChangeOfPriceList_MarketDocument")).Should().Be(17);

                var rejection = peekResult.Single(s => s.Contains("RejectRequestChangeOfPriceList_MarketDocument"));
                rejection.Should().Contain("<cim:process.processType>D08</cim:process.processType>");
                rejection.Should().Contain("<cim:code>E0I</cim:code>");
            }

            // TODO: Reenable test as soon as business rules are validated
            [Fact(Skip = "Not able to validate business rules, because UpdatePrices is commented out and therefore rules are never validated.")]
            public async Task When_ChargePriceRequestFailsBusinessValidation_Then_ARejectionShouldBeSent()
            {
                // Arrange
                var (request, correlationId) = Fixture.AsGridAccessProvider.PrepareHttpPostRequestWithAuthorization(
                    EndpointUrl, ChargeDocument.TariffPriceSeriesInvalidStartAndEndDate);

                // Act
                var actual = await Fixture.HostManager.HttpClient.SendAsync(request);

                // Assert
                actual.StatusCode.Should().Be(HttpStatusCode.Accepted);

                using var assertionScope = new AssertionScope();

                var peekResult = await Fixture.MessageHubMock.AssertPeekReceivesRepliesAsync(correlationId);
                peekResult.Should().ContainMatch("*RejectRequestChangeOfPriceList_MarketDocument*");
                peekResult.Should().NotContainMatch("*NotifyPriceList_MarketDocument*");
                peekResult.Should().ContainMatch("*<cim:process.processType>D08</cim:process.processType>*");
                peekResult.Should().NotContainMatch("*<cim:process.processType>D18</cim:process.processType>*");
                peekResult.Should().ContainMatch("*<cim:code>E86</cim:code>*");
            }

            [Theory]
            [InlineAutoMoqData(ChargeDocument.PriceSeriesExistingFee)]
            [InlineAutoMoqData(ChargeDocument.PriceSeriesExistingTariff)]
            [InlineAutoMoqData(ChargeDocument.PriceSeriesExistingSubscription)]
            public async Task When_SendingChargePriceRequestForExistingCharge_Then_AConfirmationIsShouldBeSent(string testFilePath)
            {
                // Arrange
                var (request, correlationId) =
                    Fixture.AsGridAccessProvider.PrepareHttpPostRequestWithAuthorization(EndpointUrl, testFilePath);

                // Act
                var actual = await Fixture.HostManager.HttpClient.SendAsync(request);

                // Assert
                using var assertionScope = new AssertionScope();
                actual.StatusCode.Should().Be(HttpStatusCode.Accepted);

                // We expect 8 peek results:
                // * 1 confirmation
                // * 7 data available to energy suppliers
                var peekResult = await Fixture.MessageHubMock.AssertPeekReceivesRepliesAsync(correlationId, 8);
                peekResult.Should().ContainMatch("*ConfirmRequestChangeOfPriceList_MarketDocument*");
                peekResult.Should().NotContainMatch("*RejectRequestChangeOfPriceList_MarketDocument*");
                peekResult.Should().ContainMatch("*<cim:process.processType>D08</cim:process.processType>*");
                peekResult.Should().NotContainMatch("*<cim:process.processType>D18</cim:process.processType>*");
            }

            // TODO: Reenable when new price flow send notifications (story #1561)
            [Fact(Skip = "Not implemented")]
            public async Task When_SendingChargePriceRequestForExistingTariff_Then_AConfirmationIsShouldBeSent()
            {
                // Arrange
                var (request, correlationId) = Fixture.AsGridAccessProvider.PrepareHttpPostRequestWithAuthorization(
                    EndpointUrl, ChargeDocument.PriceSeriesExistingTariff);

                // Act
                var actual = await Fixture.HostManager.HttpClient.SendAsync(request);

                // Assert
                using var assertionScope = new AssertionScope();
                actual.StatusCode.Should().Be(HttpStatusCode.Accepted);
                var peekResult = await Fixture.MessageHubMock.AssertPeekReceivesRepliesAsync(correlationId, 2);
                peekResult.Should().ContainMatch("*ConfirmRequestChangeOfPriceList_MarketDocument*");
                peekResult.Should().NotContainMatch("*RejectRequestChangeOfPriceList_MarketDocument*");
                peekResult.Should().ContainMatch("*NotifyPriceList_MarketDocument*");
                peekResult.Should().ContainMatch("*<cim:process.processType>D08</cim:process.processType>*");
                peekResult.Should().NotContainMatch("*<cim:process.processType>D18</cim:process.processType>*");
            }

            [Fact]
            public async Task When_SendingChargePriceRequestForExistingSubscription_Then_AConfirmationIsShouldBeSent()
            {
                // Arrange
                var (request, correlationId) = Fixture.AsGridAccessProvider.PrepareHttpPostRequestWithAuthorization(
                    EndpointUrl, ChargeDocument.PriceSeriesExistingSubscription);

                // Act
                var actual = await Fixture.HostManager.HttpClient.SendAsync(request);

                // Assert
                using var assertionScope = new AssertionScope();
                actual.StatusCode.Should().Be(HttpStatusCode.Accepted);

                // We expect 8 peek results:
                // * 1 confirmation
                // * 7 data available to energy suppliers
                var peekResult = await Fixture.MessageHubMock.AssertPeekReceivesRepliesAsync(correlationId, 8);
                peekResult.Should().ContainMatch("*ConfirmRequestChangeOfPriceList_MarketDocument*");
                peekResult.Should().NotContainMatch("*RejectRequestChangeOfPriceList_MarketDocument*");
                peekResult.Should().NotContainMatch("*NotifyPriceList_MarketDocument*");
                peekResult.Should().ContainMatch("*<cim:process.processType>D08</cim:process.processType>*");
                peekResult.Should().NotContainMatch("*<cim:process.processType>D18</cim:process.processType>*");
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
                var peekResults = await Fixture.MessageHubMock.AssertPeekReceivesRepliesAsync(correlationId, 11);
                peekResults.Should().NotContainMatch("*RejectRequestChangeOfPriceList_MarketDocument*");
                peekResults.Should().ContainMatch("*NotifyPriceList_MarketDocument*");
                peekResults.Should().ContainMatch("*8100000000030*");
                peekResults.Should().ContainMatch("*8100000000016*");
                peekResults.Should().ContainMatch("*8100000000023*");
                peekResults.Should().NotContainMatch("*8900000000005*");
                peekResults.Should().ContainMatch("*<cim:process.processType>D18</cim:process.processType>*");
                peekResults.Should().NotContainMatch("*<cim:process.processType>D08</cim:process.processType>*");
            }

            [Fact(Skip = "Disabled until Charge Price flow is fully functional as the current SupportOldFlowAsync sets Tax to TaxIndicator.Unknown which means no Grid access provider will get notified.")]
            public async Task When_TaxTariffPricesAreUpdatedBySystemOperator_Then_ANotificationShouldBeReceivedByActiveGridAccessProviders()
            {
                var (request, correlationId) = Fixture.AsSystemOperator.PrepareHttpPostRequestWithAuthorization(
                    EndpointUrl, ChargeDocument.PriceSeriesTariffFromSystemOperator);

                // Act
                await Fixture.HostManager.HttpClient.SendAsync(request);

                // Assert
                using var assertionScope = new AssertionScope();
                var peekResults = await Fixture.MessageHubMock.AssertPeekReceivesRepliesAsync(correlationId, 4);
                peekResults.Should().NotContainMatch("*RejectRequestChangeOfPriceList_MarketDocument*");
                peekResults.Should().ContainMatch("*NotifyPriceList_MarketDocument*");
                peekResults.Should().ContainMatch("*8100000000030*");
                peekResults.Should().ContainMatch("*8100000000016*");
                peekResults.Should().ContainMatch("*8100000000023*");
                peekResults.Should().NotContainMatch("*8900000000005*");
            }

            private static ZonedDateTimeService GetZonedDateTimeService()
            {
                var clock = new FakeClock(InstantHelper.GetTodayAtMidnightUtc());
                return new ZonedDateTimeService(clock, new Iso8601ConversionConfiguration("Europe/Copenhagen"));
            }

            private static Charge GetCharge(
                IChargesDatabaseContext chargesDatabaseContext,
                string senderProvidedChargeId,
                Guid ownerId,
                ChargeType chargeType)
            {
                var charge = chargesDatabaseContext.Charges.Single(x =>
                    x.SenderProvidedChargeId == senderProvidedChargeId &&
                    x.OwnerId == ownerId &&
                    x.Type == chargeType);
                return charge;
            }
        }
    }
}
