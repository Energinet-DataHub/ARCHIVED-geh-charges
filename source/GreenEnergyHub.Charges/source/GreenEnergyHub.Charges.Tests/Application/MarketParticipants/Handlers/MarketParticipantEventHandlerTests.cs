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
using System.Threading.Tasks;
using AutoFixture.Xunit2;
using GreenEnergyHub.Charges.Application.MarketParticipants.Handlers;
using GreenEnergyHub.Charges.Domain.Dtos.Events;
using GreenEnergyHub.Charges.Domain.Dtos.SharedDtos;
using GreenEnergyHub.TestHelpers;
using Moq;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Application.MarketParticipants.Handlers
{
    [UnitTest]
    public class MarketParticipantEventHandlerTests
    {
        [Theory]
        [InlineAutoDomainData]
        public async Task
            HandleAsync_WhenCalledWithActorUpdatedIntegrationEvent_ShouldCallMarketParticipantPersister(
                Guid actorId,
                Guid b2CActorId,
                string actorNumber,
                [Frozen] Mock<IMarketParticipantPersister> marketParticipantPersister,
                MarketParticipantUpdatedCommandHandler sut)
        {
            // Assert
            var actorUpdatedIntegrationEvent = new MarketParticipantUpdatedCommand(
                actorId,
                b2CActorId,
                actorNumber,
                new List<MarketParticipantRole> { MarketParticipantRole.EnergySupplier },
                MarketParticipantStatus.Active,
                new List<Guid>());

            // Act
            await sut.HandleAsync(actorUpdatedIntegrationEvent);

            // Assert
            marketParticipantPersister
                .Verify(v => v.PersistAsync(It.IsAny<MarketParticipantUpdatedCommand>()), Times.Once);
        }

        [Theory]
        [InlineAutoDomainData]
        public async Task
            HandleAsync_WhenCalledWithGridAreaUpdatedIntegrationEvent_ShouldCallGridAreaPersister(
                [Frozen] Mock<IGridAreaLinkPersister> gridAreaPersister,
                MarketParticipantGridAreaUpdatedCommand gridAreaUpdatedIntegrationEvent,
                MarketParticipantGridAreaUpdatedCommandHandler sut)
        {
            // Act
            await sut.HandleAsync(gridAreaUpdatedIntegrationEvent);

            // Assert
            gridAreaPersister.Verify(v => v.PersistAsync(It.IsAny<MarketParticipantGridAreaUpdatedCommand>()), Times.Once);
        }
    }
}
