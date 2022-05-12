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

using System.Threading.Tasks;
using AutoFixture.Xunit2;
using FluentAssertions;
using GreenEnergyHub.Charges.Domain.ChargeInformations;
using GreenEnergyHub.Charges.Domain.ChargePrices;
using GreenEnergyHub.Charges.TestCore;
using GreenEnergyHub.Charges.Tests.Builders.Command;
using GreenEnergyHub.TestHelpers;
using GreenEnergyHub.TestHelpers.FluentAssertionsExtensions;
using Moq;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Domain.ChargePrices
{
    [UnitTest]
    public class ChargePriceFactoryTests
    {
        [Theory]
        [InlineAutoDomainData]
        public async Task CreateFromOperationAsync_Charge_HasNoNullsOrEmptyCollections(
            [Frozen] Mock<IChargeInformationRepository> chargeInformationRepository,
            ChargeInformationBuilder chargeInformationBuilder,
            ChargeInformationIdentifier chargeInformationIdentifier,
            ChargePriceFactory sut)
        {
            // Arrange
            var chargeInformation = chargeInformationBuilder.Build();
            chargeInformationRepository
                .Setup(repo => repo.GetOrNullAsync(It.IsAny<ChargeInformationIdentifier>()))
                .ReturnsAsync(chargeInformation);
            var point = new Point(1, 1.00m, InstantHelper.GetTomorrowAtMidnightUtc());

            // Act
            var actual = await sut.CreateChargePriceFromPointAsync(chargeInformationIdentifier, point);

            // Assert
            actual.Should().NotBeNull();
        }
    }
}
