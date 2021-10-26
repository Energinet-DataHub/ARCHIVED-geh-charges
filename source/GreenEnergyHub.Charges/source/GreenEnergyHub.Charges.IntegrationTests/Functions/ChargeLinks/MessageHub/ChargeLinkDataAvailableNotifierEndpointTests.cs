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
using FluentAssertions.Execution;
using GreenEnergyHub.Charges.IntegrationTests.Fixtures;
using GreenEnergyHub.Charges.IntegrationTests.TestHelpers;
using NodaTime;
using Xunit;
using Xunit.Abstractions;

namespace GreenEnergyHub.Charges.IntegrationTests.Functions.ChargeLinks.MessageHub
{
    public class ChargeLinkDataAvailableNotifierEndpointTests
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
                Fixture.DataAvailableListenerMock.ResetMessageHandlersAndReceivedMessages();
                return Task.CompletedTask;
            }

            [Fact]
            public async Task When_ReceivingChargeLinkMessage_Then_MessageHubIsNotifiedAboutAvailableData()
            {
                // Arrange
                var testFilePath = "TestFiles/CreateFixedPeriodTariffChargeLink.xml";
                var clock = SystemClock.Instance;
                var messageString = EmbeddedResourceHelper.GetEmbeddedFile(testFilePath, clock);

                var request = new HttpRequestMessage(HttpMethod.Post, "api/ChargeLinkIngestion");
                var expectedBody = messageString;
                request.Content = new StringContent(expectedBody, Encoding.UTF8, "application/json");

                using var isMessageReceivedEvent = await Fixture.DataAvailableListenerMock
                    .WhenAny()
                    .VerifyOnceAsync(_ => Task.CompletedTask);

                // Act
                var actualResponse = await Fixture.HostManager.HttpClient.SendAsync(request);

                // Assert
                using var assertionScope = new AssertionScope();

                // => Http response
                actualResponse.StatusCode.Should().Be(HttpStatusCode.OK);

                // => Service Bus (timeout should not be more than 5 secs. - currently it's high so we can break during demo).
                var isMessageReceived = isMessageReceivedEvent.Wait(TimeSpan.FromSeconds(200));
                isMessageReceived.Should().BeTrue();
            }
        }
    }
}
