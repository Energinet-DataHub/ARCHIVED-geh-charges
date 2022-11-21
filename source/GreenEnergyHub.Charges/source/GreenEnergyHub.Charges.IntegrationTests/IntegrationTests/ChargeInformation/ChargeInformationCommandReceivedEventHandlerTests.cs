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
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Energinet.DataHub.Core.JsonSerialization;
using FluentAssertions;
using GreenEnergyHub.Charges.Application.Charges.Factories;
using GreenEnergyHub.Charges.Application.Charges.Handlers.ChargeInformation;
using GreenEnergyHub.Charges.Application.Common.Services;
using GreenEnergyHub.Charges.Domain.Charges;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeInformationCommandReceivedEvents;
using GreenEnergyHub.Charges.Domain.Dtos.Events;
using GreenEnergyHub.Charges.Domain.Dtos.Validation;
using GreenEnergyHub.Charges.Infrastructure.Outbox;
using GreenEnergyHub.Charges.Infrastructure.Persistence;
using GreenEnergyHub.Charges.IntegrationTest.Core.Fixtures;
using Microsoft.Extensions.Logging;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.IntegrationTests.IntegrationTests.ChargeInformation
{
    /// <summary>
    /// Tests ChargeInformation flow with managed dependencies
    /// </summary>
    [IntegrationTest]
    public class ChargeInformationCommandReceivedEventHandlerTests : IClassFixture<ChargesManagedDependenciesTestFixture>, IAsyncLifetime
    {
        private readonly ChargesManagedDependenciesTestFixture _fixture;
        private readonly IJsonSerializer _jsonSerializer;

        public ChargeInformationCommandReceivedEventHandlerTests(ChargesManagedDependenciesTestFixture fixture)
        {
            _fixture = fixture;
            _jsonSerializer = _fixture.GetService<IJsonSerializer>();
        }

        public Task InitializeAsync()
        {
            _fixture.CorrelationContext.SetId(Guid.NewGuid().ToString());
            return Task.CompletedTask;
        }

        public Task DisposeAsync()
        {
            return Task.CompletedTask;
        }

        [Fact]
        public async Task HandleAsync_WhenValidChargeInformationCommandReceivedEvent_ThenChargeIsPersisted()
        {
            // Arrange
            await using var chargesDatabaseReadContext = _fixture.ChargesDatabaseManager.CreateDbContext();
            var sut = BuildChargeInformationCommandReceivedEventHandler();
            var receivedEvent =
                await GetTestDataFromFile("ChargeInformationTestTariff.json");

            // Act
            await sut.HandleAsync(receivedEvent);
            await _fixture.ChargesUnitOfWork.SaveChangesAsync();

            // Assert
            var (charge, outboxMessages) = GetResultFromDatabase(receivedEvent, chargesDatabaseReadContext);

            charge.Should().NotBeNull();
            outboxMessages.Should().Contain(x => x.Type == ChargeInformationOperationsAcceptedEventFullName);
        }

        [Fact]
        public async Task HandleAsync_BundleWithTwoOperationsForSameTariffWhere2ndIsInvalid_Confirms1st_Rejects2nd_And_NotifiesAbout1st()
        {
            // Arrange
            await using var chargesDatabaseReadContext = _fixture.ChargesDatabaseManager.CreateDbContext();
            var sut = BuildChargeInformationCommandReceivedEventHandler();
            var receivedEvent = await GetTestDataFromFile(
                "ChargeInformationBundleWithTwoOperationsForSameTariffSecondOpViolatingVr903.json");

            // Act
            await sut.HandleAsync(receivedEvent);
            await _fixture.ChargesUnitOfWork.SaveChangesAsync();

            // Assert
            var (charge, outboxMessages) = GetResultFromDatabase(receivedEvent, chargesDatabaseReadContext);

            charge.Should().NotBeNull();
            charge?.TaxIndicator.Should().BeFalse();
            outboxMessages.Should().Contain(x => x.Type == ChargeInformationOperationsAcceptedEventFullName);
            outboxMessages.Single(x => x.Type == ChargeInformationOperationsRejectedEventFullName).Data.Should()
                .Contain($@"ValidationRuleIdentifier"":{(int)ValidationRuleIdentifier.ChangingTariffTaxValueNotAllowed}");
        }

        [Fact]
        public async Task HandleAsync_ChargeInformation_WithInvalidDocument_Rejects()
        {
            // Arrange
            await using var chargesDatabaseReadContext = _fixture.ChargesDatabaseManager.CreateDbContext();
            var sut = BuildChargeInformationCommandReceivedEventHandler();
            var receivedEvent = await GetTestDataFromFile(
                "ChargeInformationWithUnsupportedBusinessReasonCode.json");

            // Act
            await sut.HandleAsync(receivedEvent);
            await _fixture.ChargesUnitOfWork.SaveChangesAsync();

            // Assert
            var (charge, outboxMessages) = GetResultFromDatabase(receivedEvent, chargesDatabaseReadContext);

            charge.Should().BeNull();
            outboxMessages.Single(x => x.Type == ChargeInformationOperationsRejectedEventFullName).Data.Should()
                .Contain($@"ValidationRuleIdentifier"":{(int)ValidationRuleIdentifier.BusinessReasonCodeMustBeUpdateChargeInformationOrChargePrices}");
        }

        private (Charge? Charge, IList<OutboxMessage> OutboxMessages) GetResultFromDatabase(
            ChargeInformationCommandReceivedEvent receivedEvent,
            IChargesDatabaseContext chargesDatabaseReadContext)
        {
            var senderProvidedChargeId = receivedEvent.Command.Operations.First().SenderProvidedChargeId;
            var charge = chargesDatabaseReadContext.Charges
                .FirstOrDefault(x => x.SenderProvidedChargeId == senderProvidedChargeId);

            var outboxMessages = chargesDatabaseReadContext.OutboxMessages
                .Where(x => x.CorrelationId == _fixture.CorrelationContext.Id).ToList();

            return (charge, outboxMessages);
        }

        private async Task<ChargeInformationCommandReceivedEvent> GetTestDataFromFile(string filename)
        {
            var jsonString = await File.ReadAllTextAsync($"IntegrationTests\\ChargeInformation\\TestData\\{filename}");
            return _jsonSerializer.Deserialize<ChargeInformationCommandReceivedEvent>(jsonString);
        }

        private ChargeInformationCommandReceivedEventHandler BuildChargeInformationCommandReceivedEventHandler()
        {
            return new ChargeInformationCommandReceivedEventHandler(
                _fixture.GetService<ILoggerFactory>(),
                _fixture.GetService<IChargeInformationOperationsHandler>(),
                _fixture.GetService<IDocumentValidator>(),
                _fixture.GetService<IDomainEventPublisher>(),
                _fixture.GetService<IChargeInformationOperationsRejectedEventFactory>());
        }

        private static string? ChargeInformationOperationsAcceptedEventFullName =>
            typeof(ChargeInformationOperationsAcceptedEvent).FullName;

        private static string? ChargeInformationOperationsRejectedEventFullName =>
            typeof(ChargeInformationOperationsRejectedEvent).FullName;
    }
}
