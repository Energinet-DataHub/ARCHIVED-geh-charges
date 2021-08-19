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
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using GreenEnergyHub.Charges.Domain;
using GreenEnergyHub.Charges.Domain.ChangeOfCharges.Transaction;
using GreenEnergyHub.Charges.Infrastructure.Context.Model;
using NodaTime;

namespace GreenEnergyHub.Charges.Infrastructure.Context.Mapping
{
    public static class ChargeMapper
    {
        public static Charge MapChargeContextModelToDomainModel(DBCharge charge)
        {
            if (charge == null) throw new ArgumentNullException(nameof(charge));

            var currentChargeDetails = charge.ChargePeriodDetails
                .OrderBy(x => Math.Abs((x.StartDateTime - DateTime.UtcNow).Ticks)).First();

            return new Charge
            {
                Id = charge.ChargeId,
                Type = (ChargeType)charge.ChargeType,
                Name = currentChargeDetails.Name,
                Description = currentChargeDetails.Description,
                StartDateTime = Instant.FromDateTimeUtc(currentChargeDetails.StartDateTime),
                Owner = charge.MarketParticipant.MarketParticipantId,
                Resolution = (Resolution)charge.Resolution,
                TaxIndicator = Convert.ToBoolean(charge.TaxIndicator),
                TransparentInvoicing = Convert.ToBoolean(charge.TransparentInvoicing),
                VatClassification = (VatClassification)currentChargeDetails.VatClassification,
                ChargeOperationId = charge.ChargeOperation.ChargeOperationId,
                EndDateTime = currentChargeDetails.EndDateTime != null ?
                    Instant.FromDateTimeUtc(currentChargeDetails.EndDateTime.Value) : (Instant?)null,
                Points = charge.ChargePrices.Select(x => new Point
                {
                    Position = 0,
                    Price = x.Price,
                    Time = Instant.FromDateTimeUtc(x.Time),
                }).ToList(),
            };
        }

        public static DBCharge MapChargeDomainModelToContextModel(
            [NotNull] Charge charge,
            DBMarketParticipant marketParticipant)
        {
            if (charge == null) throw new ArgumentNullException(nameof(charge));
            if (marketParticipant == null) throw new ArgumentNullException(nameof(marketParticipant));

            var chargeOperation = MapToChargeOperation(charge);

            return new DBCharge
            {
                Currency = "DKK",
                MarketParticipantRowId = marketParticipant.RowId,
                ChargeId = charge.Id,
                ChargeType = (int)charge.Type,
                Resolution = (int)charge.Resolution,
                TaxIndicator = charge.TaxIndicator,
                TransparentInvoicing = charge.TransparentInvoicing,
                ChargePrices = MapChargeToChargePrice(charge, chargeOperation).ToList(),
                ChargePeriodDetails = new List<DBChargePeriodDetails>
                {
                    MapChargeToChargePeriodDetails(charge, chargeOperation),
                },
                MarketParticipant = marketParticipant,
                ChargeOperation = chargeOperation,
            };
        }

        private static DBChargeOperation MapToChargeOperation(
            [NotNull] Charge charge)
        {
            return new DBChargeOperation
            {
                CorrelationId = charge.CorrelationId,
                WriteDateTime = charge.Document.RequestDate.ToDateTimeUtc(),
                ChargeOperationId = charge.ChargeOperationId,
            };
        }

        private static IEnumerable<DBChargePrice> MapChargeToChargePrice(
            [NotNull] Charge charge,
            DBChargeOperation chargeOperation)
        {
            return charge.Points.Select(point => new DBChargePrice
                {
                    Time = point.Time.ToDateTimeUtc(),
                    Price = point.Price,
                    ChargeOperation = chargeOperation,
                }).ToList();
        }

        private static DBChargePeriodDetails MapChargeToChargePeriodDetails(
            [NotNull] Charge charge,
            DBChargeOperation chargeOperation)
        {
            return new DBChargePeriodDetails
            {
                Description = charge.Description,
                Name = charge.Name,
                VatClassification = (int)charge.VatClassification,
                EndDateTime = charge.EndDateTime?.ToDateTimeUtc(),
                StartDateTime = charge.StartDateTime.ToDateTimeUtc(),
                ChargeOperation = chargeOperation,
            };
        }
    }
}
