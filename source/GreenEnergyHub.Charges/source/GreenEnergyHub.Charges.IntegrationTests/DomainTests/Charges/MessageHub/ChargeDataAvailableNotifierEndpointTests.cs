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
using Energinet.DataHub.MessageHub.Client.DataAvailable;
using Energinet.DataHub.MessageHub.Client.Model;
using FluentAssertions;
using GreenEnergyHub.Charges.IntegrationTests.Fixtures;
using GreenEnergyHub.Charges.IntegrationTests.TestHelpers;
using NodaTime;
using Xunit;
using Xunit.Abstractions;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.IntegrationTests.DomainTests.Charges.MessageHub
{
    /// <summary>
    /// This test that when receiving a valid charge, a notification is send and valid bundle is created for the message hub.
    /// </summary>
    [IntegrationTest]
    public class ChargeDataAvailableNotifierEndpointTests
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
                Fixture.MessageHubDataAvailableListenerMock.ResetMessageHandlersAndReceivedMessages();
                Fixture.MessageHubBundleResponseListenerMock.ResetMessageHandlersAndReceivedMessages();
                return Task.CompletedTask;
            }

            [Fact]
            public async Task When_ReceivingChargeLinkMessage_MessageHubIsNotifiedAboutAvailableData_And_Then_When_MessageHubRequestsTheBundle_Then_MessageHubReceivesBundleResponse()
            {
                // Arrange
                var testFilePath = "TestFiles/ValidCreateTariffCommand.xml";
                var clock = SystemClock.Instance;
                var messageString = EmbeddedResourceHelper.GetEmbeddedFile(testFilePath, clock);

                var request = new HttpRequestMessage(HttpMethod.Post, "api/ChargeIngestion");
                var expectedBody = messageString;
                request.Content = new StringContent(expectedBody, Encoding.UTF8, "application/json");

                // => Simulate that MessageHub requests a bundle when data available
                DataAvailableNotificationDto? dataAvailableNotification;
                using var dataAvailableAwaiter = await Fixture.MessageHubDataAvailableListenerMock
                    .WhenAny()
                    .VerifyOnceAsync(async receivedMessage =>
                    {
                        dataAvailableNotification =
                            new DataAvailableNotificationParser().Parse(receivedMessage.Body.ToArray());
                        await Fixture.MessageHubMock.RequestBundleAsync(dataAvailableNotification);
                    });

                // => Register the bundle response so we can assert it
                using var bundleResponseAwaiter = await Fixture.MessageHubBundleResponseListenerMock
                    .WhenAny()
                    .VerifyOnceAsync(_ => Task.CompletedTask);

                // Act
                var actualResponse = await Fixture.HostManager.HttpClient.SendAsync(request);

                // Assert

                // => Did domain respond with an HTTP OK status?
                actualResponse.StatusCode.Should().Be(HttpStatusCode.OK);

                // => Was data available notification sent from the domain?
                //    (Timeout should not be more than 5 secs. - currently it's high so we can break during demo.)
                var isDataAvailableReceived = dataAvailableAwaiter.Wait(TimeSpan.FromSeconds(15));
                isDataAvailableReceived.Should().BeTrue();

                // => Was bundle response sent from the domain?
                //   (timeout should not be more than 5 secs. - currently it's high so we can break during demo).
                var isBundleResponseReceived = dataAvailableAwaiter.Wait(TimeSpan.FromSeconds(15));
                isBundleResponseReceived.Should().BeTrue();
            }
        }
    }
}
