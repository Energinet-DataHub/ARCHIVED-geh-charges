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
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommands;
using GreenEnergyHub.Charges.Domain.MarketParticipants;
using GreenEnergyHub.Charges.Tests.Builders.Testables;
using GreenEnergyHub.TestHelpers;
using GreenEnergyHub.TestHelpers.FluentAssertionsExtensions;
using Moq;
using NodaTime;
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
            ChargeCommand chargeCommand,
            [Frozen] Mock<IMarketParticipantRepository> marketParticipantRepository,
            /*[Frozen] Mock<IChargePeriodFactory> chargePeriodFactory,
            Mock<ChargePeriod> chargePeriod,*/
            ChargeFactory sut)
        {
            marketParticipantRepository
                .Setup(repo => repo.GetOrNullAsync(chargeCommand.ChargeOperation.ChargeOwner))
                .ReturnsAsync(owner);

            /*chargePeriodFactory
                .Setup(f => f.CreateFromChargeOperationDto(It.IsAny<Instant>(), It.IsAny<ChargeOperationDto>()))
                .Returns(chargePeriod.Object);*/

            var actual = await sut.CreateFromCommandAsync(chargeCommand);

            actual.Should().NotContainNullsOrEmptyEnumerables();
        }

        [Theory]
        [InlineAutoDomainData]
        public async Task CreateFromCommandAsync_WhenOwnerIsNull_ThrowsException(
            ChargeCommand chargeCommand,
            [Frozen] Mock<IMarketParticipantRepository> marketParticipantRepository,
            /*[Frozen] Mock<IChargePeriodFactory> chargePeriodFactory,
            Mock<ChargePeriod> chargePeriod,*/
            ChargeFactory sut)
        {
            marketParticipantRepository
                .Setup(repo => repo.GetOrNullAsync(It.IsAny<string>()))
                .ReturnsAsync((MarketParticipant?)null);

            /*chargePeriodFactory
                .Setup(f => f.CreateFromChargeOperationDto(It.IsAny<Instant>(), It.IsAny<ChargeOperationDto>()))
                .Returns(chargePeriod.Object);*/

            await Assert.ThrowsAsync<InvalidOperationException>(() => sut.CreateFromCommandAsync(chargeCommand));
        }
    }
}
