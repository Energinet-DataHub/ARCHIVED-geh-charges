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
using AutoFixture.Xunit2;
using FluentAssertions;
using GreenEnergyHub.Charges.Domain.Charges;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeInformationCommands;
using GreenEnergyHub.Charges.Domain.MarketParticipants;
using GreenEnergyHub.Charges.TestCore.Builders.Command;
using GreenEnergyHub.Charges.TestCore.Builders.Testables;
using GreenEnergyHub.TestHelpers;
using GreenEnergyHub.TestHelpers.FluentAssertionsExtensions;
using Moq;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Domain.Charges
{
    [UnitTest]
    public class ChargeFactoryTests
    {
        [Theory]
        [InlineAutoDomainData]
        public async Task CreateFromCommandAsync_Charge_HasNoNullsOrEmptyCollections(
            TestMarketParticipant owner,
            [Frozen] Mock<IMarketParticipantRepository> marketParticipantRepository,
            ChargeInformationOperationDtoBuilder chargeInformationOperationDtoBuilder,
            ChargeFactory sut)
        {
            // Arrange
            var chargeOperationDto = chargeInformationOperationDtoBuilder.Build();
            marketParticipantRepository
                .Setup(repo => repo.SingleOrNullAsync(
                    chargeOperationDto.ChargeOwner))
                .ReturnsAsync(owner);

            // Act
            var actual = await sut.CreateFromChargeOperationDtoAsync(chargeOperationDto);

            // Assert
            actual.Should().NotContainNullEnumerable();
        }

        [Theory]
        [InlineAutoDomainData]
        public async Task CreateFromCommandAsync_WhenOwnerIsNull_ThrowsException(
            [Frozen] Mock<IMarketParticipantRepository> marketParticipantRepository,
            [Frozen] Mock<IChargePeriodFactory> chargePeriodFactory,
            ChargeInformationOperationDto chargeInformationOperationDto,
            ChargeFactory sut)
        {
            // Arrange
            var chargePeriod = new ChargePeriodBuilder().Build();
            marketParticipantRepository
                .Setup(repo => repo.SingleOrNullAsync(
                    It.IsAny<string>()))
                .ReturnsAsync((MarketParticipant?)null);

            chargePeriodFactory
                .Setup(f => f.CreateFromChargeOperationDto(It.IsAny<ChargeInformationOperationDto>()))
                .Returns(chargePeriod);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                sut.CreateFromChargeOperationDtoAsync(chargeInformationOperationDto));
        }
    }
}
