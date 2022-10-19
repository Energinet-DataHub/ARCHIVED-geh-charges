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
using GreenEnergyHub.Charges.Application.MarketParticipants.Handlers;
using GreenEnergyHub.Charges.Domain.Dtos.Events;
using GreenEnergyHub.Charges.Domain.GridAreaLinks;
using GreenEnergyHub.Charges.Domain.MarketParticipants;
using GreenEnergyHub.Charges.TestCore.Attributes;
using GreenEnergyHub.Charges.TestCore.Builders.Testables;
using Moq;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Application.MarketParticipants.Handlers
{
    [UnitTest]
    public class GridAreaRemovedFromMarketParticipantCommandHandlerTests
    {
        [Theory]
        [InlineAutoMoqData]
        public async Task HandleAsync_ValidEvent_ShouldRemoveGridAreaOwner(
            GridAreaLink gridAreaLink,
            [Frozen] Mock<IGridAreaLinkRepository> gridAreaLinkRepository,
            GridAreaRemovedFromMarketParticipantCommandHandler sut)
        {
            // Arrange
            var command = new GridAreaRemovedFromMarketParticipantCommand(
                Guid.NewGuid());
            gridAreaLinkRepository
                .Setup(g => g.GetGridAreaOrNullAsync(It.IsAny<Guid>()))
                .ReturnsAsync(gridAreaLink);

            gridAreaLink.OwnerId.Should().NotBeNull();

            // Act
            await sut.HandleAsync(command);

            // Assert
            gridAreaLink.OwnerId.Should().BeNull();
        }
    }
}
