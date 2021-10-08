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
using FluentAssertions;
using FluentAssertions.Execution;
using GreenEnergyHub.Charges.IntegrationTests.Fixtures;
using GreenEnergyHub.Charges.IntegrationTests.TestHelpers;
using GreenEnergyHub.FunctionApp.TestCommon;
using GreenEnergyHub.FunctionApp.TestCommon.ServiceBus.ListenerMock;
using NodaTime;
using Xunit;
using Xunit.Abstractions;

namespace GreenEnergyHub.Charges.IntegrationTests.Functions.Charges
{
    /// <summary>
    /// Proof-of-concept on integration testing a function.
    /// </summary>
    public class ChargeIngestionTests
    {
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
                Fixture.ServiceBusListenerMock.ResetMessageHandlersAndReceivedMessages();
                return Task.CompletedTask;
            }

            [Fact]
            public async Task When_CallingChargeIngestion_Then_RequestIsProcessedAndMessageIsSendToPostOffice()
            {
                // Arrange
                var testFilePath = "TestFiles/ValidCreateTariffCommand.json";
                var clock = SystemClock.Instance;
                var chargeJson = EmbeddedResourceHelper.GetInputJson(testFilePath, clock);

                var request = new HttpRequestMessage(HttpMethod.Post, "api/ChargeIngestion");
                var expectedBody = chargeJson;
                request.Content = new StringContent(expectedBody, Encoding.UTF8, "application/json");

                var body = string.Empty;
                using var isMessageReceivedEvent = await Fixture.ServiceBusListenerMock
                    .WhenAny()
                    .VerifyOnceAsync(receivedMessage =>
                    {
                        body = receivedMessage.Body.ToString();

                        return Task.CompletedTask;
                    });

                // Act
                var actualResponse = await Fixture.HostManager.HttpClient.SendAsync(request);

                // Assert
                using var assertionScope = new AssertionScope();

                // => Http response
                actualResponse.StatusCode.Should().Be(HttpStatusCode.OK);

                // => Service Bus (timeout should not be more than 5 secs. - currently it's high so we can break during demo).
                var isMessageReceived = isMessageReceivedEvent.Wait(TimeSpan.FromSeconds(120));
                isMessageReceived.Should().BeTrue();

                body.Should().NotBeEmpty();
            }
        }
    }
}
