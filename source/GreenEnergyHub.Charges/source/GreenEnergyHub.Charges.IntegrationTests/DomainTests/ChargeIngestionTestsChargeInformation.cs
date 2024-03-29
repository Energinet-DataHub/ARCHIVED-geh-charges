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
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Energinet.DataHub.Core.FunctionApp.TestCommon;
using Energinet.DataHub.Core.TestCommon.AutoFixture.Attributes;
using Energinet.DataHub.MessageHub.Model.Model;
using FluentAssertions;
using FluentAssertions.Execution;
using GreenEnergyHub.Charges.Infrastructure.Core.Function;
using GreenEnergyHub.Charges.IntegrationTest.Core.Fixtures.FunctionApp;
using GreenEnergyHub.Charges.IntegrationTest.Core.MessageHub;
using GreenEnergyHub.Charges.IntegrationTest.Core.TestFiles.Charges;
using GreenEnergyHub.Charges.IntegrationTest.Core.TestHelpers;
using GreenEnergyHub.Charges.IntegrationTests.Fixtures;
using GreenEnergyHub.Charges.TestCore.TestHelpers;
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
        [Trait("Category", "ChargeIngestionTestsChargeInformation")]
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
            public async Task Ingestion_ValidChargeInformationRequest_Http202AcceptedWithEmptyBodyResponse()
            {
                var request = Fixture.AsGridAccessProvider.PrepareHttpPostRequestWithAuthorization(
                    EndpointUrl, ChargeInformationRequests.Tariff);

                var actualResponse = await Fixture.HostManager.HttpClient.SendAsync(request.Request);
                var responseBody = await actualResponse.Content.ReadAsStringAsync();

                actualResponse.StatusCode.Should().Be(HttpStatusCode.Accepted);
                responseBody.Should().BeEmpty();
            }

            [Fact]
            public async Task Ingestion_SenderDoesNotMatchTokenClaim_Http400BadRequestWithB2B008ErrorResponse()
            {
                // Arrange
                var (request, _) = Fixture.AsGridAccessProvider.PrepareHttpPostRequestWithAuthorization(
                    EndpointUrl, ChargeInformationRequests.SenderIdDoNotMatchAuthorizedActorId);

                // Act
                var actual = await Fixture.HostManager.HttpClient.SendAsync(request);

                // Assert
                actual.StatusCode.Should().Be(HttpStatusCode.BadRequest);
                var errorMessage = await actual.Content.ReadAsStreamAsync();
                var document = await XDocument.LoadAsync(errorMessage, LoadOptions.None, CancellationToken.None);
                document.Element("Error")?.Element("Code")?.Value.Should().Be(B2BErrorCodeConstants.SenderIsNotAuthorized);
                document.Element("Error")?.Element("Message")?.Value.Should().Be(ErrorMessageConstants.ActorIsNotWhoTheyClaimToBeErrorMessage);
            }

            [Fact]
            public async Task Ingestion_CIMDocumentIdIsEmptyOrOnlyContainsWhitespace_Http400BadRequestWithB2B005ErrorResponse()
            {
                // Arrange
                var expectedMessage = string.Format(
                    ErrorMessageConstants.SyntaxValidationErrorMessage +
                    Environment.NewLine +
                    ErrorMessageConstants.ValueIsEmptyOrIsWhiteSpace,
                    "mRID");

                var (request, _) = Fixture.AsGridAccessProvider.PrepareHttpPostRequestWithAuthorization(
                    EndpointUrl, ChargeInformationRequests.ChargeDocumentIdIsEmpty);

                // Act
                var actual = await Fixture.HostManager.HttpClient.SendAsync(request);

                // Assert
                actual.StatusCode.Should().Be(HttpStatusCode.BadRequest);
                var errorMessage = await actual.Content.ReadAsStreamAsync();
                var document = await XDocument.LoadAsync(errorMessage, LoadOptions.None, CancellationToken.None);
                document.Element("Error")?.Element("Code")?.Value.Should().Be(B2BErrorCodeConstants.SyntaxValidation);
                var actualMessage = document.Element("Error")?.Element("Message")?.Value;
                Assert.Equal(expectedMessage, actualMessage, ignoreLineEndingDifferences: true);
            }

            [Fact]
            public async Task Ingestion_CIMDocumentBusinessReasonCodeIsUnsupported_Http400BadRequestWithB2B005ErrorResponse()
            {
                // Arrange
                var expectedMessage = string.Format(
                    ErrorMessageConstants.SyntaxValidationErrorMessage +
                    Environment.NewLine +
                    ErrorMessageConstants.UnsupportedEnumErrorMessage,
                    "process.processType",
                    "D02");

                var (request, _) = Fixture.AsGridAccessProvider.PrepareHttpPostRequestWithAuthorization(
                    EndpointUrl, ChargeInformationRequests.InvalidBusinessReasonCode);

                // Act
                var actual = await Fixture.HostManager.HttpClient.SendAsync(request);

                // Assert
                actual.StatusCode.Should().Be(HttpStatusCode.BadRequest);
                var errorMessage = await actual.Content.ReadAsStreamAsync();
                var document = await XDocument.LoadAsync(errorMessage, LoadOptions.None, CancellationToken.None);
                document.Element("Error")?.Element("Code")?.Value.Should().Be(B2BErrorCodeConstants.SyntaxValidation);
                var actualMessage = document.Element("Error")?.Element("Message")?.Value;
                Assert.Equal(expectedMessage, actualMessage, ignoreLineEndingDifferences: true);
            }

            /* CONFIRMATIONS - PLEASE REFER TO SAMPLES BELOW */

            /* NOTIFICATIONS */

            [Fact]
            public async Task Ingestion_NewTaxTariffBySystemOperator_NotificationsReceivedByGridAccessProvidersAndEnergySuppliers()
            {
                var (request, correlationId) = Fixture.AsSystemOperator.PrepareHttpPostRequestWithAuthorization(
                    EndpointUrl, ChargeInformationRequests.TaxTariffAsSystemOperator);

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
                peekResults.Should().ContainMatch("*<cim:process.processType>D18</cim:process.processType>*");
                peekResults.Should().NotContainMatch("*<cim:process.processType>D08</cim:process.processType>*");

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
            public async Task Ingestion_NewTaxTariffBySystemOperator_NotificationsReceivedByGridAccessProvidersAndEnergySuppliersAsJson()
            {
                var (request, correlationId) = Fixture.AsSystemOperator.PrepareHttpPostRequestWithAuthorization(
                    EndpointUrl, ChargeInformationRequests.TaxTariffAsSystemOperator);

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
                    .AssertPeekReceivesRepliesAsync(correlationId, ResponseFormat.Json, 8);

                var energySupplierNotifications = peekResults
                    .Where(x => x.Contains(CimMessageConstants.NotifyPriceListRootElement) && x.Contains("DDQ"))
                    .ToList();
                energySupplierNotifications.Should().HaveCount(3);

                using var supplierJsonDocument = JsonDocumentExtensions.AsJsonDocument(energySupplierNotifications.First());
                supplierJsonDocument.GetDocumentType().Should().Be("D12");
                supplierJsonDocument.GetBusinessReasonCode().Should().Be("D18");
                supplierJsonDocument.GetReceiverRole().Should().Be("DDQ");
                supplierJsonDocument.GetChargeIds().Single().Should().Be("SYOCharge");

                var gridAccessProviderNotifications = peekResults
                    .Where(x => x.Contains(CimMessageConstants.NotifyPriceListRootElement) && x.Contains("DDM"))
                    .ToList();
                gridAccessProviderNotifications.Should().HaveCount(4);

                using var gridAccessProviderJsonDocument =
                    JsonDocumentExtensions.AsJsonDocument(gridAccessProviderNotifications.First());
                gridAccessProviderJsonDocument.GetDocumentType().Should().Be("D12");
                gridAccessProviderJsonDocument.GetBusinessReasonCode().Should().Be("D18");
                gridAccessProviderJsonDocument.GetReceiverRole().Should().Be("DDM");
                gridAccessProviderJsonDocument.GetChargeIds().Single().Should().Be("SYOCharge");
            }

            /* REJECTIONS */

            [Fact]
            public async Task Ingestion_InvalidChargeRequest_RejectionReceivedWithErrorCodeAndMessage()
            {
                // Arrange
                var (request, correlationId) = Fixture.AsGridAccessProvider.PrepareHttpPostRequestWithAuthorization(
                    EndpointUrl, ChargeInformationRequests.InvalidTaxTariffAsGridAccessProvider);

                // Act
                var actual = await Fixture.HostManager.HttpClient.SendAsync(request);

                // Assert
                actual.StatusCode.Should().Be(HttpStatusCode.Accepted);

                using var assertionScope = new AssertionScope();

                var peekResult =
                    await Fixture.MessageHubMock.AssertPeekReceivesRepliesAsync(correlationId, ResponseFormat.Xml);

                peekResult.Should().ContainMatch("*" + CimMessageConstants.RejectRequestChangeOfPriceListRootElement + "*");
                peekResult.Should().ContainMatch("*<cim:code>E0I</cim:code>*");
                peekResult.Should().ContainMatch("*The sender role used is not allowed to set tax indicator to Tax for charge ID*");
            }

            /* BUNDLING */

            [Fact]
            public async Task Ingestion_BundleWithMultipleOperationsForSameCharge_NotificationsWithChronologicallyOrderedOperations()
            {
                // Arrange
                var expectedNotificationOperations = new List<string> { "CREATE", "UPDATE", "STOP", "CANCEL STOP" };

                var (request, correlationId) = Fixture.AsGridAccessProvider.PrepareHttpPostRequestWithAuthorization(
                    EndpointUrl, ChargeInformationRequests.BundleWithMultipleOperationsForSameTariff);

                // Act
                var actual = await Fixture.HostManager.HttpClient.SendAsync(request);

                // Assert
                actual.StatusCode.Should().Be(HttpStatusCode.Accepted);

                // We expect four peeks:
                // * A confirmation to Grid Access Provider
                // * A single notification to Energy Supplier 8100000000108
                // * A single notification to Energy Supplier 8100000001004
                // * A single notification to Energy Supplier 8510000000013
                using var assertionScope = new AssertionScope();

                var peekResults = await Fixture.MessageHubMock
                    .AssertPeekReceivesRepliesAsync(correlationId, ResponseFormat.Xml, 16);
                peekResults.Should().ContainMatch("*" + CimMessageConstants.ConfirmRequestChangeOfPriceListRootElement + "*");
                peekResults.Should().ContainMatch("*" + CimMessageConstants.NotifyPriceListRootElement + "*");
                peekResults.Should().NotContainMatch("*" + CimMessageConstants.RejectRequestChangeOfPriceListRootElement + "*");

                foreach (var peekResult in peekResults.Where(x => x.Contains(CimMessageConstants.NotifyPriceListRootElement)))
                {
                    var notificationOperations =
                        CIMXmlReader.GetActivityRecordElements(
                            peekResult,
                            CimMessageConstants.ChargeType,
                            CimMessageConstants.ChargeDescription);
                    notificationOperations.Should().ContainInOrder(expectedNotificationOperations);
                }
            }

            [Fact]
            public async Task Ingestion_BundleWithMultipleOperationsForSameCharge_NotificationsWithChronologicallyOrderedOperationsAsJson()
            {
                // Arrange
                var expectedNotificationOperations = new List<string> { "CREATE", "UPDATE", "STOP", "CANCEL STOP" };

                var (request, correlationId) = Fixture.AsGridAccessProvider.PrepareHttpPostRequestWithAuthorization(
                    EndpointUrl, ChargeInformationRequests.BundleWithMultipleOperationsForSameTariff);

                // Act
                var actual = await Fixture.HostManager.HttpClient.SendAsync(request);

                // Assert
                actual.StatusCode.Should().Be(HttpStatusCode.Accepted);

                // We expect four peeks:
                // * A confirmation to Grid Access Provider
                // * A single notification to Energy Supplier 8100000000108
                // * A single notification to Energy Supplier 8100000001004
                // * A single notification to Energy Supplier 8510000000013
                using var assertionScope = new AssertionScope();

                var peekResults = await Fixture.MessageHubMock
                    .AssertPeekReceivesRepliesAsync(correlationId, ResponseFormat.Json, 16);

                var energySupplierNotifications = peekResults
                    .Where(x => x.Contains(CimMessageConstants.NotifyPriceListRootElement) && x.Contains("DDQ"))
                    .ToList();
                energySupplierNotifications.Should().HaveCount(3);

                using var supplierJsonDocument = JsonDocumentExtensions.AsJsonDocument(energySupplierNotifications.First());
                supplierJsonDocument.GetChargeDescriptions().Should().ContainInOrder(expectedNotificationOperations);
            }

            // TODO: Remove? (Similar test is added to "ChargeInformationCommandReceivedEventHandlerTests.cs")
            [Fact]
            public async Task Ingestion_BundleWithTwoOperationsForSameTariffWhere2ndIsInvalid_Confirms1st_Rejects2nd_And_NotifiesAbout1st()
            {
                // Arrange
                var (request, correlationId) = Fixture.AsSystemOperator.PrepareHttpPostRequestWithAuthorization(
                    EndpointUrl, ChargeInformationRequests.BundleWithTwoOperationsForSameTariffSecondOpViolatingVr903);

                // Act
                var actual = await Fixture.HostManager.HttpClient.SendAsync(request);

                // Assert
                actual.StatusCode.Should().Be(HttpStatusCode.Accepted);

                // We expect 6 peeks:
                // * Confirmation (first operation) to System Operator
                // * Rejection (second operation violating VR.903) to System Operator
                // * 3x notifications to Energy Suppliers
                using var assertionScope = new AssertionScope();
                var peekResults =
                    await Fixture.MessageHubMock.AssertPeekReceivesRepliesAsync(correlationId, ResponseFormat.Xml, 5);
                peekResults.Should().ContainMatch("*" + CimMessageConstants.ConfirmRequestChangeOfPriceListRootElement + "*");
                peekResults.Should().ContainMatch("*" + CimMessageConstants.RejectRequestChangeOfPriceListRootElement + "*");
                peekResults.Should().ContainMatch("*It is not allowed to change the tax indicator to Tax for charge*");
                peekResults.Should().ContainMatch("*" + CimMessageConstants.NotifyPriceListRootElement + "*");

                var notification = peekResults.First(x => x.Contains("NotifyPriceList_MarketDocument"));
                var notificationOperations =
                    CIMXmlReader.GetActivityRecordElements(
                        notification,
                        CimMessageConstants.ChargeType,
                        CimMessageConstants.ChargeDescription);
                notificationOperations.Should().HaveCount(1);
            }

            /* INTEGRATION EVENTS */

            [Fact]
            public async Task Ingestion_ValidChargeInformationRequest_ChargeCreatedIntegrationEventIsPublished()
            {
                // Arrange
                var (request, correlationId) = Fixture.AsSystemOperator.PrepareHttpPostRequestWithAuthorization(
                    EndpointUrl, ChargeInformationRequests.TariffAsSystemOperator);
                using var eventualChargeCreatedEvent = await Fixture
                    .ChargeCreatedListener
                    .ListenForMessageAsync(correlationId)
                    .ConfigureAwait(false);

                // Act
                var actual = await Fixture.HostManager.HttpClient.SendAsync(request);

                // Assert
                actual.StatusCode.Should().Be(HttpStatusCode.Accepted);

                var isChargeCreatedReceived = eventualChargeCreatedEvent
                    .MessageAwaiter!
                    .Wait(TimeSpan.FromSeconds(SecondsToWaitForIntegrationEvents));
                isChargeCreatedReceived.Should().BeTrue();
            }

            /* SAMPLES */

            [Theory]
            [InlineAutoMoqData(ChargeInformationRequests.ChargeInformationTariffHourlySample, 4, 1)]
            [InlineAutoMoqData(ChargeInformationRequests.BundledChargeInformationSample, 12, 3)]
            [InlineAutoMoqData(ChargeInformationRequests.ChargeInformationSubscriptionMonthlySample, 4, 1)]
            [InlineAutoMoqData(ChargeInformationRequests.ChargeInformationFeeMonthlySample, 4, 1)]
            public async Task Ingestion_ChargeInformationSampleFile_ConfirmationReceivedByGridAccessProvider(
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

                // We expect 4 peek results:
                // * Confirmation to Grid Access Provider
                // * 3x Notifications to Energy Suppliers
                var peekResults = await Fixture.MessageHubMock
                    .AssertPeekReceivesRepliesAsync(correlationId, ResponseFormat.Xml, noOfDataAvailableNotificationsExpected);

                peekResults.Count.Should().Be(4);
                peekResults.Should().ContainMatch("*" + CimMessageConstants.ConfirmRequestChangeOfPriceListRootElement + "*");
                peekResults.Should().ContainMatch("*" + CimMessageConstants.NotifyPriceListRootElement + "*");
                peekResults.Should().NotContainMatch("*" + CimMessageConstants.RejectRequestChangeOfPriceListRootElement + "*");

                var peekResult = peekResults.Single(x => x.Contains(CimMessageConstants.ConfirmRequestChangeOfPriceListRootElement));
                var operations = CIMXmlReader.GetActivityRecordElements(
                    peekResult, CimMessageConstants.MarketActivityRecord, CimMessageConstants.OriginalTransactionId);
                operations.Count.Should().Be(noOfConfirmedActivityRecordsExpected);
            }
        }
    }
}
