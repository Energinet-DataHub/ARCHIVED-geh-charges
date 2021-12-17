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

using System.Linq;
using Energinet.Charges.Contracts.Charge;
using Energinet.Charges.Contracts.ChargeLink;
using GreenEnergyHub.Charges.QueryApi.Model;

namespace GreenEnergyHub.Charges.WebApi.ModelPredicates
{
    public static class ChargeLinkQueryLogic
    {
        public static IQueryable<ChargeLinkDto> AsChargeLinkDto(this IQueryable<ChargeLink> queryable)
        {
            return queryable
                .Select(c => new ChargeLinkDto(
                    (ChargeType)c.Charge.GetChargeType(), // TODO BJARKE: Map correctly
                    c.Charge.SenderProvidedChargeId,
                    c.Charge.Name,
                    c.Charge.Owner.MarketParticipantId,
                    "Netvirksomhed XYZ", // Hardcoded as we currently don't have the data
                    c.Charge.TaxIndicator,
                    c.Charge.TransparentInvoicing,
                    c.Factor,
                    c.StartDateTime,
                    c.EndDateTime));
        }
    }
}
