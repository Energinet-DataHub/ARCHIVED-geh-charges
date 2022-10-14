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
using System.Linq;
using AutoFixture.Xunit2;
using FluentAssertions;
using GreenEnergyHub.Charges.Domain.Dtos.SharedDtos;
using GreenEnergyHub.Charges.Domain.MarketParticipants;
using Xunit;

namespace GreenEnergyHub.Charges.Tests.Domain.MarketParticipants
{
    public class MarketParticipantTests
    {
        [Theory]
        [AutoData]
        public void Create_ReturnsMarketParticipant(
            Guid actorId,
            Guid b2CActorId,
            string marketParticipantId,
            MarketParticipantStatus status,
            MarketParticipantRole businessProcessRole)
        {
            var actual =
                MarketParticipant.Create(actorId, b2CActorId, marketParticipantId, status, businessProcessRole);

            actual.ActorId.Should().Be(actorId);
            actual.B2CActorId.Should().Be(b2CActorId);
            actual.MarketParticipantId.Should().Be(marketParticipantId);
            actual.Status.Should().Be(status);
            actual.BusinessProcessRole.Should().Be(businessProcessRole);
        }

        [Theory]
        [AutoData]
        public void Update_ShouldUpdateProperties(
            Guid actorId,
            Guid b2CActorId,
            string marketParticipantId,
            MarketParticipantStatus status,
            MarketParticipantRole businessProcessRole,
            Guid newActorId,
            Guid newB2CActorId,
            MarketParticipantStatus newStatus)
        {
            // Arrange
            var actual =
                MarketParticipant.Create(actorId, b2CActorId, marketParticipantId, status, businessProcessRole);

            // Act
            actual.Update(newActorId, newB2CActorId, newStatus);

            // Assert
            actual.ActorId.Should().Be(newActorId);
            actual.B2CActorId.Should().Be(newB2CActorId);
            actual.Status.Should().Be(newStatus);
        }
    }
}
