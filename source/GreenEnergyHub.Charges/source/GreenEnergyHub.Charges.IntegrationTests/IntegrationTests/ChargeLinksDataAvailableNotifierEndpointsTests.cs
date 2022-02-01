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
using System.Text.Json;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Energinet.DataHub.Core.FunctionApp.TestCommon;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeLinksAcceptedEvents;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeLinksCommands;
using GreenEnergyHub.Charges.FunctionHost.ChargeLinks.MessageHub;
using GreenEnergyHub.Charges.IntegrationTests.Fixtures.FunctionApp;
using GreenEnergyHub.Charges.IntegrationTests.TestCommon;
using GreenEnergyHub.Charges.IntegrationTests.TestHelpers;
using GreenEnergyHub.Charges.TestCore.Attributes;
using NodaTime;
using Xunit;
using Xunit.Abstractions;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.IntegrationTests.IntegrationTests
{
    [IntegrationTest]
    public class ChargeLinksDataAvailableNotifierEndpointsTests
    {
        [Collection(nameof(ChargesFunctionAppCollectionFixture))]
        public class RunAsync : FunctionAppTestBase<ChargesFunctionAppFixture>, IAsyncLifetime
        {
            public RunAsync(ChargesFunctionAppFixture fixture, ITestOutputHelper testOutputHelper)
                : base(fixture, testOutputHelper)
            {
            }

            public Task DisposeAsync()
            {
                Fixture.MessageHubMock.Clear();
                return Task.CompletedTask;
            }

            public Task InitializeAsync()
            {
                return Task.CompletedTask;
            }

            [Theory]
            [InlineAutoMoqData]
            public async Task When_ChargeLinksAcceptedEvent_Then_Publish_ChargeLinksDataAvailableNotifiedEvent(
                ChargeLinksCommand chargeLinksCommand)
            {
                // Arrange
                var message = CreateServiceBusMessage(chargeLinksCommand, out var correlationId, out var parentId);

                await MockTelemetryClient.WrappedOperationWithTelemetryDependencyInformationAsync(
                    () => Fixture.ChargeLinksAcceptedTopic.SenderClient.SendMessageAsync(message), correlationId, parentId);

                await FunctionAsserts.AssertHasExecutedAsync(Fixture.HostManager, nameof(ChargeLinkDataAvailableNotifierEndpoint)).ConfigureAwait(false);
            }

            private ServiceBusMessage CreateServiceBusMessage(
                ChargeLinksCommand command,
                out string correlationId,
                out string parentId)
            {
                correlationId = CorrelationIdGenerator.Create();
                var message = new ChargeLinksAcceptedEvent(command, Instant.FromDateTimeUtc(DateTime.UtcNow));
                parentId = $"00-{correlationId}-b7ad6b7169203331-02";

                var data = JsonSerializer.Serialize(message);

                var serviceBusMessage = new ServiceBusMessage(data)
                {
                    CorrelationId = correlationId,
                };
                serviceBusMessage.ApplicationProperties.Add("OperationTimestamp", DateTime.UtcNow);
                serviceBusMessage.ApplicationProperties.Add("OperationCorrelationId", "1bf1b76337f14b78badc248a3289d022");
                serviceBusMessage.ApplicationProperties.Add("MessageVersion", 1);
                serviceBusMessage.ApplicationProperties.Add("MessageType", "ChargeLinksAcceptedEvent");
                serviceBusMessage.ApplicationProperties.Add("EventIdentification", "2542ed0d242e46b68b8b803e93ffbf7c");
                return serviceBusMessage;
            }
        }
    }
}
