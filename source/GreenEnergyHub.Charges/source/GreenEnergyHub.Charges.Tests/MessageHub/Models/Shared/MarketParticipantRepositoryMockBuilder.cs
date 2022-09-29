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
using GreenEnergyHub.Charges.Domain.Dtos.SharedDtos;
using GreenEnergyHub.Charges.Domain.MarketParticipants;
using Moq;

namespace GreenEnergyHub.Charges.Tests.MessageHub.Models.Shared
{
    public class MarketParticipantRepositoryMockBuilder
    {
        public static void SetupMarketParticipantRepositoryMock(
            Mock<IMarketParticipantRepository> marketParticipantRepository,
            MarketParticipant meteringPointAdministrator,
            MarketParticipantDto marketParticipantDto,
            Guid actorId)
        {
            marketParticipantRepository
                .Setup(r => r.GetMeteringPointAdministratorAsync())
                .ReturnsAsync(meteringPointAdministrator);

            var sender = new MarketParticipant(
                marketParticipantDto.Id,
                actorId,
                marketParticipantDto.B2CActorId,
                marketParticipantDto.MarketParticipantId,
                MarketParticipantStatus.Active,
                marketParticipantDto.BusinessProcessRole);

            marketParticipantRepository
                .Setup(r => r.GetSystemOperatorOrGridAccessProviderAsync(It.IsAny<string>()))
                .ReturnsAsync(sender);
        }
    }
}
