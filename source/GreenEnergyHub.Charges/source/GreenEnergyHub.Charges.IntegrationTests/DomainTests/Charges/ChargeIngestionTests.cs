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
using GreenEnergyHub.Charges.IntegrationTests.TestHelpers;
using NodaTime;
using Xunit;
using Xunit.Abstractions;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.IntegrationTests.DomainTests.Charges
{
    /// <summary>
    /// Proof-of-concept on integration testing a function.
    /// </summary>
    [IntegrationTest]
    public class ChargeIngestionTests
    {
        [Collection(nameof(ChargesFunctionAppCollectionFixture))]
        public class RunAsync : FunctionAppTestBase<ChargesFunctionAppFixture>
        {
            public RunAsync(ChargesFunctionAppFixture fixture, ITestOutputHelper testOutputHelper)
                : base(fixture, testOutputHelper)
            {
            }

            [Fact]
            public async Task When_ChargeIsReceived_Then_AHttp200ResponseIsReturned()
            {
                var request = CreateTariffWithPricesRequest(out string _);

                var actualResponse = await Fixture.HostManager.HttpClient.SendAsync(request);

                actualResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            }

            /// <summary>
            /// Test of old Message Hub simulating code. Will be refactored in upcoming stories.
            /// </summary>
            [Fact]
            public async Task When_ChargeIsReceived_Then_MessageIsSentToPostOffice()
            {
                // Arrange
                Fixture.PostOfficeListener.ResetMessageHandlersAndReceivedMessages();
                var request = CreateTariffWithPricesRequest(out string _);

                var body = string.Empty;
                using var isMessageReceivedEvent = await Fixture.PostOfficeListener
                    .WhenAny()
                    .VerifyOnceAsync(receivedMessage =>
                    {
                        body = receivedMessage.Body.ToString();

                        return Task.CompletedTask;
                    });

                // Act
                await Fixture.HostManager.HttpClient.SendAsync(request);

                // Assert
                var isMessageReceived = isMessageReceivedEvent.Wait(TimeSpan.FromSeconds(5));
                isMessageReceived.Should().BeTrue();
                body.Should().NotBeEmpty();
            }

            [Fact]
            public async Task When_ChargeIsReceived_Then_ChargeCreatedIntegrationEventIsPublished()
            {
                // Arrange
                var request = CreateTariffWithPricesRequest(out string correlationId);
                Fixture.ChargeCreatedListener.Reset();
                using var eventualChargeCreatedEvent = await Fixture
                    .ChargeCreatedListener
                    .ListenForMessageAsync(correlationId)
                    .ConfigureAwait(false);

                // Act
                await Fixture.HostManager.HttpClient.SendAsync(request);

                // Assert
                var isChargeCreatedReceived = eventualChargeCreatedEvent.MessageAwaiter!.Wait(TimeSpan.FromSeconds(10));
                isChargeCreatedReceived.Should().BeTrue();
            }

            [Fact]
            public async Task When_ChargeIncludingPriceIsReceived_Then_ChargePricesUpdatedIntegrationEventIsPublished()
            {
                // Arrange
                var request = CreateTariffWithPricesRequest(out string correlationId);
                Fixture.ChargePricesUpdatedListener.Reset();
                using var eventualChargePriceUpdatedEvent = await Fixture
                    .ChargePricesUpdatedListener
                    .ListenForMessageAsync(correlationId)
                    .ConfigureAwait(false);

                // Act
                await Fixture.HostManager.HttpClient.SendAsync(request);

                // Assert
                var isChargePricesUpdatedReceived = eventualChargePriceUpdatedEvent.MessageAwaiter!.Wait(TimeSpan.FromSeconds(10));
                isChargePricesUpdatedReceived.Should().BeTrue();
            }

            private static HttpRequestMessage CreateTariffWithPricesRequest(
                out string correlationId)
            {
                var testFilePath = "TestFiles/Charges/ValidCreateTariffCommand.xml";
                var clock = SystemClock.Instance;
                var chargeJson = EmbeddedResourceHelper.GetEmbeddedFile(testFilePath, clock);
                correlationId = CreateCorrelationId();
                var traceParent = CreateTraceParentHttpHeaderValue(correlationId);

                var request = new HttpRequestMessage(HttpMethod.Post, "api/ChargeIngestion")
                {
                    Content = new StringContent(chargeJson, Encoding.UTF8, "application/xml"),
                };

                // See https://tsuyoshiushio.medium.com/correlation-with-activity-with-application-insights-3-w3c-tracecontext-d9fb143c0ce2
                request.Headers.Add("traceparent", traceParent);

                return request;
            }

            private static string CreateTraceParentHttpHeaderValue(string correlationId)
            {
                return $"00-{correlationId}-b7ad6b7169203331-01";
            }

            private static string CreateCorrelationId()
            {
                return Guid.NewGuid().ToString().Replace("-", string.Empty);
            }
        }
    }
}
