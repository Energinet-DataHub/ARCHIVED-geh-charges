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
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Energinet.DataHub.Core.FunctionApp.TestCommon;
using Energinet.DataHub.Core.FunctionApp.TestCommon.ServiceBus.ListenerMock;
using FluentAssertions;
using GreenEnergyHub.Charges.IntegrationTests.Fixtures;
using GreenEnergyHub.Charges.IntegrationTests.TestFiles.Charges;
using GreenEnergyHub.Charges.IntegrationTests.TestHelpers;
using NodaTime;
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
                return Task.CompletedTask;
            }

            [Fact]
            public async Task When_ChargeIsReceived_Then_AHttp200ResponseIsReturned()
            {
                var request = CreateHttpRequest(ChargeDocument.AnyValid, out string _);

                var actualResponse = await Fixture.HostManager.HttpClient.SendAsync(request);

                actualResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            }

            // TODO: Should this (and all other "when charge" tests) be split in subscription, fee and tariff?
            [Fact]
            public async Task When_ChargeIsReceived_Then_ChargeCreatedIntegrationEventIsPublished()
            {
                // Arrange
                var request = CreateHttpRequest(ChargeDocument.AnyValid, out string correlationId);
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
            public async Task When_ChargeIncludingPriceIsReceived_Then_ChargePricesUpdatedIntegrationEventIsPublished()
            {
                // Arrange
                var request = CreateHttpRequest(ChargeDocument.AnyWithPrice, out string correlationId);
                using var eventualChargePriceUpdatedEvent = await Fixture
                    .ChargePricesUpdatedListener
                    .ListenForMessageAsync(correlationId)
                    .ConfigureAwait(false);

                // Act
                await Fixture.HostManager.HttpClient.SendAsync(request);

                // Assert
                var isChargePricesUpdatedReceived = eventualChargePriceUpdatedEvent.MessageAwaiter!.Wait(
                    TimeSpan.FromSeconds(SecondsToWaitForIntegrationEvents));
                isChargePricesUpdatedReceived.Should().BeTrue();
            }

            [Fact]
            public async Task Given_NewTaxTariffWithPrices_When_GridAccessProviderPeeks_Then_MessageHubReceivesReply()
            {
                // Arrange
                var request = CreateHttpRequest(ChargeDocument.TaxTariffWithPrice, out var correlationId);

                // Act
                await Fixture.HostManager.HttpClient.SendAsync(request);

                // Act and assert
                // We expect two peaks, one for the charge and one for the receipt
                await Fixture.MessageHubMock.AssertPeekReceivesReplyAsync(correlationId, 2);
            }

            private static HttpRequestMessage CreateHttpRequest(
                string testFilePath,
                out string correlationId)
            {
                var clock = SystemClock.Instance;
                var chargeJson = EmbeddedResourceHelper.GetEmbeddedFile(testFilePath, clock);
                correlationId = CorrelationIdGenerator.Create();

                var request = new HttpRequestMessage(HttpMethod.Post, "api/ChargeIngestion")
                {
                    Content = new StringContent(chargeJson, Encoding.UTF8, "application/xml"),
                };
                request.ConfigureTraceContext(correlationId);

                return request;
            }
        }
    }
}
