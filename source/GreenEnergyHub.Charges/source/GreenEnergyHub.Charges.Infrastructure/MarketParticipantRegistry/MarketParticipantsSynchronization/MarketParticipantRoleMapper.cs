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
using GreenEnergyHub.Charges.Infrastructure.MarketParticipantRegistry.Persistence.Actors;

namespace GreenEnergyHub.Charges.Infrastructure.MarketParticipantRegistry.MarketParticipantsSynchronization
{
    public static class MarketParticipantRoleMapper
    {
        public static MarketParticipantRole Map(Role role) => role switch
        {
            Role.Ddm => MarketParticipantRole.GridAccessProvider,
            Role.Ddq => MarketParticipantRole.EnergySupplier,
            Role.Ddz => MarketParticipantRole.MeteringPointAdministrator,
            Role.Ez => MarketParticipantRole.SystemOperator,
            _ => throw new NotImplementedException($"Role {role} is not implemented."),
        };

        public static Role Map(MarketParticipantRole role) => role switch
        {
            MarketParticipantRole.GridAccessProvider => Role.Ddm,
            MarketParticipantRole.EnergySupplier => Role.Ddq,
            MarketParticipantRole.MeteringPointAdministrator => Role.Ddz,
            MarketParticipantRole.SystemOperator => Role.Ez,
            _ => throw new NotImplementedException($"Role {role} is not implemented."),
        };
    }
}
