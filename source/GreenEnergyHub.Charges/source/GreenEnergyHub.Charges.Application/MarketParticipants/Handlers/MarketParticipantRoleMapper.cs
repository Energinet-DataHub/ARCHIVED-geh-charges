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
using Energinet.DataHub.MarketParticipant.Integration.Model.Dtos;
using GreenEnergyHub.Charges.Domain.Dtos.SharedDtos;

namespace GreenEnergyHub.Charges.Application.MarketParticipants.Handlers
{
    public static class MarketParticipantRoleMapper
    {
        public static MarketParticipantRole Map(BusinessRoleCode businessRole) => businessRole switch
        {
            BusinessRoleCode.Ddm => MarketParticipantRole.GridAccessProvider,
            BusinessRoleCode.Ddq => MarketParticipantRole.EnergySupplier,
            BusinessRoleCode.Ddz => MarketParticipantRole.MeteringPointAdministrator,
            BusinessRoleCode.Ez => MarketParticipantRole.SystemOperator,
            BusinessRoleCode.Ddk => MarketParticipantRole.BalanceResponsibleParty,
            BusinessRoleCode.Ddx => MarketParticipantRole.ImbalanceSettlementResponsible,
            BusinessRoleCode.Dea => MarketParticipantRole.MeteredDataAggregator,
            BusinessRoleCode.Dgl => MarketParticipantRole.MeteredDataAdministrator,
            BusinessRoleCode.Mdr => MarketParticipantRole.MeteredDataResponsible,
            BusinessRoleCode.Sts => MarketParticipantRole.EnergyAgency,
            _ => throw new ArgumentOutOfRangeException(
                nameof(businessRole), businessRole, $"Role {businessRole} is not implemented"),
        };
    }
}
