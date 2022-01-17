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
using GreenEnergyHub.Charges.Domain.MarketParticipants;

namespace GreenEnergyHub.Charges.Infrastructure.Core.Cim.MarketDocument
{
    public static class MarketParticipantRoleMapper
    {
        private const string CimEnergySupplier = "DDQ";
        private const string CimGridAccessProvider = "DDM";
        private const string CimMeteringPointAdministrator = "DDZ";
        private const string CimSystemOperator = "EZ";

        public static MarketParticipantRole Map(string value)
        {
            return value switch
            {
                CimEnergySupplier => MarketParticipantRole.EnergySupplier,
                CimGridAccessProvider => MarketParticipantRole.GridAccessProvider,
                CimSystemOperator => MarketParticipantRole.SystemOperator,
                CimMeteringPointAdministrator => MarketParticipantRole.MeteringPointAdministrator,
                _ => MarketParticipantRole.Unknown,
            };
        }

        public static string Map(MarketParticipantRole marketParticipantRole)
        {
            return marketParticipantRole switch
            {
                MarketParticipantRole.EnergySupplier => CimEnergySupplier,
                MarketParticipantRole.GridAccessProvider => CimGridAccessProvider,
                MarketParticipantRole.MeteringPointAdministrator => CimMeteringPointAdministrator,
                MarketParticipantRole.SystemOperator => CimSystemOperator,
                _ => throw new InvalidEnumArgumentException(
                    $"Provided MarketParticipantRole value '{marketParticipantRole}' is invalid and cannot be mapped."),
            };
        }
    }
}
