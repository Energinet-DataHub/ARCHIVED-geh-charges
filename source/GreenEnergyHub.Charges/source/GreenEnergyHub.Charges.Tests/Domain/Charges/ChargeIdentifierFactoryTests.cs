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
using GreenEnergyHub.Charges.Domain.Charges;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommands;
using GreenEnergyHub.Charges.Domain.MarketParticipants;
using GreenEnergyHub.Charges.Tests.Builders.Testables;
using GreenEnergyHub.TestHelpers;
using GreenEnergyHub.TestHelpers.FluentAssertionsExtensions;
using Moq;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Domain.Charges
{
    [UnitTest]
    public class ChargeIdentifierFactoryTests
    {
        [Theory]
        [InlineAutoDomainData]
        public async Task CreateAsync_ChargeIdentifier_HasNoNullsOrEmptyCollections(
            TestMarketParticipant owner,
            [Frozen] Mock<IMarketParticipantRepository> marketParticipantRepository,
            ChargeOperationDto chargeOperationDto,
            ChargeIdentifierFactory sut)
        {
            // Arrange
            marketParticipantRepository
                .Setup(repo => repo.SingleAsync(chargeOperationDto.ChargeOwner))
                .ReturnsAsync(owner);

            // Act
            var actual = await sut.CreateAsync(
                chargeOperationDto.ChargeId, chargeOperationDto.Type, chargeOperationDto.ChargeOwner);

            // Assert
            actual.Should().NotContainNullEnumerable();
        }
    }
}
