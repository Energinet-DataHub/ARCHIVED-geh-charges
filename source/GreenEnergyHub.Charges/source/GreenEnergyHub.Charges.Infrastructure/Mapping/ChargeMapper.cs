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
using System.Linq;
using GreenEnergyHub.Charges.Domain.ChangeOfCharges.Transaction;
using GreenEnergyHub.Charges.Infrastructure.Context.Model;
using JetBrains.Annotations;
using NodaTime;
using ChargeOperation = GreenEnergyHub.Charges.Infrastructure.Context.Model.ChargeOperation;
using MarketParticipant = GreenEnergyHub.Charges.Infrastructure.Context.Model.MarketParticipant;

namespace GreenEnergyHub.Charges.Infrastructure.Mapping
{
    public static class ChargeMapper
    {
        public static ChargeOperation MapToChargeOperation(
            [NotNull] Domain.Charge charge)
        {
            if (charge == null) throw new ArgumentNullException(nameof(charge));

            return new ChargeOperation
            {
                CorrelationId = charge.Document.CorrelationId,
                WriteDateTime = charge.Document.RequestDate.ToDateTimeUtc(),
                ChargeOperationId = charge.ChargeOperationId,
            };
        }

        public static Domain.Charge MapChargeToChargeDomainModel(Charge charge)
        {
            if (charge == null) throw new ArgumentNullException(nameof(charge));

            var validChargeDetails = charge.ChargePeriodDetails
                .OrderBy(x => Math.Abs((x.StartDateTime - DateTime.UtcNow).Ticks)).First();

            return new Domain.Charge
            {
                Id = charge.ChargeId,
                Type = (ChargeType)charge.ChargeType,
                Name = validChargeDetails.Name,
                Description = validChargeDetails.Description,
                StartDateTime = Instant.FromDateTimeUtc(validChargeDetails.StartDateTime),
                Owner = charge.MarketParticipant.MarketParticipantId,
                Resolution = (Resolution)charge.Resolution,
                TaxIndicator = Convert.ToBoolean(charge.TaxIndicator),
                TransparentInvoicing = Convert.ToBoolean(charge.TransparentInvoicing),
                VatClassification = (VatClassification)validChargeDetails.VatClassification,
                ChargeOperationId = charge.ChargeId,
                EndDateTime = validChargeDetails.EndDateTime != null ? Instant.FromDateTimeUtc(validChargeDetails.EndDateTime.Value) : null,
                Points = charge.ChargePrices.Select(x => new Point
                {
                    Position = x.RowId,
                    Price = x.Price,
                    Time = Instant.FromDateTimeUtc(x.Time),
                }).ToList(),
            };
        }

        public static Charge MapDomainChargeToCharge(
            [NotNull] Domain.Charge charge,
            MarketParticipant marketParticipant)
        {
            if (charge == null) throw new ArgumentNullException(nameof(charge));
            if (marketParticipant == null) throw new ArgumentNullException(nameof(marketParticipant));

            var chargeOperation = MapToChargeOperation(charge);
            return new Charge
            {
                Currency = "DKK",
                MarketParticipantRowId = marketParticipant.RowId,
                ChargeId = charge.Id,
                ChargeType = (int)charge.Type,
                Resolution = (int)charge.Resolution,
                TaxIndicator = Convert.ToByte(charge.TaxIndicator),
                TransparentInvoicing = Convert.ToByte(charge.TransparentInvoicing),
                ChargePrices = MapChargeToChargePrice(charge, chargeOperation).ToList(),
                ChargePeriodDetails = new List<ChargePeriodDetails>
                {
                    MapChargeToChargePeriodDetails(charge, chargeOperation),
                },
                MarketParticipant = marketParticipant,
                ChargeOperation = chargeOperation,
            };
        }

        private static IEnumerable<ChargePrice> MapChargeToChargePrice(
            [NotNull] Domain.Charge charge,
            ChargeOperation chargeOperation)
        {
            if (charge == null) throw new ArgumentNullException(nameof(charge));

            return charge.Points.Select(point => new ChargePrice
                {
                    Time = point.Time.ToDateTimeUtc(),
                    Price = point.Price,
                    Retired = false,
                    ChargeOperation = chargeOperation,
                }).ToList();
        }

        private static ChargePeriodDetails MapChargeToChargePeriodDetails(
            [NotNull] Domain.Charge charge,
            ChargeOperation chargeOperation)
        {
            if (charge == null) throw new ArgumentNullException(nameof(charge));

            return new ChargePeriodDetails
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
