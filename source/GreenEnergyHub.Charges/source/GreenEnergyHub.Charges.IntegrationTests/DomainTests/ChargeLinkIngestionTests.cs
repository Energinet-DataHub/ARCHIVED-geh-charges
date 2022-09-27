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

using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Energinet.DataHub.Core.FunctionApp.TestCommon;
using FluentAssertions;
using FluentAssertions.Execution;
using GreenEnergyHub.Charges.IntegrationTest.Core.Fixtures.FunctionApp;
using GreenEnergyHub.Charges.IntegrationTest.Core.MessageHub;
using GreenEnergyHub.Charges.IntegrationTest.Core.TestFiles.ChargeLinks;
using GreenEnergyHub.Charges.IntegrationTest.Core.TestHelpers;
using GreenEnergyHub.Charges.IntegrationTests.Fixtures;
using GreenEnergyHub.Charges.TestCore;
using GreenEnergyHub.Charges.TestCore.TestHelpers;
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
                Fixture.MessageHubMock.Reset();
                return Task.CompletedTask;
            }

            [Fact]
            public async Task When_RequestIsUnauthenticated_Then_AHttp401UnauthorizedIsReturned()
            {
                var request = HttpRequestGenerator.CreateHttpPostRequest(
                    EndpointUrl,
                    ChargeLinkDocument.AnyValid,
                    ZonedDateTimeServiceHelper.GetZonedDateTimeService(InstantHelper.GetTodayAtMidnightUtc()));

                var actual = await Fixture.HostManager.HttpClient.SendAsync(request);

                actual.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            }

            [Fact]
            public async Task Given_NewMessage_When_SenderIdDoesNotMatchAuthenticatedId_Then_ShouldReturnErrorMessage()
            {
                // Arrange
                var (request, _) = Fixture.AsGridAccessProvider.PrepareHttpPostRequestWithAuthorization(
                    EndpointUrl, ChargeLinkDocument.ChargeLinkDocumentWhereSenderIdDoNotMatchAuthorizedActorId);

                // Act
                var actual = await Fixture.HostManager.HttpClient.SendAsync(request);

                // Assert
                actual.StatusCode.Should().Be(HttpStatusCode.BadRequest);
                var errorMessage = await actual.Content.ReadAsStreamAsync();
                var document = await XDocument.LoadAsync(errorMessage, LoadOptions.None, CancellationToken.None);
                document.Element("Message")?.Value.Should().Be(ErrorMessageConstants.ActorIsNotWhoTheyClaimToBeErrorMessage);
            }

            [Fact]
            public async Task When_ChargeLinkIsReceived_Then_AHttp202ResponseWithEmptyBodyIsReturned()
            {
                var (request, _) = Fixture.AsGridAccessProvider.PrepareHttpPostRequestWithAuthorization(
                    EndpointUrl, ChargeLinkDocument.AnyValid);

                var actualResponse = await Fixture.HostManager.HttpClient.SendAsync(request);
                var responseBody = await actualResponse.Content.ReadAsStringAsync();

                actualResponse.StatusCode.Should().Be(HttpStatusCode.Accepted);
                responseBody.Should().BeEmpty();
            }

            [Fact]
            public async Task When_InvalidChargeLinkIsReceived_Then_AHttp400ResponseIsReturned()
            {
                // Arrange
                var (request, _) = Fixture.AsGridAccessProvider.PrepareHttpPostRequestWithAuthorization(
                    EndpointUrl, ChargeLinkDocument.InvalidSchema);

                // Act
                var actualResponse = await Fixture.HostManager.HttpClient.SendAsync(request);

                // Assert
                actualResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
                var responseAsString = await actualResponse.Content.ReadAsStringAsync();
                responseAsString.Should().Contain("type' element is invalid - The value 'InvalidType' is invalid according to its datatype");
            }

            [Fact]
            public async Task Given_NewTaxChargeLinkMessage_When_GridAccessProviderPeeks_Then_MessageHubReceivesReply()
            {
                // Arrange
                var (request, correlationId) = Fixture.AsGridAccessProvider.PrepareHttpPostRequestWithAuthorization(
                    EndpointUrl, ChargeLinkDocument.TaxWithCreateAndUpdateDueToOverLappingPeriod);

                // Act
                await Fixture.HostManager.HttpClient.SendAsync(request);

                // Assert
                // We expect 3 message types in the MessageHub, one for the receipt,
                // one for the charge link itself and one rejected
                using var assertionScope = new AssertionScope();
                var peekResults = await Fixture.MessageHubMock.AssertPeekReceivesRepliesAsync(correlationId, 3);
                peekResults.Should().ContainMatch("*ConfirmRequestChangeBillingMasterData_MarketDocument*");
                peekResults.Should().ContainMatch("*NotifyBillingMasterData_MarketDocument*");

                // For now, ChargeLinkCommandConverter splits all CIM MktActivityRecord into separate ChargeLinkCommands
                // so we do not always receive a rejection due to the parallel handling of commands.
                if (peekResults.Any(s => s.Contains("RejectRequestChangeBillingMasterData_MarketDocument")))
                    peekResults.Should().ContainMatch("*cannot yet be updated or stopped. The functionality is not implemented yet*");
            }

            [Fact]
            public async Task Given_ChargeLinkSampleFile_When_GridAccessProviderPeeks_Then_MessageHubReceivesReply()
            {
                // Arrange
                var (request, correlationId) = Fixture.AsGridAccessProvider.PrepareHttpPostRequestWithAuthorization(
                    EndpointUrl, ChargeLinkDocument.ChargeLinkSubscriptionSample);

                // Act
                var actual = await Fixture.HostManager.HttpClient.SendAsync(request);

                // Assert
                actual.StatusCode.Should().Be(HttpStatusCode.Accepted);

                using var assertionScope = new AssertionScope();
                var peekResults = await Fixture.MessageHubMock.AssertPeekReceivesRepliesAsync(correlationId);
                peekResults.Should().ContainMatch("*ConfirmRequestChangeBillingMasterData_MarketDocument*");
                peekResults.Should().NotContainMatch("*Reject*");
            }
        }
    }
}
