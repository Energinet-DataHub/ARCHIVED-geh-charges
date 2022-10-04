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
using GreenEnergyHub.Charges.Core.DateTime;
using GreenEnergyHub.Charges.QueryApi.Model;

namespace GreenEnergyHub.Charges.QueryApi.ModelPredicates;

public static class ChargeQueryLogic
{
#pragma warning disable SA1118

    public static IQueryable<ChargeV1Dto> AsChargeV1Dto(this IQueryable<Charge> queryable)
    {
        var todayAtMidnightUtc = DateTime.Now.Date.ToUniversalTime();

        return queryable.Select(c => new ChargeV1Dto(
            MapChargeType(c.GetChargeType()),
            MapResolution(c.GetResolution()),
            c.SenderProvidedChargeId,
            (c.ChargePeriods
                 .Where(cp => cp.StartDateTime <= todayAtMidnightUtc)
                 .OrderByDescending(cp => cp.StartDateTime)
                 .FirstOrDefault() ??
             c.ChargePeriods
                 .OrderBy(cp => cp.StartDateTime)
                 .First()).Name,
            c.Owner.MarketParticipantId,
            "<Aktørnavn XYZ>", // Hardcoded as we currently don't have the ChargeOwnerName data
            c.TaxIndicator,
            (c.ChargePeriods
                 .Where(cp => cp.StartDateTime <= todayAtMidnightUtc)
                 .OrderByDescending(cp => cp.StartDateTime)
                 .FirstOrDefault() ??
             c.ChargePeriods
                 .OrderBy(cp => cp.StartDateTime)
                 .First()).TransparentInvoicing,
            (c.ChargePeriods
                 .Where(cp => cp.StartDateTime <= todayAtMidnightUtc)
                 .OrderByDescending(cp => cp.StartDateTime)
                 .FirstOrDefault() ??
             c.ChargePeriods
                 .OrderBy(cp => cp.StartDateTime)
                 .First()).StartDateTime,
            GetValidToDate(c.ChargePeriods.OrderByDescending(cp => cp.EndDateTime).First().EndDateTime)));
    }

    private static DateTime? GetValidToDate(DateTime validToDate)
    {
        return validToDate == InstantExtensions.GetEndDefault().ToDateTimeUtc() ? null : validToDate;
    }
#pragma warning restore SA1118

    private static ChargeType MapChargeType(Domain.Charges.ChargeType chargeType) => chargeType switch
    {
        Domain.Charges.ChargeType.Fee => ChargeType.D02,
        Domain.Charges.ChargeType.Subscription => ChargeType.D01,
        Domain.Charges.ChargeType.Tariff => ChargeType.D03,
        Domain.Charges.ChargeType.Unknown =>
            throw new NotSupportedException($"Charge type '{Domain.Charges.ChargeType.Unknown}' is not supported"),
        _ =>
            throw new ArgumentOutOfRangeException(nameof(chargeType)),
    };

    private static Resolution MapResolution(Domain.Charges.Resolution resolution) => resolution switch
    {
        Domain.Charges.Resolution.PT15M => Resolution.PT15M,
        Domain.Charges.Resolution.PT1H => Resolution.PT1H,
        Domain.Charges.Resolution.P1D => Resolution.P1D,
        Domain.Charges.Resolution.P1M => Resolution.P1M,
        Domain.Charges.Resolution.Unknown =>
            throw new NotSupportedException($"Resolution '{Domain.Charges.Resolution.Unknown}' is not supported"),
        _ =>
            throw new ArgumentOutOfRangeException(nameof(resolution)),
    };
}
