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
using GreenEnergyHub.Charges.Domain.ChargeLinks;
using GreenEnergyHub.Charges.Domain.Charges;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeLinksCommands;
using GreenEnergyHub.Charges.Domain.MeteringPoints;
using GreenEnergyHub.TestHelpers;
using Moq;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Domain.ChargeLinks
{
    [UnitTest]
    public class ChargeLinkFactoryTests
    {
        [Theory]
        [InlineAutoDomainData]
        public async Task CreateAsync_WhenCalled_ShouldCreateChargeLinkCorrectly(
            ChargeLinkDto chargeLinkDto,
            Charge expectedCharge,
            MeteringPoint expectedMeteringPoint,
            [Frozen] Mock<IChargeIdentifierFactory> chargeIdentifierFactory,
            [Frozen] Mock<IChargeRepository> chargeRepository,
            [Frozen] Mock<IMeteringPointRepository> meteringPointRepository,
            ChargeLinkFactory sut)
        {
            // Arrange
            chargeIdentifierFactory
                .Setup(x => x.CreateAsync(It.IsAny<string>(), It.IsAny<ChargeType>(), It.IsAny<string>()))
                .ReturnsAsync(It.IsAny<ChargeIdentifier>());

            chargeRepository
                .Setup(x => x.SingleAsync(It.IsAny<ChargeIdentifier>()))
                .ReturnsAsync(expectedCharge);

            meteringPointRepository
                .Setup(x => x.GetMeteringPointAsync(It.IsAny<string>()))
                .ReturnsAsync(expectedMeteringPoint);

            // Act
            var actual = await sut.CreateAsync(chargeLinkDto).ConfigureAwait(false);

            // Assert
            actual.ChargeId.Should().Be(expectedCharge.Id);
            actual.MeteringPointId.Should().Be(expectedMeteringPoint.Id);
            actual.Factor.Should().Be(chargeLinkDto.Factor);
            actual.StartDateTime.Should().Be(chargeLinkDto.StartDateTime);
            actual.EndDateTime.Should().Be(chargeLinkDto.EndDateTime!.Value);
        }

        [Theory]
        [InlineAutoDomainData]
        public async Task CreateAsync_WhenCalledWithNull_ShouldThrow(ChargeLinkFactory sut)
        {
            await Assert
                .ThrowsAsync<ArgumentNullException>(async () => await sut.CreateAsync(null!)
                    .ConfigureAwait(false))
                .ConfigureAwait(false);
        }
    }
}
