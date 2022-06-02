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
using Energinet.DataHub.MarketParticipant.Integration.Model.Dtos;
using GreenEnergyHub.Charges.Application.MarketParticipants.Handlers;
using GreenEnergyHub.Charges.Domain.Dtos.GridAreas;
using GreenEnergyHub.Charges.Domain.Dtos.MarketParticipantsUpdatedEvents;
using GreenEnergyHub.TestHelpers;
using Moq;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Application.MarketParticipants.Handlers
{
    public class MarketParticipantEventHandlerTests
    {
        [UnitTest]
        public class MarketParticipantPersisterTests
        {
            [Theory]
            [InlineAutoDomainData]
            public async Task
                HandleAsync_WhenCalledWithActorUpdateIntegrationEvent_ShouldCallMarketParticipantPersister(
                    [Frozen] Mock<IMarketParticipantPersister> marketParticipantPersister,
                    MarketParticipantEventHandler sut)
            {
                // Assert
                var actorUpdatedIntegrationEvent = new ActorUpdatedIntegrationEvent(
                    Guid.Empty,
                    Guid.Empty,
                    Guid.Empty,
                    Guid.Empty,
                    string.Empty,
                    ActorStatus.New,
                    new List<BusinessRoleCode>() { BusinessRoleCode.Ddm },
                    new List<EicFunction>(),
                    new List<Guid>(),
                    new List<string>());

                // Act
                await sut.HandleAsync(actorUpdatedIntegrationEvent);

                // Assert
                marketParticipantPersister
                    .Verify(v => v.PersistAsync(It.IsAny<MarketParticipantUpdatedEvent>()), Times.Exactly(1));
            }

            [Theory]
            [InlineAutoDomainData]
            public async Task
                HandleAsync_WhenCalledWithGridAreaUpdateIntegrationEvent_ShouldCallGridAreaPersister(
                    [Frozen] Mock<IGridAreaLinkPersister> gridAreaPersister,
                    GridAreaUpdatedIntegrationEvent gridAreaUpdatedIntegrationEvent,
                    MarketParticipantEventHandler sut)
            {
                // Act
                await sut.HandleAsync(gridAreaUpdatedIntegrationEvent);

                // Assert
                gridAreaPersister
                    .Verify(v => v.PersistAsync(It.IsAny<GridAreaUpdatedEvent>()), Times.Exactly(1));
            }
        }
    }
}
