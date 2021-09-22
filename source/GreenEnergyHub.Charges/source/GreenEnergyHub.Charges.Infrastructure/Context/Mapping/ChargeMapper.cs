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
using GreenEnergyHub.Charges.Domain.Charges;
using GreenEnergyHub.Charges.Domain.MarketDocument;
using GreenEnergyHub.Charges.Infrastructure.Context.Model;
using NodaTime;
using Charge = GreenEnergyHub.Charges.Infrastructure.Context.Model.Charge;
using ChargeOperation = GreenEnergyHub.Charges.Infrastructure.Context.Model.ChargeOperation;
using MarketParticipant = GreenEnergyHub.Charges.Infrastructure.Context.Model.MarketParticipant;

namespace GreenEnergyHub.Charges.Infrastructure.Context.Mapping
{
    public static class ChargeMapper
    {
        public static Domain.Charges.Charge MapChargeToChargeDomainModel(Charge charge)
        {
            if (charge == null) throw new ArgumentNullException(nameof(charge));

            var currentChargeDetails = charge.ChargePeriodDetails
                .OrderBy(x => Math.Abs((x.StartDateTime - DateTime.UtcNow).Ticks)).First();

            var fromDateTimeUtc = currentChargeDetails.EndDateTime != null ?
                Instant.FromDateTimeUtc(currentChargeDetails.EndDateTime.Value.ToUniversalTime()) : (Instant?)null;

            return new Domain.Charges.Charge(
                charge.Id,
                new Document(),
                charge.ChargeOperation.ChargeOperationId,
                charge.SenderProvidedChargeId,
                currentChargeDetails.Name,
                currentChargeDetails.Description,
                charge.MarketParticipant.MarketParticipantId,
                "Volt",
                charge.ChargeOperation.CorrelationId,
                Instant.FromDateTimeUtc(currentChargeDetails.StartDateTime.ToUniversalTime()),
                fromDateTimeUtc,
                (ChargeType)charge.ChargeType,
                (VatClassification)currentChargeDetails.VatClassification,
                (Resolution)charge.Resolution,
                Convert.ToBoolean(charge.TransparentInvoicing),
                Convert.ToBoolean(charge.TaxIndicator),
                charge.ChargePrices.Select(x => new Point
                {
                    Position = 0, Price = x.Price, Time = Instant.FromDateTimeUtc(x.Time.ToUniversalTime()),
                }).ToList());
        }

        public static Charge MapDomainChargeToCharge(
            [NotNull] Domain.Charges.Charge charge,
            MarketParticipant marketParticipant)
        {
            if (charge == null) throw new ArgumentNullException(nameof(charge));
            if (marketParticipant == null) throw new ArgumentNullException(nameof(marketParticipant));

            var chargeOperation = MapToChargeOperation(charge);

            return new Charge
            {
                Id = charge.Id,
                Currency = "DKK",
                MarketParticipantId = marketParticipant.Id,
                SenderProvidedChargeId = charge.SenderProvidedChargeId,
                ChargeType = (int)charge.Type,
                Resolution = (int)charge.Resolution,
                TaxIndicator = charge.TaxIndicator,
                TransparentInvoicing = charge.TransparentInvoicing,
                ChargePrices = MapChargeToChargePrice(charge, chargeOperation).ToList(),
                ChargePeriodDetails = new List<ChargePeriodDetails>
                {
                    MapChargeToChargePeriodDetails(charge, chargeOperation),
                },
                MarketParticipant = marketParticipant,
                ChargeOperation = chargeOperation,
            };
        }

        private static ChargeOperation MapToChargeOperation(
            [NotNull] Domain.Charges.Charge charge)
        {
            return new ChargeOperation
            {
                CorrelationId = charge.CorrelationId,
                WriteDateTime = charge.Document.RequestDate.ToDateTimeUtc().ToUniversalTime(),
                ChargeOperationId = charge.ChargeOperationId,
            };
        }

        private static IEnumerable<ChargePrice> MapChargeToChargePrice(
            [NotNull] Domain.Charges.Charge charge,
            ChargeOperation chargeOperation)
        {
            return charge.Points.Select(point => new ChargePrice
                {
                    Time = point.Time.ToDateTimeUtc(),
                    Price = point.Price,
                    ChargeOperation = chargeOperation,
                }).ToList();
        }

        private static ChargePeriodDetails MapChargeToChargePeriodDetails(
            [NotNull] Domain.Charges.Charge charge,
            ChargeOperation chargeOperation)
        {
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
