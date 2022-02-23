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
            var todayAtMidnightUtc = DateTime.Now.Date.ToUniversalTime();

#pragma warning disable SA1118
            return queryable
                .Select(cl => new ChargeLinkV1Dto(
                    Map(cl.Charge.GetChargeType()),
                    cl.Charge.SenderProvidedChargeId,
                    (cl.Charge.ChargePeriods
                        .Where(cp => cp.StartDateTime <= todayAtMidnightUtc)
                        .OrderByDescending(cp => cp.StartDateTime)
                        .FirstOrDefault() ??
                     cl.Charge.ChargePeriods
                         .OrderBy(cp => cp.StartDateTime)
                         .First()).Name,
                    cl.Charge.Owner.MarketParticipantId,
                    "<Aktørnavn XYZ>", // Hardcoded as we currently don't have the ChargeOwnerName data
                    cl.Charge.TaxIndicator,
                    (cl.Charge.ChargePeriods
                        .Where(cp => cp.StartDateTime <= todayAtMidnightUtc)
                        .OrderByDescending(cp => cp.StartDateTime)
                        .FirstOrDefault() ??
                     cl.Charge.ChargePeriods
                         .OrderBy(cp => cp.StartDateTime)
                         .First()).TransparentInvoicing,
                    cl.Factor,
                    cl.StartDateTime,
                    cl.EndDateTime == InstantExtensions.GetEndDefault().ToDateTimeOffset() ? null : cl.EndDateTime)); // Nullify "EndDefault" end dates
#pragma warning restore SA1118
        }

        public static IQueryable<ChargeLinkV2Dto> AsChargeLinkV2Dto(this IQueryable<ChargeLink> queryable)
        {
            var todayAtMidnightUtc = DateTime.Now.Date.ToUniversalTime();

#pragma warning disable SA1118
            return queryable
                .Select(cl => new ChargeLinkV2Dto(
                    Map(cl.Charge.GetChargeType()),
                    cl.Charge.SenderProvidedChargeId,
                    (cl.Charge.ChargePeriods
                         .Where(cp => cp.StartDateTime <= todayAtMidnightUtc)
                         .OrderByDescending(cp => cp.StartDateTime)
                         .FirstOrDefault() ??
                     cl.Charge.ChargePeriods
                         .OrderBy(cp => cp.StartDateTime)
                         .First()).Name,
                    cl.Charge.Owner.Id,
                    cl.Charge.TaxIndicator,
                    (cl.Charge.ChargePeriods
                        .Where(cp => cp.StartDateTime <= todayAtMidnightUtc)
                        .OrderByDescending(cp => cp.StartDateTime)
                        .FirstOrDefault() ??
                     cl.Charge.ChargePeriods
                         .OrderBy(cp => cp.StartDateTime)
                         .First()).TransparentInvoicing,
                    cl.Factor,
                    cl.StartDateTime,
                    cl.EndDateTime == InstantExtensions.GetEndDefault().ToDateTimeOffset() ? null : cl.EndDateTime)); // Nullify "EndDefault" end dates
#pragma warning restore SA1118
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
