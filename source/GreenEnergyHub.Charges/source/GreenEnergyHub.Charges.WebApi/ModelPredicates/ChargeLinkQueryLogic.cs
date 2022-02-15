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
        public static IQueryable<ChargeLinkV1Dto> AsChargeLinkV1Dto(this IQueryable<ChargeLink> queryable)
        {
            return queryable
                .Select(c => new ChargeLinkV1Dto(
                    Map(c.Charge.GetChargeType()),
                    c.Charge.SenderProvidedChargeId,
                    c.Charge.Name,
                    c.Charge.Owner.MarketParticipantId,
                    "<Aktørnavn XYZ>", // Hardcoded as we currently don't have the ChargeOwnerName data
                    c.Charge.TaxIndicator,
                    c.Charge.TransparentInvoicing,
                    c.Factor,
                    c.StartDateTime,
                    c.EndDateTime == InstantExtensions.GetEndDefault().ToDateTimeOffset() ? null : c.EndDateTime)); // Nullify "EndDefault" end dates
        }

        public static IQueryable<ChargeLinkV2Dto> AsChargeLinkV2Dto(this IQueryable<ChargeLink> queryable)
        {
            return queryable
                .Select(c => new ChargeLinkV2Dto(
                    Map(c.Charge.GetChargeType()),
                    c.Charge.SenderProvidedChargeId,
                    c.Charge.Name,
                    c.Charge.Owner.Id,
                    c.Charge.TaxIndicator,
                    c.Charge.TransparentInvoicing,
                    c.Factor,
                    c.StartDateTime,
                    c.EndDateTime == InstantExtensions.GetEndDefault().ToDateTimeOffset() ? null : c.EndDateTime)); // Nullify "EndDefault" end dates
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
