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
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Energinet.DataHub.Core.FunctionApp.TestCommon;
using FluentAssertions;
using FluentAssertions.Execution;
using GreenEnergyHub.Charges.Domain.Charges;
using GreenEnergyHub.Charges.Domain.Dtos.ChargePriceCommandReceivedEvents;
using GreenEnergyHub.Charges.Domain.Dtos.Events;
using GreenEnergyHub.Charges.Domain.Dtos.SharedDtos;
using GreenEnergyHub.Charges.FunctionHost.Charges;
using GreenEnergyHub.Charges.IntegrationTest.Core.Fixtures.FunctionApp;
using GreenEnergyHub.Charges.IntegrationTest.Core.TestCommon;
using GreenEnergyHub.Charges.IntegrationTest.Core.TestHelpers;
using GreenEnergyHub.Charges.IntegrationTests.Fixtures;
using GreenEnergyHub.Charges.TestCore;
using GreenEnergyHub.Charges.TestCore.Attributes;
using GreenEnergyHub.Charges.TestCore.Builders.Command;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using NodaTime.Text;
using Xunit;
using Xunit.Abstractions;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.IntegrationTests.IntegrationTests.EndpointTests
{
    [IntegrationTest]
    public class ChargePriceCommandReceiverEndpointTests
    {
        [Collection(nameof(ChargesFunctionAppCollectionFixture))]
        public class RunAsync : FunctionAppTestBase<ChargesFunctionAppFixture>, IAsyncLifetime
        {
            private static readonly string _operationsRejectedEventType = typeof(ChargePriceOperationsRejectedEvent).FullName!;

            public RunAsync(ChargesFunctionAppFixture fixture, ITestOutputHelper testOutputHelper)
                : base(fixture, testOutputHelper)
            {
            }

            public Task DisposeAsync()
            {
                Fixture.HostManager.ClearHostLog();
                Fixture.MessageHubSimulator.Clear();
                return Task.CompletedTask;
            }

            public Task InitializeAsync()
            {
                return Task.CompletedTask;
            }

            [Theory]
            [InlineAutoMoqData]
            public async Task RunAsync_WhenValidationFails_PersistsRejectedEvent(
                ChargePriceCommandBuilder commandBuilder,
                ChargePriceOperationDtoBuilder operationDtoBuilder)
            {
                // Arrange
                await using var context = Fixture.ChargesDatabaseManager.CreateDbContext();
                var invalidChargePriceCommandReceivedEvent = CreateInvalidChargePriceCommandReceivedEvent(
                    commandBuilder, operationDtoBuilder);
                var correlationId = CorrelationIdGenerator.Create();
                var message = ServiceBusMessageGenerator.CreateServiceBusMessage(
                    invalidChargePriceCommandReceivedEvent,
                    correlationId);

                // Act
                await MockTelemetryClient.WrappedOperationWithTelemetryDependencyInformationAsync(
                    () => Fixture.ChargesDomainEventTopic.SenderClient.SendMessageAsync(message), correlationId);

                await FunctionAsserts.AssertHasExecutedAsync(
                    Fixture.HostManager, nameof(ChargePriceCommandReceiverEndpoint));

                // Assert
                var actualOutboxMessage = context.OutboxMessages.Single(x => x.CorrelationId.Contains(correlationId));
                actualOutboxMessage.Type.Should().Be(_operationsRejectedEventType);
            }

            [Theory]
            [InlineAutoMoqData]
            public async Task RunAsync_WhenNewPriceSeries_ChargePricesUpdatedAndConfirmationEventIsRaised(
                ChargePriceCommandBuilder chargePriceCommandBuilder,
                DocumentDtoBuilder documentDtoBuilder,
                ChargePriceOperationDtoBuilder operationDtoBuilder)
            {
                // Arrange
                var ownerId = SeededData.GridAreaLink.Provider8100000000030.Id;
                const string ownerGln = SeededData.GridAreaLink.Provider8100000000030.MarketParticipantId;
                const string chargeId = "TestTariff";
                const ChargeType chargeType = ChargeType.Tariff;
                var correlationId = CorrelationIdGenerator.Create();

                Charge existingCharge;
                await using (var preOperationReadContext = Fixture.ChargesDatabaseManager.CreateDbContext())
                {
                    existingCharge = await preOperationReadContext.Charges.FirstAsync(
                        GetChargePredicate(chargeId, ownerId, chargeType));
                }

                var newPrices = existingCharge.Points.Select(point => new Point(point.Price + 100, point.Time)).ToList();
                var chargePriceCommandReceivedEvent = CreateChargePriceCommandReceivedEvent(
                    chargePriceCommandBuilder, documentDtoBuilder, operationDtoBuilder, chargeId, ownerGln, chargeType, newPrices);
                var message = ServiceBusMessageGenerator.CreateServiceBusMessage(
                    chargePriceCommandReceivedEvent,
                    correlationId);

                // Act
                await MockTelemetryClient.WrappedOperationWithTelemetryDependencyInformationAsync(
                    () => Fixture.ChargesDomainEventTopic.SenderClient.SendMessageAsync(message), correlationId);

                await FunctionAsserts.AssertHasExecutedAsync(
                    Fixture.HostManager, nameof(ChargePriceCommandReceiverEndpoint));

                // Assert
                await using var postOperationReadContext = Fixture.ChargesDatabaseManager.CreateDbContext();
                var actualCharge = await postOperationReadContext.Charges.FirstAsync(
                    GetChargePredicate(chargeId, ownerId, chargeType));

                using var assertionScope = new AssertionScope();
                actualCharge.Points.Should().BeEquivalentTo(newPrices);
                var actualOutboxMessage = await postOperationReadContext.OutboxMessages
                    .SingleAsync(x => x.CorrelationId.Contains(correlationId));
                actualOutboxMessage.Type.Should().Be(typeof(ChargePriceOperationsAcceptedEvent).FullName);
            }

            [Theory]
            [InlineAutoMoqData]
            public async Task RunAsync_WhenPriceSeriesIsNotChanged_ConfirmationEventIsRaised(
                ChargePriceCommandBuilder commandBuilder,
                DocumentDtoBuilder documentDtoBuilder,
                ChargePriceOperationDtoBuilder operationDtoBuilder)
            {
                // Arrange
                var ownerId = SeededData.GridAreaLink.Provider8100000000030.Id;
                const string ownerGln = SeededData.GridAreaLink.Provider8100000000030.MarketParticipantId;
                const string chargeId = "TestTariff";
                const ChargeType chargeType = ChargeType.Tariff;

                await using var preOperationReadContext = Fixture.ChargesDatabaseManager.CreateDbContext();

                var expectedCharge = await preOperationReadContext.Charges.FirstAsync(
                    GetChargePredicate(chargeId, ownerId, chargeType));

                var chargePriceCommandReceivedEvent = CreateChargePriceCommandReceivedEvent(
                    commandBuilder, documentDtoBuilder, operationDtoBuilder, chargeId, ownerGln, chargeType, expectedCharge.Points.ToList());
                var correlationId = CorrelationIdGenerator.Create();
                var message = ServiceBusMessageGenerator.CreateServiceBusMessage(
                    chargePriceCommandReceivedEvent,
                    correlationId);

                // Act
                await MockTelemetryClient.WrappedOperationWithTelemetryDependencyInformationAsync(
                    () => Fixture.ChargesDomainEventTopic.SenderClient.SendMessageAsync(message), correlationId);

                await FunctionAsserts.AssertHasExecutedAsync(
                    Fixture.HostManager, nameof(ChargePriceCommandReceiverEndpoint));

                // Assert
                await using var postOperationReadContext = Fixture.ChargesDatabaseManager.CreateDbContext();
                var actualCharge = await postOperationReadContext.Charges.FirstAsync(
                    GetChargePredicate(chargeId, ownerId, chargeType));

                using var assertionScope = new AssertionScope();
                actualCharge.Should().BeEquivalentTo(expectedCharge);
                var actualOutboxMessage = await postOperationReadContext.OutboxMessages
                    .SingleOrDefaultAsync(x => x.CorrelationId.Contains(correlationId));
                actualOutboxMessage!.Type.Should().Be(typeof(ChargePriceOperationsAcceptedEvent).FullName);
            }

            private static ChargePriceCommandReceivedEvent CreateInvalidChargePriceCommandReceivedEvent(
                ChargePriceCommandBuilder commandBuilder, ChargePriceOperationDtoBuilder operationDtoBuilder)
            {
                var invalidOperation = operationDtoBuilder.WithPoint(123456789m).Build();
                var priceCommand = commandBuilder.WithChargeOperation(invalidOperation).Build();
                var chargePriceReceivedEvent = new ChargePriceCommandReceivedEvent(
                    Instant.FromDateTimeUtc(DateTime.UtcNow), priceCommand);
                return chargePriceReceivedEvent;
            }

            private static Expression<Func<Charge, bool>> GetChargePredicate(
                string chargeId, Guid ownerId, ChargeType chargeType) =>
                x =>
                    x.SenderProvidedChargeId == chargeId &&
                    x.OwnerId == ownerId &&
                    x.Type == chargeType;

            private static ChargePriceCommandReceivedEvent CreateChargePriceCommandReceivedEvent(
                ChargePriceCommandBuilder commandBuilder,
                DocumentDtoBuilder documentDtoBuilder,
                ChargePriceOperationDtoBuilder operationDtoBuilder,
                string chargeId,
                string ownerId,
                ChargeType chargeType,
                List<Point> points)
            {
                var document = documentDtoBuilder
                    .WithBusinessReasonCode(BusinessReasonCode.UpdateChargePrices)
                    .WithDocumentType(DocumentType.RequestChangeOfPriceList)
                    .Build();

                var pointsStartDateTime = InstantPattern.ExtendedIso.Parse("2022-02-01T23:00:00Z").Value;
                var pointsEndTime = InstantPattern.ExtendedIso.Parse("2022-02-02T23:00:00Z").Value;

                operationDtoBuilder
                    .WithOwner(ownerId)
                    .WithChargeId(chargeId)
                    .WithChargeType(chargeType)
                    .WithPriceResolution(Resolution.P1D)
                    .WithPointsInterval(pointsStartDateTime, pointsEndTime)
                    .WithStartDateTime(pointsStartDateTime);

                operationDtoBuilder.WithPoints(points);

                var operation = operationDtoBuilder.Build();
                var priceCommand = commandBuilder
                    .WithDocument(document)
                    .WithChargeOperation(operation)
                    .Build();
                var chargePriceReceivedEvent = new ChargePriceCommandReceivedEvent(
                    Instant.FromDateTimeUtc(DateTime.UtcNow), priceCommand);
                return chargePriceReceivedEvent;
            }
        }
    }
}
