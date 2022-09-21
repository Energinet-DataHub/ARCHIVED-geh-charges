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
using GreenEnergyHub.Charges.Domain.Dtos.ChargeInformationCommands;
using GreenEnergyHub.Charges.Domain.Dtos.SharedDtos;
using GreenEnergyHub.Charges.Domain.MarketParticipants;
using GreenEnergyHub.Charges.MessageHub.Models.AvailableChargeData;
using GreenEnergyHub.Charges.TestCore.Builders.Command;
using GreenEnergyHub.Charges.TestCore.Builders.Testables;
using GreenEnergyHub.TestHelpers;
using GreenEnergyHub.TestHelpers.FluentAssertionsExtensions;
using Moq;
using NodaTime;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.MessageHub.Models.AvailableChargeData
{
    [UnitTest]
    public class AvailableChargeDataFactoryTests
    {
        [Theory]
        [InlineAutoDomainData]
        public async Task CreateAsync_WhenTaxCharge_CreatesAvailableDataPerActiveGrid(
            [Frozen] Mock<IMarketParticipantRepository> marketParticipantRepository,
            [Frozen] Mock<IMessageMetaDataContext> messageMetaDataContext,
            Instant now,
            TestMeteringPointAdministrator meteringPointAdministrator,
            List<TestGridAccessProvider> gridAccessProvider,
            ChargeInformationCommandBuilder chargeInformationCommandBuilder,
            ChargeCommandAcceptedEventBuilder chargeCommandAcceptedEventBuilder,
            AvailableChargeDataFactory sut)
        {
            // Arrange
            var chargeOperationDto = new ChargeInformationOperationDtoBuilder()
                .WithTaxIndicator(TaxIndicator.Tax)
                .WithTransparentInvoicing(TransparentInvoicing.Transparent)
                .Build();
            var chargeCommand = chargeInformationCommandBuilder.WithChargeOperation(chargeOperationDto).Build();
            var acceptedEvent = chargeCommandAcceptedEventBuilder.WithChargeCommand(chargeCommand).Build();

            marketParticipantRepository
                .Setup(r => r.GetActiveGridAccessProvidersAsync())
                .ReturnsAsync(gridAccessProvider.Cast<MarketParticipant>().ToList);

            marketParticipantRepository
                .Setup(r => r.GetMeteringPointAdministratorAsync())
                .ReturnsAsync(meteringPointAdministrator);

            messageMetaDataContext.Setup(m => m.RequestDataTime).Returns(now);

            // Act
            var actual = await sut.CreateAsync(acceptedEvent);

            // Assert
            var operation = acceptedEvent.Command.Operations.First();
            actual.Should().HaveSameCount(gridAccessProvider);
            for (var i = 0; i < actual.Count; i++)
            {
                actual[i].Should().NotContainNullEnumerable();
                actual[i].RecipientId.Should().Be(gridAccessProvider[i].MarketParticipantId);
                actual[i].RecipientRole.Should().Be(gridAccessProvider[i].BusinessProcessRole);
                actual[i].BusinessReasonCode.Should().Be(acceptedEvent.Command.Document.BusinessReasonCode);
                actual[i].RequestDateTime.Should().Be(now);
                actual[i].ChargeId.Should().Be(operation.SenderProvidedChargeId);
                actual[i].ChargeOwner.Should().Be(operation.ChargeOwner);
                actual[i].ChargeType.Should().Be(operation.ChargeType);
                actual[i].ChargeName.Should().Be(operation.ChargeName);
                actual[i].ChargeDescription.Should().Be(operation.ChargeDescription);
                actual[i].StartDateTime.Should().Be(operation.StartDateTime);
                actual[i].EndDateTime.Should().Be(operation.EndDateTime.TimeOrEndDefault());
                actual[i].VatClassification.Should().Be(operation.VatClassification);
                actual[i].TaxIndicator.Should().Be(true);
                actual[i].TransparentInvoicing.Should().Be(true);
                actual[i].Resolution.Should().Be(operation.Resolution);
            }
        }

        [Theory]
        [InlineAutoDomainData(TaxIndicator.NoTax, 1)]
        [InlineAutoDomainData(TaxIndicator.Tax, 2)]
        public async Task CreateAsync_WhenNotTaxCharge_ReturnsOnlyEnergySuppliers(
            TaxIndicator taxIndicator,
            int availableChargeDataCount,
            [Frozen] Mock<IMarketParticipantRepository> marketParticipantRepository,
            ChargeInformationCommandBuilder chargeInformationCommandBuilder,
            ChargeCommandAcceptedEventBuilder chargeCommandAcceptedEventBuilder,
            AvailableChargeDataFactory sut)
        {
            // Arrange
            var gridAccessProviders = new List<MarketParticipant>()
            {
                new MarketParticipantBuilder()
                    .WithRole(MarketParticipantRole.GridAccessProvider)
                    .Build(),
            };
            var energySuppliers = new List<MarketParticipant>()
            {
                new MarketParticipantBuilder()
                    .WithRole(MarketParticipantRole.EnergySupplier)
                    .Build(),
            };
            marketParticipantRepository
                .Setup(m => m.GetActiveGridAccessProvidersAsync())
                .ReturnsAsync(gridAccessProviders);
            marketParticipantRepository
                .Setup(r => r.GetActiveEnergySuppliersAsync())
                .ReturnsAsync(energySuppliers);
            marketParticipantRepository
                .Setup(m => m.GetMeteringPointAdministratorAsync())
                .ReturnsAsync(new MarketParticipantBuilder().Build());
            var chargeOperationDto = new ChargeInformationOperationDtoBuilder()
                .WithTaxIndicator(taxIndicator)
                .WithTransparentInvoicing(TransparentInvoicing.Transparent)
                .Build();
            var chargeCommand = chargeInformationCommandBuilder.WithChargeOperation(chargeOperationDto).Build();
            var acceptedEvent = chargeCommandAcceptedEventBuilder.WithChargeCommand(chargeCommand).Build();

            // Act
            var actual = await sut.CreateAsync(acceptedEvent);

            // Assert
            actual.Count.Should().Be(availableChargeDataCount);
        }

        [Theory]
        [InlineAutoDomainData]
        public async Task CreateAsync_WhenSeveralOperationsInChargeCommand_ReturnOrderedListOfOperations(
            [Frozen] Mock<IMarketParticipantRepository> marketParticipantRepository,
            ChargeInformationCommandBuilder chargeInformationCommandBuilder,
            ChargeCommandAcceptedEventBuilder chargeCommandAcceptedEventBuilder,
            List<TestGridAccessProvider> gridAccessProvider,
            TestMeteringPointAdministrator meteringPointAdministrator,
            AvailableChargeDataFactory sut)
        {
            // Arrange
            marketParticipantRepository
                .Setup(r => r.GetActiveGridAccessProvidersAsync())
                .ReturnsAsync(gridAccessProvider.Cast<MarketParticipant>().ToList);
            marketParticipantRepository
                .Setup(r => r.GetActiveEnergySuppliersAsync())
                .ReturnsAsync(() => new List<MarketParticipant>());
            marketParticipantRepository
                .Setup(r => r.GetMeteringPointAdministratorAsync())
                .ReturnsAsync(meteringPointAdministrator);
            var chargeCommand = chargeInformationCommandBuilder
                .WithChargeOperations(
                    new List<ChargeInformationOperationDto>
                    {
                        new ChargeInformationOperationDtoBuilder().WithTaxIndicator(TaxIndicator.Tax)
                            .WithTransparentInvoicing(TransparentInvoicing.Transparent).Build(),
                        new ChargeInformationOperationDtoBuilder().WithTaxIndicator(TaxIndicator.Tax)
                            .WithTransparentInvoicing(TransparentInvoicing.Transparent).Build(),
                        new ChargeInformationOperationDtoBuilder().WithTaxIndicator(TaxIndicator.Tax)
                            .WithTransparentInvoicing(TransparentInvoicing.Transparent).Build(),
                    })
                .Build();
            var acceptedEvent = chargeCommandAcceptedEventBuilder.WithChargeCommand(chargeCommand).Build();

            // Act
            var actual = await sut.CreateAsync(acceptedEvent);

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
    }
}
