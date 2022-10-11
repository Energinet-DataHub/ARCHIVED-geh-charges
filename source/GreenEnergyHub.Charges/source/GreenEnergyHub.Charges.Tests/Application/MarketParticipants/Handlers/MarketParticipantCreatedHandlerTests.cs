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
using Castle.Components.DictionaryAdapter;
using Energinet.DataHub.MarketParticipant.Integration.Model.Dtos;
using GreenEnergyHub.Charges.Application.MarketParticipants.Handlers;
using GreenEnergyHub.Charges.Domain.Dtos.MarketParticipantsUpdatedEvents;
using GreenEnergyHub.Charges.Domain.Dtos.SharedDtos;
using GreenEnergyHub.TestHelpers;
using Moq;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Application.MarketParticipants.Handlers
{
    [UnitTest]
    public class MarketParticipantCreatedHandlerTests
    {
        [Theory]
        [InlineAutoDomainData]
        public async Task
            HandleMarketParticipantCreatedIntegrationEventAsync_WhenCalled_ShouldCallMarketParticipantPersister(
                [Frozen] Mock<IMarketParticipantPersister> marketParticipantPersister,
                MarketParticipantCreatedHandler sut)
        {
            // Arrange
            var m = GetMarketParticipantUpdatedEvent();

            // Act
            await sut.HandleAsync(m);

            // Assert
            marketParticipantPersister.Verify(
                v => v.PersistAsync(It.IsAny<MarketParticipantUpdatedEvent>()),
                Times.Once);
        }

        private static MarketParticipantUpdatedEvent GetMarketParticipantUpdatedEvent()
        {
            return new MarketParticipantUpdatedEvent(
                actorId: Guid.NewGuid(),
                b2CActorId: Guid.NewGuid(),
                "mp123",
                new EditableList<MarketParticipantRole> { MarketParticipantRole.BalanceResponsibleParty, },
                MarketParticipantStatus.Active,
                new List<Guid> { Guid.NewGuid(), });
        }
    }
}
