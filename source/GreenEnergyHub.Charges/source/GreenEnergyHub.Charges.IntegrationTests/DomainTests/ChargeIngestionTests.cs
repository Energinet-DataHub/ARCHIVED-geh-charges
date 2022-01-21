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
using System.Net;
using System.Threading.Tasks;
using Energinet.DataHub.Core.FunctionApp.TestCommon;
using FluentAssertions;
using GreenEnergyHub.Charges.Domain.MarketParticipants;
using GreenEnergyHub.Charges.IntegrationTests.Fixtures;
using GreenEnergyHub.Charges.IntegrationTests.TestFiles.Charges;
using GreenEnergyHub.Charges.IntegrationTests.TestHelpers;
using Xunit;
using Xunit.Abstractions;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.IntegrationTests.DomainTests
{
    [IntegrationTest]
    public class ChargeIngestionTests
    {
        private const int SecondsToWaitForIntegrationEvents = 15;

        [Collection(nameof(ChargesFunctionAppCollectionFixture))]
        public class RunAsync : FunctionAppTestBase<ChargesFunctionAppFixture>, IAsyncLifetime
        {
            private readonly HttpRequestGenerator _httpRequestGenerator;

            public RunAsync(ChargesFunctionAppFixture fixture, ITestOutputHelper testOutputHelper)
                : base(fixture, testOutputHelper)
            {
                var dbc = fixture.DatabaseManager.CreateDbContext();
                dbc.MarketParticipants.Add(new MarketParticipant(
                    new Guid("ed6c94f3-24a8-43b3-913d-bf7513390a32"),
                    "81502664",
                    true,
                    MarketParticipantRole.GridAccessProvider));
                dbc.SaveChanges();

                _httpRequestGenerator = new HttpRequestGenerator(fixture, "api/ChargeIngestion");
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
            public async Task When_ChargeIsReceived_Then_AHttp200ResponseIsReturned()
            {
                var request = await _httpRequestGenerator.CreateHttpRequestAsync(ChargeDocument.AnyValid);

                var actualResponse = await Fixture.HostManager.HttpClient.SendAsync(request.Request);

                actualResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            }

            [Fact]
            public async Task When_InvalidChargeIsReceived_Then_AHttp400ResponseIsReturned()
            {
                var request = await _httpRequestGenerator.CreateHttpRequestAsync(ChargeDocument.TariffInvalidSchema);
                var actualResponse = await Fixture.HostManager.HttpClient.SendAsync(request.Request);
                actualResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            }

            // TODO: Should this (and all other "when charge" tests) be split in subscription, fee and tariff?
            [Fact]
            public async Task When_ChargeIsReceived_Then_ChargeCreatedIntegrationEventIsPublished()
            {
                // Arrange
                var (request, correlationId) = await _httpRequestGenerator.CreateHttpRequestAsync(ChargeDocument.AnyValid);
                using var eventualChargeCreatedEvent = await Fixture
                    .ChargeCreatedListener
                    .ListenForMessageAsync(correlationId)
                    .ConfigureAwait(false);

                // Act
                await Fixture.HostManager.HttpClient.SendAsync(request);

                // Assert
                var isChargeCreatedReceived = eventualChargeCreatedEvent.MessageAwaiter!.Wait(
                    TimeSpan.FromSeconds(SecondsToWaitForIntegrationEvents));
                isChargeCreatedReceived.Should().BeTrue();
            }

            [Fact]
            public async Task When_ChargeBundleWithChargesIncludingPriceIsReceived_Then_ChargePricesUpdatedIntegrationEventsArePublished()
            {
                // Arrange
                var (request, correlationId) = await _httpRequestGenerator.CreateHttpRequestAsync(ChargeDocument.TariffBundleWithValidAndInvalid);
                using var eventualChargePriceUpdatedEvent = await Fixture
                    .ChargePricesUpdatedListener
                    .ListenForEventsAsync(correlationId, expectedCount: 2)
                    .ConfigureAwait(false);

                // Act
                await Fixture.HostManager.HttpClient.SendAsync(request);

                // Assert
                var isChargePricesUpdatedReceived = eventualChargePriceUpdatedEvent.CountdownEvent!.Wait(
                    TimeSpan.FromSeconds(SecondsToWaitForIntegrationEvents));
                isChargePricesUpdatedReceived.Should().BeTrue();
            }

            [Fact]
            public async Task Given_NewTaxBundleTariffWithPrices_When_GridAccessProviderPeeks_Then_MessageHubReceivesReply()
            {
                // Arrange
                var (request, correlationId) = await _httpRequestGenerator.CreateHttpRequestAsync(ChargeDocument.TariffBundleWithValidAndInvalid);

                // Act
                await Fixture.HostManager.HttpClient.SendAsync(request);

                // Act and assert
                // We expect three peaks, one for the charge and one for the receipt and one rejection
                await Fixture.MessageHubMock.AssertPeekReceivesReplyAsync(correlationId, 3);
            }
        }
    }
}
