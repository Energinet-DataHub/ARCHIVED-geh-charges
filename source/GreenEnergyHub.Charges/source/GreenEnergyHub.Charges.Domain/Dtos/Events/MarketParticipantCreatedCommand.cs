﻿// Copyright 2020 Energinet DataHub A/S
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
using GreenEnergyHub.Charges.Domain.Dtos.SharedDtos;
using NodaTime;

namespace GreenEnergyHub.Charges.Domain.Dtos.Events
{
    public class MarketParticipantCreatedCommand
    {
        public MarketParticipantCreatedCommand(
            Guid actorId,
            Guid? b2CActorId,
            string marketParticipantId,
            IEnumerable<MarketParticipantRole> businessProcessRoles,
            MarketParticipantStatus status,
            IEnumerable<Guid> gridAreas)
        {
            B2CActorId = b2CActorId;
            ActorId = actorId;
            MarketParticipantId = marketParticipantId;
            BusinessProcessRoles = businessProcessRoles;
            Status = status;
            GridAreas = gridAreas;
        }

        public Guid ActorId { get; }

        public Guid? B2CActorId { get; }

        public string MarketParticipantId { get; }

        public IEnumerable<MarketParticipantRole> BusinessProcessRoles { get; }

        public MarketParticipantStatus Status { get; }

        public IEnumerable<Guid> GridAreas { get; }
    }
}
