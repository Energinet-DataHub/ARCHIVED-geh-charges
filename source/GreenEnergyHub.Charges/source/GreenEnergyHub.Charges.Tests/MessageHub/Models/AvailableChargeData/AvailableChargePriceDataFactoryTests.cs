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

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture.Xunit2;
using FluentAssertions;
using GreenEnergyHub.Charges.Application.Messaging;
using GreenEnergyHub.Charges.Core.DateTime;
using GreenEnergyHub.Charges.Domain.Charges;
using GreenEnergyHub.Charges.Domain.Dtos.ChargePriceCommands;
using GreenEnergyHub.Charges.Domain.Dtos.SharedDtos;
using GreenEnergyHub.Charges.Domain.MarketParticipants;
using GreenEnergyHub.Charges.MessageHub.Models.AvailableChargeData;
using GreenEnergyHub.Charges.Tests.Builders.Command;
using GreenEnergyHub.Charges.Tests.Builders.Testables;
using GreenEnergyHub.TestHelpers;
using GreenEnergyHub.TestHelpers.FluentAssertionsExtensions;
using Moq;
using NodaTime;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.MessageHub.Models.AvailableChargeData
{
    [UnitTest]
    public class AvailableChargePriceDataFactoryTests
    {
        [Theory]
        [InlineAutoDomainData]
        public async Task CreateAsync_WhenTaxCharge_CreatesAvailableDataPerActiveGrid(
            [Frozen] Mock<IMarketParticipantRepository> marketParticipantRepository,
            [Frozen] Mock<IChargeRepository> chargeRepository,
            [Frozen] Mock<IChargeIdentifierFactory> chargeIdentifierFactory,
            [Frozen] Mock<IMessageMetaDataContext> messageMetaDataContext,
            Instant now,
            TestMeteringPointAdministrator meteringPointAdministrator,
            List<TestGridAccessProvider> gridAccessProvider,
            ChargePriceOperationsConfirmedEventBuilder chargePriceOperationsConfirmedEventBuilder,
            AvailableChargePriceDataFactory sut)
        {
            // Arrange
            var operations = new List<ChargePriceOperationDto>()
            {
                new ChargePriceOperationDtoBuilder()
                    .WithPoint(1)
                    .Build(),
            };
            var confirmedEvent = chargePriceOperationsConfirmedEventBuilder.WithOperations(operations).Build();

            SetupChargeIdentifierFactoryMock(chargeIdentifierFactory);
            SetupChargeRepository(chargeRepository, TaxIndicator.Tax);
            marketParticipantRepository
                .Setup(r => r.GetGridAccessProvidersAsync())
                .ReturnsAsync(gridAccessProvider.Cast<MarketParticipant>().ToList);

            marketParticipantRepository
                .Setup(r => r.GetMeteringPointAdministratorAsync())
                .ReturnsAsync(meteringPointAdministrator);

            messageMetaDataContext.Setup(m => m.RequestDataTime).Returns(now);

            // Act
            var actual = await sut.CreateAsync(confirmedEvent);

            // Assert
            var operation = confirmedEvent.Operations.First();
            actual.Should().HaveSameCount(gridAccessProvider);
            for (var i = 0; i < actual.Count; i++)
            {
                actual[i].Should().NotContainNullEnumerable();
                actual[i].RecipientId.Should().Be(gridAccessProvider[i].MarketParticipantId);
                actual[i].RecipientRole.Should().Be(gridAccessProvider[i].BusinessProcessRole);
                actual[i].BusinessReasonCode.Should().Be(confirmedEvent.Document.BusinessReasonCode);
                actual[i].RequestDateTime.Should().Be(now);
                actual[i].ChargeId.Should().Be(operation.SenderProvidedChargeId);
                actual[i].ChargeOwner.Should().Be(operation.ChargeOwner);
                actual[i].ChargeType.Should().Be(operation.ChargeType);
                actual[i].StartDateTime.Should().Be(operation.StartDateTime);
                actual[i].Resolution.Should().Be(operation.Resolution);
                actual[i].Points.Should().BeEquivalentTo(
                    operation.Points,
                    options => options.ExcludingMissingMembers());
            }
        }

        [Theory]
        [InlineAutoDomainData(TaxIndicator.NoTax, 0)]
        [InlineAutoDomainData(TaxIndicator.Tax, 1)]
        public async Task CreateAsync_WhenNotTaxCharge_ReturnsEmptyList(
            TaxIndicator taxIndicator,
            int availableChargeDataCount,
            [Frozen] Mock<IChargeRepository> chargeRepository,
            [Frozen] Mock<IChargeIdentifierFactory> chargeIdentifierFactory,
            [Frozen] Mock<IMarketParticipantRepository> marketParticipantRepository,
            ChargePriceOperationsConfirmedEventBuilder chargePriceOperationsConfirmedEventBuilder,
            AvailableChargePriceDataFactory sut)
        {
            // Arrange
            var marketParticipants = new List<MarketParticipant>()
            {
                new MarketParticipantBuilder()
                    .WithRole(MarketParticipantRole.GridAccessProvider)
                    .Build(),
            };
            marketParticipantRepository
                .Setup(m => m.GetGridAccessProvidersAsync())
                .ReturnsAsync(marketParticipants);
            marketParticipantRepository
                .Setup(m => m.GetMeteringPointAdministratorAsync())
                .ReturnsAsync(new MarketParticipantBuilder().Build());
            SetupChargeIdentifierFactoryMock(chargeIdentifierFactory);
            SetupChargeRepository(chargeRepository, taxIndicator);
            var operations = new List<ChargePriceOperationDto>()
            {
                new ChargePriceOperationDtoBuilder()
                    .WithPoint(1)
                    .Build(),
            };
            var confirmedEvent = chargePriceOperationsConfirmedEventBuilder.WithOperations(operations).Build();

            // Act
            var actual = await sut.CreateAsync(confirmedEvent);

            // Assert
            actual.Count.Should().Be(availableChargeDataCount);
        }

        [Theory]
        [InlineAutoDomainData]
        public async Task CreateAsync_WhenSeveralOperationsInChargePriceCommand_ReturnOrderedListOfOperations(
            [Frozen] Mock<IMarketParticipantRepository> marketParticipantRepository,
            [Frozen] Mock<IChargeRepository> chargeRepository,
            [Frozen] Mock<IChargeIdentifierFactory> chargeIdentifierFactory,
            ChargePriceOperationsConfirmedEventBuilder chargePriceOperationsConfirmedEventBuilder,
            List<TestGridAccessProvider> gridAccessProvider,
            TestMeteringPointAdministrator meteringPointAdministrator,
            AvailableChargePriceDataFactory sut)
        {
            // Arrange
            marketParticipantRepository
                .Setup(r => r.GetGridAccessProvidersAsync())
                .ReturnsAsync(gridAccessProvider.Cast<MarketParticipant>().ToList);
            marketParticipantRepository
                .Setup(r => r.GetMeteringPointAdministratorAsync())
                .ReturnsAsync(meteringPointAdministrator);
            SetupChargeIdentifierFactoryMock(chargeIdentifierFactory);
            SetupChargeRepository(chargeRepository, TaxIndicator.Tax);
            var confirmedEvent = chargePriceOperationsConfirmedEventBuilder.WithOperations(
                    new List<ChargePriceOperationDto>
                    {
                        new ChargePriceOperationDtoBuilder().Build(),
                        new ChargePriceOperationDtoBuilder().Build(),
                        new ChargePriceOperationDtoBuilder().Build(),
                    })
                .Build();

            // Act
            var actual = await sut.CreateAsync(confirmedEvent);

            // Assert
            actual.Count.Should().Be(gridAccessProvider.Count * 3);
            foreach (var gap in gridAccessProvider)
            {
                var availableChargesForProvider = actual
                    .Where(x => x.RecipientId == gap.MarketParticipantId).ToList();
                var operationOrder = -1;
                for (var i = 0; i < availableChargesForProvider.Count; i++)
                {
                    availableChargesForProvider[i].OperationOrder.Should().BeGreaterThan(operationOrder);
                    operationOrder = actual[i].OperationOrder;
                }
            }
        }

        private static void SetupChargeIdentifierFactoryMock(Mock<IChargeIdentifierFactory> chargeIdentifierFactory)
        {
            chargeIdentifierFactory
                .Setup(x => x.CreateAsync(It.IsAny<string>(), It.IsAny<ChargeType>(), It.IsAny<string>()))
                .ReturnsAsync(It.IsAny<ChargeIdentifier>());
        }

        private static void SetupChargeRepository(Mock<IChargeRepository> chargeRepository, TaxIndicator taxIndicator)
        {
            var charge = new ChargeBuilder()
                .WithTaxIndicator(taxIndicator)
                .Build();
            chargeRepository
                .Setup(r => r.SingleAsync(It.IsAny<ChargeIdentifier>()))
                .ReturnsAsync(charge);
        }
    }
}
