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
using System.Collections.Generic;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Energinet.DataHub.Core.FunctionApp.TestCommon;
using FluentAssertions;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeLinksAcceptedEvents;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeLinksCommands;
using GreenEnergyHub.Charges.Infrastructure.Core.MessageMetaData;
using GreenEnergyHub.Charges.IntegrationTest.Core.Fixtures.FunctionApp;
using GreenEnergyHub.Charges.IntegrationTest.Core.TestHelpers;
using GreenEnergyHub.Charges.IntegrationTests.Fixtures;
using GreenEnergyHub.Charges.TestCore.Builders.Command;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using Xunit;
using Xunit.Abstractions;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.IntegrationTests.IntegrationTests.EndpointTests.ChargeLinks
{
    [IntegrationTest]
    public class CreateDefaultChargeLinksReplierEndpointTests
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
                Fixture.MessageHubMock.Reset();
                return Task.CompletedTask;
            }

            public Task InitializeAsync()
            {
                return Task.CompletedTask;
            }

            [Fact]
            public async Task When_ChargeLinksAcceptedEvent_Then_CreateLinkReply()
            {
                // Arrange
                await using var context = Fixture.ChargesDatabaseManager.CreateDbContext();
                var charge = await context.Charges.FirstAsync();
                var marketParticipant = await context.MarketParticipants.SingleAsync(mp => mp.Id == charge.OwnerId);
                var links = new List<ChargeLinkOperationDto>
                {
                    new ChargeLinkDtoBuilder()
                        .WithCharge(charge.SenderProvidedChargeId, charge.Type, marketParticipant.MarketParticipantId)
                        .WithMeteringPointId("571313180000000005").Build(),
                };
                var command = new ChargeLinksCommandBuilder().WithChargeLinks(links).Build();
                var correlationId = CorrelationIdGenerator.Create();
                var message = CreateServiceBusMessage(command, correlationId);

                using var isMessageReceived = await Fixture.CreateLinkReplyQueueListener
                    .ListenForMessageAsync(correlationId)
                    .ConfigureAwait(false);

                // Act
                await MockTelemetryClient.WrappedOperationWithTelemetryDependencyInformationAsync(
                    () => Fixture.ChargesDomainEventTopic.SenderClient.SendMessageAsync(message), correlationId);

                // Assert
                var isMessageReceivedByQueue = isMessageReceived.MessageAwaiter!.Wait(TimeSpan.FromSeconds(120));
                isMessageReceivedByQueue.Should().BeTrue();
                isMessageReceived.ApplicationProperties![MessageMetaDataConstants.CorrelationId]
                    .Should().Be(correlationId);
            }

            private ServiceBusMessage CreateServiceBusMessage(ChargeLinksCommand command, string correlationId)
            {
                var chargeLinksAcceptedEvent = new ChargeLinksAcceptedEvent(
                    command, Instant.FromDateTimeUtc(DateTime.UtcNow));

                return ServiceBusMessageGenerator.CreateServiceBusMessage(
                    chargeLinksAcceptedEvent,
                    correlationId,
                    Fixture.CreateLinkReplyQueue.Name);
            }
        }
    }
}
