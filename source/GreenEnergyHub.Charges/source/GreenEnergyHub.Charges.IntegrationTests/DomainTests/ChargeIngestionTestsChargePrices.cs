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
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Energinet.DataHub.Core.FunctionApp.TestCommon;
using Energinet.DataHub.MessageHub.Model.Model;
using FluentAssertions;
using FluentAssertions.Execution;
using GreenEnergyHub.Charges.Core.DateTime;
using GreenEnergyHub.Charges.Domain.Charges;
using GreenEnergyHub.Charges.FunctionHost.Charges;
using GreenEnergyHub.Charges.Infrastructure.Persistence;
using GreenEnergyHub.Charges.IntegrationTest.Core.Fixtures.FunctionApp;
using GreenEnergyHub.Charges.IntegrationTest.Core.MessageHub;
using GreenEnergyHub.Charges.IntegrationTest.Core.TestCommon;
using GreenEnergyHub.Charges.IntegrationTest.Core.TestFiles.Charges.PriceSeries;
using GreenEnergyHub.Charges.IntegrationTest.Core.TestHelpers;
using GreenEnergyHub.Charges.IntegrationTests.Fixtures;
using GreenEnergyHub.Charges.TestCore.Attributes;
using GreenEnergyHub.Charges.TestCore.Data;
using GreenEnergyHub.Charges.TestCore.TestHelpers;
using GreenEnergyHub.Iso8601;
using NodaTime.Testing;
using Xunit;
using Xunit.Abstractions;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.IntegrationTests.DomainTests
{
    [IntegrationTest]
    public class ChargeIngestionTestsChargePrices
    {
        private const string EndpointUrl = "api/ChargeIngestion";
        private const int SecondsToWaitForIntegrationEvents = 15;

        [Collection(nameof(ChargesFunctionAppCollectionFixture))]
        [Trait("Category", "ChargeIngestionTestsChargePrices")]
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
                Fixture.MessageHubMock.Reset();

                // We need to clear host log after each test is done to ensure that we can assert on function executed
                // on each test run because we only check on function name.
                Fixture.HostManager.ClearHostLog();
                return Task.CompletedTask;
            }

            /* SYNCHRONOUS RESPONSES */

            [Fact]
            public async Task Ingestion_NoBearerToken_Http401UnauthorizedResponse()
            {
                var request = HttpRequestGenerator.CreateHttpPostRequest(
                    EndpointUrl, ChargePricesRequests.TariffPriceSeries, GetZonedDateTimeService());

                var actual = await Fixture.HostManager.HttpClient.SendAsync(request);

                actual.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            }

            [Fact]
            public async Task Ingestion_InvalidResolutionCode_Http400BadRequestWithB2B005ErrorResponse()
            {
                // Arrange
                var (request, _) = Fixture.AsGridAccessProvider.PrepareHttpPostRequestWithAuthorization(
                    EndpointUrl, ChargePricesRequests.UnsupportedResolution);

                // Act
                var response = await Fixture.HostManager.HttpClient.SendAsync(request);

                // Act and assert
                response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
                var responseAsString = await response.Content.ReadAsStringAsync();
                responseAsString.Should().Contain("<Code>B2B-005</Code>");
                responseAsString.Should().Contain("Provided 'resolution' value 'P1Y' is invalid.");
            }

            /* CONFIRMATIONS - PLEASE REFER TO SAMPLES BELOW */

            /* NOTIFICATIONS */

            [Fact]
            public async Task Ingestion_ChargePricesRequestForTaxTariff_NotificationsReceivedByGridAccessProvidersAndEnergySuppliers()
            {
                var (request, correlationId) = Fixture.AsSystemOperator.PrepareHttpPostRequestWithAuthorization(
                    EndpointUrl, ChargePricesRequests.TaxTariffPriceSeries);

                // Act
                var actual = await Fixture.HostManager.HttpClient.SendAsync(request);

                // Assert
                actual.StatusCode.Should().Be(HttpStatusCode.Accepted);

                // We expect at least 8 peek results:
                // * Confirmation to System Operator
                // * 3x Notifications to Energy Suppliers
                // * 4x Notifications to Grid Access Providers
                using var assertionScope = new AssertionScope();
                var peekResults = await Fixture.MessageHubMock
                    .AssertPeekReceivesRepliesAsync(correlationId, ResponseFormat.Xml, 8);

                peekResults.Should().HaveCount(8);
                peekResults.Should().ContainMatch("*" + CimMessageConstants.ConfirmRequestChangeOfPriceListRootElement + "*");
                peekResults.Should().NotContainMatch("*" + CimMessageConstants.RejectRequestChangeOfPriceListRootElement + "*");
                peekResults.Should().ContainMatch("*<cim:process.processType>D08</cim:process.processType>*");
                peekResults.Should().NotContainMatch("*<cim:process.processType>D18</cim:process.processType>*");

                var energySupplierNotifications = peekResults
                    .Where(x => x.Contains(CimMessageConstants.NotifyPriceListRootElement) && x.Contains("DDQ"))
                    .ToList();
                energySupplierNotifications.Should().HaveCount(3);
                energySupplierNotifications.Should().ContainMatch("*8100000000108*");

                var gridAccessProviderNotifications = peekResults
                    .Where(x => x.Contains(CimMessageConstants.NotifyPriceListRootElement) && x.Contains("DDM"))
                    .ToList();
                gridAccessProviderNotifications.Should().HaveCount(4);
                gridAccessProviderNotifications.Should().ContainMatch("*8100000000030*");
            }

            [Fact]
            public async Task Ingestion_ChargePricesRequestForTaxTariff_NotificationsShouldBeReceivedByGridAccessProvidersAndSuppliersAsJson()
            {
                // Arrange
                var (request, correlationId) = Fixture.AsSystemOperator.PrepareHttpPostRequestWithAuthorization(
                    EndpointUrl, ChargePricesRequests.TaxTariffPriceSeries);

                // Act
                var actualResponse = await Fixture.HostManager.HttpClient.SendAsync(request);

                // Assert
                actualResponse.StatusCode.Should().Be(HttpStatusCode.Accepted);

                // We expect at least 8 peek results:
                // * Confirmation to System Operator
                // * 3x Notifications to Energy Suppliers
                // * 4x Notifications to Grid Access Providers
                using var assertionScope = new AssertionScope();

                var peekResults = await Fixture.MessageHubMock
                    .AssertPeekReceivesRepliesAsync(correlationId, ResponseFormat.Json, 8);

                var energySupplierNotifications = peekResults
                    .Where(x => x.Contains(CimMessageConstants.NotifyPriceListRootElement) && x.Contains("DDQ"))
                    .ToList();
                energySupplierNotifications.Should().HaveCount(3);

                using var supplierJsonDocument = JsonDocumentExtensions.AsJsonDocument(energySupplierNotifications.First());
                supplierJsonDocument.GetDocumentType().Should().Be("D12");
                supplierJsonDocument.GetBusinessReasonCode().Should().Be("D08");
                supplierJsonDocument.GetReceiverRole().Should().Be("DDQ");
                supplierJsonDocument.GetChargeIds().Single().Should().Be("EA-001");

                var gridAccessProviderNotifications = peekResults
                    .Where(x => x.Contains(CimMessageConstants.NotifyPriceListRootElement) && x.Contains("DDM"))
                    .ToList();
                gridAccessProviderNotifications.Should().HaveCount(4);

                using var gridAccessProviderJsonDocument =
                    JsonDocumentExtensions.AsJsonDocument(gridAccessProviderNotifications.First());
                gridAccessProviderJsonDocument.GetDocumentType().Should().Be("D12");
                gridAccessProviderJsonDocument.GetBusinessReasonCode().Should().Be("D08");
                gridAccessProviderJsonDocument.GetReceiverRole().Should().Be("DDM");
                gridAccessProviderJsonDocument.GetChargeIds().Single().Should().Be("EA-001");
            }

            [Fact]
            public async Task Ingestion_ChargePricesRequestWithChargeInformationData_NotificationContainsMandatoryChargeInformationDataOnly()
            {
                // Arrange
                var (request, correlationId) = Fixture.AsSystemOperator.PrepareHttpPostRequestWithAuthorization(
                    EndpointUrl, ChargePricesRequests.TaxTariffPriceSeriesWithInformationToBeIgnored);

                // Act
                var actualResponse = await Fixture.HostManager.HttpClient.SendAsync(request);

                // Assert
                actualResponse.StatusCode.Should().Be(HttpStatusCode.Accepted);

                using var assertionScope = new AssertionScope();

                // We expect 8 peek results:
                // Confirmation to System Operator
                // 3x Notifications to Energy Suppliers
                // 4x Notifications to Grid Access Providers
                var peekResult = await Fixture.MessageHubMock
                    .AssertPeekReceivesRepliesAsync(correlationId, ResponseFormat.Xml, 8);
                var notification = peekResult.First(s =>
                    s.Contains(CimMessageConstants.NotifyPriceListRootElement)
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

            /* REJECTIONS */

            [Fact]
            public async Task Ingestion_ChargePricesRequestFailsDocumentValidation_RejectionReceived()
            {
                // Arrange
                var (request, correlationId) = Fixture.AsGridAccessProvider.PrepareHttpPostRequestWithAuthorization(
                    EndpointUrl, ChargePricesRequests.TariffPriceSeriesWithInvalidRecipientType);

                // Act
                var actualResponse = await Fixture.HostManager.HttpClient.SendAsync(request);

                // Assert
                actualResponse.StatusCode.Should().Be(HttpStatusCode.Accepted);

                // We expect 1 peek result:
                // Rejection to Grid Access Provider due to invalid recipient role (Must be DDZ)
                using var assertionScope = new AssertionScope();

                var peekResult = await Fixture.MessageHubMock
                    .AssertPeekReceivesRepliesAsync(correlationId, ResponseFormat.Xml);
                peekResult.Should().ContainMatch("*" + CimMessageConstants.RejectRequestChangeOfPriceListRootElement + "*");
                peekResult.Should().ContainMatch("*<cim:process.processType>D08</cim:process.processType>*");
                peekResult.Should().ContainMatch("*<cim:code>E55</cim:code>*");
                peekResult.Should().NotContainMatch("*" + CimMessageConstants.NotifyPriceListRootElement + "*");
                peekResult.Should().NotContainMatch("*<cim:process.processType>D18</cim:process.processType>*");
            }

            [Theory]
            [InlineData(ChargePricesRequests.TariffPriceSeriesInvalidMaximumPrice, "*<cim:code>E90</cim:code>*")]
            [InlineData(ChargePricesRequests.TariffPriceSeriesInvalidNumberOfPoints, "*<cim:code>E87</cim:code>*")]
            [InlineData(ChargePricesRequests.TariffPriceSeriesInvalidPointsStart, "*<cim:code>E86</cim:code>*")]
            public async Task Ingestion_ChargePricesRequestFailsInputValidation_RejectionReceived(
                string testFilePath,
                string expectedErrorCode)
            {
                // Arrange
                var (request, correlationId) =
                    Fixture.AsGridAccessProvider.PrepareHttpPostRequestWithAuthorization(EndpointUrl, testFilePath);

                // Act
                var actual = await Fixture.HostManager.HttpClient.SendAsync(request);

                // Assert
                actual.StatusCode.Should().Be(HttpStatusCode.Accepted);

                // We expect 1 peek result:
                // Rejection to Grid Access Provider
                using var assertionScope = new AssertionScope();

                var peekResult = await Fixture.MessageHubMock
                    .AssertPeekReceivesRepliesAsync(correlationId, ResponseFormat.Xml);
                peekResult.Should().ContainMatch("*" + CimMessageConstants.RejectRequestChangeOfPriceListRootElement + "*");
                peekResult.Should().ContainMatch("*<cim:process.processType>D08</cim:process.processType>*");
                peekResult.Should().ContainMatch(expectedErrorCode);
                peekResult.Should().NotContainMatch("*" + CimMessageConstants.NotifyPriceListRootElement + "*");
                peekResult.Should().NotContainMatch("*<cim:process.processType>D18</cim:process.processType>*");
            }

            [Fact]
            public async Task Ingestion_ChargePriceRequestFailsBusinessValidation_RejectionReceived()
            {
                // Arrange
                var (request, correlationId) = Fixture.AsGridAccessProvider.PrepareHttpPostRequestWithAuthorization(
                    EndpointUrl, ChargePricesRequests.IrregularPriceSeriesInvalidEndDate);

                // Act
                var actual = await Fixture.HostManager.HttpClient.SendAsync(request);

                // Assert
                actual.StatusCode.Should().Be(HttpStatusCode.Accepted);

                // We expect 1 peek result:
                // Rejection to Grid Access Provider due to irregular price series end date not matching first of month
                using var assertionScope = new AssertionScope();

                var peekResult = await Fixture.MessageHubMock
                    .AssertPeekReceivesRepliesAsync(correlationId, ResponseFormat.Xml);
                peekResult.Should().ContainMatch("*" + CimMessageConstants.RejectRequestChangeOfPriceListRootElement + "*");
                peekResult.Should().ContainMatch("*<cim:process.processType>D08</cim:process.processType>*");
                peekResult.Should().ContainMatch("*<cim:code>D14</cim:code>*");
                peekResult.Should().ContainMatch("*has an end time interval that does not match the first of a month or the charge's stop date*");
                peekResult.Should().NotContainMatch("*" + CimMessageConstants.NotifyPriceListRootElement + "*");
                peekResult.Should().NotContainMatch("*<cim:process.processType>D18</cim:process.processType>*");
            }

            /* BUNDLING */

            [Fact]
            public async Task Ingestion_BundleWithMultiplePriceSeriesForSameSubscription__NotificationsWithChronologicallyOrderedPriceSeries()
            {
                // Arrange
                var expectedNotificationOperations = new List<string> { "1.100000", "2.200000", "3.300000" };

                var (request, correlationId) = Fixture.AsGridAccessProvider.PrepareHttpPostRequestWithAuthorization(
                    EndpointUrl, ChargePricesRequests.BundledSubscriptionPriceSeries);

                // Act
                var actual = await Fixture.HostManager.HttpClient.SendAsync(request);

                // Assert
                actual.StatusCode.Should().Be(HttpStatusCode.Accepted);

                // We expect 4 peek results:
                // * Confirmation to Grid Access Provider
                // * 3x Notifications to Energy Suppliers
                using var assertionScope = new AssertionScope();
                var peekResults = await Fixture.MessageHubMock
                    .AssertPeekReceivesRepliesAsync(correlationId, ResponseFormat.Xml, 12);
                peekResults.Should().HaveCount(4);
                peekResults.Should().ContainMatch("*" + CimMessageConstants.ConfirmRequestChangeOfPriceListRootElement + "*");
                peekResults.Should().ContainMatch("*" + CimMessageConstants.NotifyPriceListRootElement + "*");
                peekResults.Should().NotContainMatch("*" + CimMessageConstants.RejectRequestChangeOfPriceListRootElement + "*");

                foreach (var peekResult in peekResults.Where(x => x.Contains(CimMessageConstants.NotifyPriceListRootElement)))
                {
                    var notificationOperations =
                        CIMXmlReader.GetActivityRecordElements(peekResult, CimMessageConstants.Point, CimMessageConstants.Price);
                    notificationOperations.Should().ContainInOrder(expectedNotificationOperations);
                }
            }

            [Fact]
            public async Task Ingestion_BundleWithMultiplePriceSeriesForSameSubscription__NotificationsWithChronologicallyOrderedPriceSeriesAsJson()
            {
                // Arrange
                var expectedNotificationOperations = new List<string> { "1.100000", "2.200000", "3.300000" };

                var (request, correlationId) = Fixture.AsGridAccessProvider.PrepareHttpPostRequestWithAuthorization(
                    EndpointUrl, ChargePricesRequests.BundledSubscriptionPriceSeries);

                // Act
                var actual = await Fixture.HostManager.HttpClient.SendAsync(request);

                // Assert
                actual.StatusCode.Should().Be(HttpStatusCode.Accepted);

                // We expect 4 peek results:
                // * Confirmation to Grid Access Provider
                // * 3x Notifications to Energy Suppliers
                using var assertionScope = new AssertionScope();
                var peekResults = await Fixture.MessageHubMock
                    .AssertPeekReceivesRepliesAsync(correlationId, ResponseFormat.Json, 12);

                var energySupplierNotifications = peekResults
                    .Where(x => x.Contains(CimMessageConstants.NotifyPriceListRootElement) && x.Contains("DDQ"))
                    .ToList();
                energySupplierNotifications.Should().HaveCount(3);

                using var supplierJsonDocument = JsonDocumentExtensions.AsJsonDocument(energySupplierNotifications.First());
                supplierJsonDocument.GetPrices().Should().ContainInOrder(expectedNotificationOperations);
            }

            [Fact]
            public async Task Ingestion_BundledChargePricesRequestForSameCharge_FirstPriceSeriesFailsInputValidation_RejectionAlsoContainsSubsequentPriceSeries()
            {
                // Arrange
                var (request, correlationId) = Fixture.AsGridAccessProvider.PrepareHttpPostRequestWithAuthorization(
                    EndpointUrl, ChargePricesRequests.BundledTariffPriceSeriesFirstOperationInvalidMaximumPrice);

                // Act
                var actual = await Fixture.HostManager.HttpClient.SendAsync(request);

                // Assert
                actual.StatusCode.Should().Be(HttpStatusCode.Accepted);

                // We expect 1 peek result:
                // Rejection with both first and second price series
                using var assertionScope = new AssertionScope();

                var peekResult = await Fixture.MessageHubMock
                    .AssertPeekReceivesRepliesAsync(correlationId, ResponseFormat.Xml, 2);
                var result = peekResult.Single();
                result.Should().Contain(CimMessageConstants.RejectRequestChangeOfPriceListRootElement);
                result.Should().Contain("<cim:process.processType>D08</cim:process.processType>");
                result.Should().Contain("OpId1_0000A");
                result.Should().Contain("<cim:code>E90</cim:code>");
                result.Should().Contain("OpId2_0000A");
                result.Should().Contain("<cim:code>D14</cim:code>");
            }

            [Fact]
            public async Task Ingestion_BundledChargePricesRequestWhere2ndPriceSeriesHasMismatchingOwner_RejectionWith2ndPriceSeries_And_ConfirmationWith1stAnd3rdPricesSeries()
            {
                // Arrange
                var (request, correlationId) = Fixture.AsGridAccessProvider.PrepareHttpPostRequestWithAuthorization(
                    EndpointUrl, ChargePricesRequests.BundledSubscriptionPriceSeriesSecondOperationChargeOwnerMismatch);

                // Act
                var actual = await Fixture.HostManager.HttpClient.SendAsync(request);

                // Assert
                actual.StatusCode.Should().Be(HttpStatusCode.Accepted);

                // We expect 5 peek results:
                // Confirmation to Grid Access Provider (1st + 3rd price series)
                // Rejection to Grid Access Provider due to mismatching owner (2nd price series)
                // 3x Notifications to Energy Suppliers
                using var assertionScope = new AssertionScope();

                var peekResult = await Fixture.MessageHubMock
                    .AssertPeekReceivesRepliesAsync(correlationId, ResponseFormat.Xml, 9);

                var confirmation = peekResult.Single(s => s.Contains(CimMessageConstants.ConfirmRequestChangeOfPriceListRootElement));
                confirmation.Should().Contain("OpId1_0000B");
                confirmation.Should().Contain("OpId3_0000B");

                var rejection = peekResult.Single(s => s.Contains(CimMessageConstants.RejectRequestChangeOfPriceListRootElement));
                rejection.Should().Contain("<cim:process.processType>D08</cim:process.processType>");
                rejection.Should().Contain("OpId2_0000B");
                rejection.Should().Contain("<cim:code>E0I</cim:code>");
            }

            /* INTEGRATION EVENTS */

            [Fact]
            public async Task Ingestion_ValidChargePricesRequest_ChargePricesUpdatedIntegrationEventPublished()
            {
                // Arrange
                var (request, correlationId) = Fixture.AsGridAccessProvider.PrepareHttpPostRequestWithAuthorization(
                    EndpointUrl, ChargePricesRequests.TariffPriceSeries);
                using var eventualChargePriceUpdatedEvent = await Fixture
                    .ChargePricesUpdatedListener
                    .ListenForMessageAsync(correlationId)
                    .ConfigureAwait(false);

                // Act
                await Fixture.HostManager.HttpClient.SendAsync(request);

                // Assert
                var isChargePricesUpdatedReceived = eventualChargePriceUpdatedEvent
                    .MessageAwaiter!
                    .Wait(TimeSpan.FromSeconds(SecondsToWaitForIntegrationEvents));
                isChargePricesUpdatedReceived.Should().BeTrue();
            }

            /* SAMPLES */

            [Theory]
            [InlineAutoMoqData(ChargePricesRequests.BundledChargePriceSeriesSample, 12, 3)]
            [InlineAutoMoqData(ChargePricesRequests.ChargePriceSeriesSubscriptionMonthlySample, 4, 1)]
            [InlineAutoMoqData(ChargePricesRequests.ChargePriceSeriesFeeMonthlySample, 4, 1)]
            [InlineAutoMoqData(ChargePricesRequests.ChargePriceSeriesTariffHourlySample, 4, 1)]
            public async Task Ingestion_ChargePriceSeriesSample_ConfirmationReceivedByGridAccessProvider(
                string testFilePath, int noOfDataAvailableNotificationsExpected, int noOfConfirmedActivityRecordsExpected)
            {
                // Arrange
                var (request, correlationId) =
                    Fixture.AsGridAccessProvider.PrepareHttpPostRequestWithAuthorization(EndpointUrl, testFilePath);

                // Act
                var actual = await Fixture.HostManager.HttpClient.SendAsync(request);

                // Assert
                actual.StatusCode.Should().Be(HttpStatusCode.Accepted);

                // We expect at least 4 peek results:
                // * Confirmation to Grid Access Provider
                // * 3x Notifications to Energy Suppliers
                using var assertionScope = new AssertionScope();

                var peekResults = await Fixture.MessageHubMock
                    .AssertPeekReceivesRepliesAsync(correlationId, ResponseFormat.Xml, noOfDataAvailableNotificationsExpected);
                peekResults.Should().ContainMatch("*" + CimMessageConstants.ConfirmRequestChangeOfPriceListRootElement + "*");
                peekResults.Should().ContainMatch("*" + CimMessageConstants.NotifyPriceListRootElement + "*");
                peekResults.Should().NotContainMatch("*" + CimMessageConstants.RejectRequestChangeOfPriceListRootElement + "*");

                var peekResult = peekResults.Single(x => x.Contains(CimMessageConstants.ConfirmRequestChangeOfPriceListRootElement));
                var operations = CIMXmlReader.GetActivityRecordElements(
                    peekResult, CimMessageConstants.MarketActivityRecord, CimMessageConstants.OriginalTransactionId);
                operations.Count.Should().Be(noOfConfirmedActivityRecordsExpected);
            }

            /* MISC */

            [Fact]
            public async Task Ingestion_ChargePricesRequestWithChargeInformationData_ChargeInformationIgnored()
            {
                // Arrange
                const string senderProvidedChargeId = "EA-001";
                const ChargeType chargeType = ChargeType.Tariff;
                var ownerId = SeededData.MarketParticipants.SystemOperator.Id;

                await using var chargesReadDatabaseContext = Fixture.ChargesDatabaseManager.CreateDbContext();
                var expected = GetCharge(chargesReadDatabaseContext, senderProvidedChargeId, ownerId, chargeType);
                var (request, _) = Fixture.AsSystemOperator.PrepareHttpPostRequestWithAuthorization(
                    EndpointUrl, ChargePricesRequests.TaxTariffPriceSeriesWithInformationToBeIgnored);

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
