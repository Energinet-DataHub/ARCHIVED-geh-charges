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
using Energinet.DataHub.Core.TestCommon.AutoFixture.Attributes;
using FluentAssertions;
using GreenEnergyHub.Charges.Application.MarketParticipants.Handlers;
using GreenEnergyHub.Charges.Domain.Dtos.Events;
using GreenEnergyHub.Charges.Domain.Dtos.SharedDtos;
using GreenEnergyHub.Charges.Domain.MarketParticipants;
using GreenEnergyHub.Charges.TestCore.Builders.Testables;
using GreenEnergyHub.TestHelpers;
using Moq;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Application.MarketParticipants.Handlers
{
    [UnitTest]
    public class MarketParticipantB2CActorIdChangedCommandHandlerTests
    {
        [Theory]
        [InlineAutoMoqData("664359b9-f6cc-45d4-9c93-ec35248e5f95")]
        [InlineAutoMoqData(null!)]
        public async Task HandleAsync_ValidEvent_ShouldAddNewMarketParticipant(
            Guid newB2CActorId,
            TestMarketParticipant marketParticipant,
            [Frozen] Mock<IMarketParticipantRepository> marketParticipantRepository,
            MarketParticipantB2CActorIdChangedCommandHandler sut)
        {
            // Arrange
            var command = new MarketParticipantB2CActorIdChangedCommand(
                Guid.NewGuid(),
                newB2CActorId);
            marketParticipantRepository
                .Setup(m => m.GetByActorIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(marketParticipant);

            // Act
            await sut.HandleAsync(command);

            // Assert
            marketParticipant.Should().NotBeNull();
            marketParticipant.B2CActorId.Should().Be(newB2CActorId);
        }
    }
}
