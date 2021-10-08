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
using System.Threading.Tasks;
using FluentAssertions;
using GreenEnergyHub.Charges.IntegrationTests.Fixtures;
using GreenEnergyHub.Charges.TestCore.Attributes;
using GreenEnergyHub.FunctionApp.TestCommon;
using GreenEnergyHub.FunctionApp.TestCommon.ServiceBus.ListenerMock;
using Xunit;
using Xunit.Abstractions;

namespace GreenEnergyHub.Charges.IntegrationTests.Functions.ChargeLinks.PostOffice
{
    public class ChargeLinkCreatedDataAvailableNotifierEndpointTests
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
                // TODO: Send CIM XML

                // Arrange
                // TODO: How to seed database with charge corresponding with inputContract?
                var body = Array.Empty<byte>();
                using var isMessageReceivedEvent = await Fixture.ServiceBusListenerMock
                    .WhenAny()
                    .VerifyOnceAsync(receivedMessage =>
                    {
                        // body = receivedMessage.Body;
                        return Task.CompletedTask;
                    });

                // Act
                // TODO: How to add inputContract to input queue?
                //await Fixture.ChargeLinkCommandAcceptedServiceBusListenerMock...

                // Assert
                // => Service Bus (timeout should not be more than 5 secs).
                var isMessageReceived = isMessageReceivedEvent.Wait(TimeSpan.FromSeconds(5));
                isMessageReceived.Should().BeTrue();

                body.Should().NotBeEmpty();
            }
        }
    }
}
