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
using System.Linq.Expressions;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Energinet.DataHub.Core.FunctionApp.TestCommon;
using FluentAssertions;
using GreenEnergyHub.Charges.Domain.Charges;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeInformationCommandReceivedEvents;
using GreenEnergyHub.Charges.Domain.Dtos.SharedDtos;
using GreenEnergyHub.Charges.FunctionHost.Charges;
using GreenEnergyHub.Charges.FunctionHost.Charges.MessageHub;
using GreenEnergyHub.Charges.Infrastructure.Core.MessageMetaData;
using GreenEnergyHub.Charges.IntegrationTest.Core.Fixtures.FunctionApp;
using GreenEnergyHub.Charges.IntegrationTest.Core.TestCommon;
using GreenEnergyHub.Charges.IntegrationTest.Core.TestHelpers;
using GreenEnergyHub.Charges.IntegrationTests.Fixtures;
using GreenEnergyHub.Charges.TestCore;
using GreenEnergyHub.Charges.TestCore.Attributes;
using GreenEnergyHub.Charges.Tests.Builders.Command;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using NodaTime.Text;
using Xunit;
using Xunit.Abstractions;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.IntegrationTests.IntegrationTests.EndpointTests
{
    [IntegrationTest]
    public class ChargeInformationCommandReceiverEndpointTests
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
            public async Task RunAsync_WhenChargeIsNotChanged_ConfirmationEventIsRaised(
                ChargeInformationCommandBuilder commandBuilder,
                DocumentDtoBuilder documentDtoBuilder,
                ChargeInformationOperationDtoBuilder operationDtoBuilder)
            {
                // Arrange
                var ownerId = SeededData.GridAreaLink.Provider8100000000030.Id;
                const string ownerGln = SeededData.GridAreaLink.Provider8100000000030.MarketParticipantId;
                const string chargeId = "TestTariff";
                const ChargeType chargeType = ChargeType.Tariff;

                await using var expectedChargeContext = Fixture.ChargesDatabaseManager.CreateDbContext();

                var expectedCharge = await expectedChargeContext.Charges.FirstAsync(
                    GetChargePredicate(chargeId, ownerId, chargeType));

                var chargeCommandReceivedEvent = CreateChargeCommandReceivedEvent(
                    commandBuilder, documentDtoBuilder, operationDtoBuilder, chargeId, ownerGln, chargeType);
                var correlationId = CorrelationIdGenerator.Create();
                var message = ServiceBusMessageGenerator.CreateServiceBusMessage(
                    chargeCommandReceivedEvent,
                    correlationId);

                // Act
                await MockTelemetryClient.WrappedOperationWithTelemetryDependencyInformationAsync(
                    () => Fixture.ChargesDomainEventTopic.SenderClient.SendMessageAsync(message), correlationId);

                await FunctionAsserts.AssertHasExecutedAsync(
                    Fixture.HostManager, nameof(ChargeInformationCommandReceiverEndpoint));

                await FunctionAsserts.AssertHasExecutedAsync(
                    Fixture.HostManager, nameof(ChargeConfirmationDataAvailableNotifierEndpoint));

                // Assert
                await using var actualChargeContext = Fixture.ChargesDatabaseManager.CreateDbContext();
                var actualCharge = await actualChargeContext.Charges.FirstAsync(
                    GetChargePredicate(chargeId, ownerId, chargeType));

                actualCharge.Should().BeEquivalentTo(expectedCharge, options => options.Excluding(x => x.Periods));
                actualCharge.Periods.Should().BeEquivalentTo(expectedCharge.Periods, o => o.Excluding(s => s.Id));
            }

            private static Expression<Func<Charge, bool>> GetChargePredicate(
                string chargeId, Guid ownerId, ChargeType chargeType) =>
                x =>
                    x.SenderProvidedChargeId == chargeId &&
                    x.OwnerId == ownerId &&
                    x.Type == chargeType;

            private static ChargeInformationCommandReceivedEvent CreateChargeCommandReceivedEvent(
                ChargeInformationCommandBuilder commandBuilder,
                DocumentDtoBuilder documentDtoBuilder,
                ChargeInformationOperationDtoBuilder operationDtoBuilder,
                string chargeId,
                string ownerId,
                ChargeType chargeType)
            {
                var document = documentDtoBuilder
                    .WithBusinessReasonCode(BusinessReasonCode.UpdateChargeInformation)
                    .WithDocumentType(DocumentType.RequestChangeOfPriceList)
                    .Build();

                var startDateTime = InstantPattern.ExtendedIso.Parse("2021-12-31T23:00:00Z").Value;

                var operation = operationDtoBuilder
                    .WithOwner(ownerId)
                    .WithChargeId(chargeId)
                    .WithChargeType(chargeType)
                    .WithTransparentInvoicing(TransparentInvoicing.NonTransparent)
                    .WithStartDateTime(startDateTime)
                    .WithEndDateTime(null)
                    .WithTaxIndicator(TaxIndicator.NoTax)
                    .WithVatClassification(VatClassification.NoVat)
                    .WithChargeName("Grid Access Provider test tariff")
                    .WithDescription("Description...")
                    .WithPriceResolution(Resolution.P1D)
                    .Build();
                var chargeCommand = commandBuilder
                    .WithDocumentDto(document)
                    .WithChargeOperation(operation)
                    .Build();
                var chargeInformationReceivedEvent = new ChargeInformationCommandReceivedEvent(
                    Instant.FromDateTimeUtc(DateTime.UtcNow), chargeCommand);

                return chargeInformationReceivedEvent;
            }
        }
    }
}
