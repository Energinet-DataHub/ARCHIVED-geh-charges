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
using System.Linq;
using Energinet.Charges.Contracts.Charge;
using Energinet.Charges.Contracts.ChargeLink;
using GreenEnergyHub.Charges.Core.DateTime;
using GreenEnergyHub.Charges.QueryApi.Model;

namespace GreenEnergyHub.Charges.WebApi.ModelPredicates
{
    public static class ChargeLinkQueryLogic
    {
        public static IQueryable<ChargeLinkDto> AsChargeLinkDto(this IQueryable<ChargeLink> queryable)
        {
            return queryable
                .Select(c => new ChargeLinkDto(
                    Map(c.Charge.GetChargeType()),
                    c.Charge.SenderProvidedChargeId,
                    c.Charge.Name,
                    c.Charge.Owner.MarketParticipantId,
                    "<Aktørnavn XYZ>", // Hardcoded as we currently don't have the ChargeOwnerName data
                    c.Charge.TaxIndicator,
                    c.Charge.TransparentInvoicing,
                    c.Factor,
                    c.StartDateTime,
                    // Nullify any "EndDefault" end dates that are not supposed to be communicated externally.
                    c.EndDateTime == InstantExtensions.GetEndDefault().ToDateTimeOffset() ? null : c.EndDateTime));
        }

        private static ChargeType Map(Domain.Charges.ChargeType chargeType) => chargeType switch
        {
            Domain.Charges.ChargeType.Fee => ChargeType.D02,
            Domain.Charges.ChargeType.Subscription => ChargeType.D01,
            Domain.Charges.ChargeType.Tariff => ChargeType.D03,
            Domain.Charges.ChargeType.Unknown =>
                throw new NotSupportedException($"Charge type '{Domain.Charges.ChargeType.Unknown}' is not supported"),
            _ =>
                throw new ArgumentOutOfRangeException(nameof(chargeType)),
        };
    }
}
