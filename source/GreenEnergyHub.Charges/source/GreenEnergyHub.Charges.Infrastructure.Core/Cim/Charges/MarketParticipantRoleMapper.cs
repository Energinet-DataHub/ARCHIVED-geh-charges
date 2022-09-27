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

using System.ComponentModel;
using GreenEnergyHub.Charges.Domain.Dtos.SharedDtos;

namespace GreenEnergyHub.Charges.Infrastructure.Core.Cim.Charges
{
    public static class MarketParticipantRoleMapper
    {
        private const string BalanceResponsibleParty = "DDK";
        private const string EnergyAgency = "STS";
        private const string EnergySupplier = "DDQ";
        private const string GridAccessProvider = "DDM";
        private const string ImbalanceSettlementResponsible = "DDX";
        private const string MeteredDataResponsible = "MDR";
        private const string MeteredDataAdministrator = "DGL";
        private const string MeteringPointAdministrator = "DDZ";
        private const string SystemOperator = "EZ";
        private const string TransmissionSystemOperator = "TSO";

        public static MarketParticipantRole Map(string value)
        {
            return value switch
            {
                BalanceResponsibleParty => MarketParticipantRole.BalanceResponsibleParty,
                EnergyAgency => MarketParticipantRole.EnergyAgency,
                EnergySupplier => MarketParticipantRole.EnergySupplier,
                GridAccessProvider => MarketParticipantRole.GridAccessProvider,
                ImbalanceSettlementResponsible => MarketParticipantRole.ImbalanceSettlementResponsible,
                MeteredDataResponsible => MarketParticipantRole.MeteredDataResponsible,
                MeteredDataAdministrator => MarketParticipantRole.MeteredDataAdministrator,
                MeteringPointAdministrator => MarketParticipantRole.MeteringPointAdministrator,
                SystemOperator => MarketParticipantRole.SystemOperator,
                TransmissionSystemOperator => MarketParticipantRole.TransmissionSystemOperator,
                _ => MarketParticipantRole.Unknown,
            };
        }

        public static string Map(MarketParticipantRole marketParticipantRole)
        {
            return marketParticipantRole switch
            {
                MarketParticipantRole.BalanceResponsibleParty => BalanceResponsibleParty,
                MarketParticipantRole.EnergyAgency => EnergyAgency,
                MarketParticipantRole.EnergySupplier => EnergySupplier,
                MarketParticipantRole.GridAccessProvider => GridAccessProvider,
                MarketParticipantRole.ImbalanceSettlementResponsible => ImbalanceSettlementResponsible,
                MarketParticipantRole.MeteredDataAdministrator => MeteredDataAdministrator,
                MarketParticipantRole.MeteredDataResponsible => MeteredDataResponsible,
                MarketParticipantRole.MeteringPointAdministrator => MeteringPointAdministrator,
                MarketParticipantRole.SystemOperator => SystemOperator,
                MarketParticipantRole.TransmissionSystemOperator => TransmissionSystemOperator,
                                _ => throw new InvalidEnumArgumentException(
                    $"Provided MarketParticipantRole value '{marketParticipantRole}' is invalid and cannot be mapped."),
            };
        }
    }
}
