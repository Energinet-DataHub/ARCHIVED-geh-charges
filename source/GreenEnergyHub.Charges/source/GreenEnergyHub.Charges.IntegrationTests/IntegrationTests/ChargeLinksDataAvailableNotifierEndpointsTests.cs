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
using System.Collections.Generic;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Energinet.DataHub.Core.FunctionApp.TestCommon;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeLinksAcceptedEvents;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeLinksCommands;
using GreenEnergyHub.Charges.Domain.Dtos.SharedDtos;
using GreenEnergyHub.Charges.Domain.MarketParticipants;
using GreenEnergyHub.Charges.FunctionHost.ChargeLinks;
using GreenEnergyHub.Charges.FunctionHost.ChargeLinks.MessageHub;
using GreenEnergyHub.Charges.IntegrationTests.Fixtures.FunctionApp;
using GreenEnergyHub.Charges.IntegrationTests.TestCommon;
using GreenEnergyHub.Charges.IntegrationTests.TestHelpers;
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

            [Fact]
            public async Task When_ChargeLinksAcceptedEvent_Then_Publish_ChargeLinksDataAvailableNotifiedEvent()
            {
                // Arrange
                var documentDto = new DocumentDto
                {
                    Id = "4ED89659-6F88-4F52-BC7B-8987B304A071",
                    Sender = new MarketParticipantDto
                    {
                        BusinessProcessRole = MarketParticipantRole.SystemOperator,
                        Id = "5790000432752",
                    },
                    Type = DocumentType.RequestChangeBillingMasterData,
                    IndustryClassification = IndustryClassification.Electricity,
                    BusinessReasonCode = BusinessReasonCode.UpdateChargeInformation,
                    RequestDate = Instant.FromDateTimeUtc(DateTime.UtcNow),
                    CreatedDateTime = Instant.FromDateTimeUtc(DateTime.UtcNow),
                };

                var command = new ChargeLinksCommand("571313180000000005", documentDto, new List<ChargeLinkDto>());

                var message = CreateServiceBusMessage(command, out var correlationId, out var parentId);

                await MockTelemetryClient.WrappedOperationWithTelemetryDependencyInformationAsync(
                    () => Fixture.ChargeLinksAcceptedTopic.SenderClient.SendMessageAsync(message), correlationId, parentId);

                await FunctionAsserts.AssertHasExecutedAsync(Fixture.HostManager, nameof(ChargeLinkDataAvailableNotifierEndpoint)).ConfigureAwait(false);
                await FunctionAsserts.AssertHasExecutedAsync(Fixture.HostManager, nameof(CreateDefaultChargeLinksReplierEndpoint)).ConfigureAwait(false);
            }

            private ServiceBusMessage CreateServiceBusMessage(
                ChargeLinksCommand command,
                out string correlationId,
                out string parentId)
            {
                correlationId = CorrelationIdGenerator.Create();
                var message = new ChargeLinksAcceptedEvent(command, Instant.FromDateTimeUtc(DateTime.UtcNow));
                parentId = $"00-{correlationId}-b7ad6b7169203331-02";

                var jsonSerializer = new Json.JsonSerializer();
                var data = jsonSerializer.Serialize(message);

                var serviceBusMessage = new ServiceBusMessage(data)
                {
                    CorrelationId = correlationId,
                };
                serviceBusMessage.ApplicationProperties.Add("ReplyTo", Fixture.CreateLinkReplyQueue.Name);
                return serviceBusMessage;
            }
        }
    }
}
