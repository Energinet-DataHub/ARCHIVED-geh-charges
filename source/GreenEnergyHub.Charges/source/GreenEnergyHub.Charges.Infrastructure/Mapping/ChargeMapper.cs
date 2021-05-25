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
using GreenEnergyHub.Charges.Domain.Common;
using GreenEnergyHub.Charges.Infrastructure.Context.Model;
using JetBrains.Annotations;
using NodaTime;
using ChargeOperation = GreenEnergyHub.Charges.Infrastructure.Context.Model.ChargeOperation;

namespace GreenEnergyHub.Charges.Infrastructure.Mapping
{
    public static class ChargeMapper
    {
        public static Charge MapDomainChargeToCharge(
            [NotNull] Domain.Charge c)
        {
            if (c == null) throw new ArgumentNullException(nameof(c));

            var charge = new Charge
            {
                Currency = "DKK",
                Name = c.Name,
                Owner = c.Owner,
                ChargeId = c.Id,
                ChargeType = (int)c.Type,
                ResolutionType = (int)c.Resolution,
                TaxIndicator = Convert.ToByte(c.TaxIndicator),
                TransparentInvoicing = Convert.ToByte(c.TransparentInvoicing),
            };

            return charge;
        }

        public static ChargeOperation CreateChargeOperation(
            [NotNull] Domain.Charge c,
            int chargeRowId)
        {
            if (c == null) throw new ArgumentNullException(nameof(c));

            return new ChargeOperation
            {
                CorrelationId = c.Document.CorrelationId,
                WriteDateTime = c.Document.CreatedDateTime.ToDateTimeUtc(),
                ChargeOperationId = c.Document.Id,
                ChargeRowId = chargeRowId,
            };
        }

        public static IEnumerable<ChargePrice> CreateChargePrice(
            [NotNull] Domain.Charge c,
            int chargeRowId,
            int chargeOperationRowId)
        {
            if (c == null) throw new ArgumentNullException(nameof(c));

            return c.Points.Select(point => new ChargePrice
                {
                    Time = point.Time.ToUnixTimeTicks(),
                    Price = point.Price,
                    Retired = false,
                    ChargeRowId = chargeRowId,
                    ChargeOperationRowId = chargeOperationRowId,
                })
                .ToList();
        }

        public static ChargePeriodDetails CreateChargePeriodDetails(
            [NotNull] Domain.Charge c,
            int chargeRowId,
            int chargeOperationRowId)
        {
            if (c == null) throw new ArgumentNullException(nameof(c));

            return new ChargePeriodDetails
            {
                Description = c.Description,
                Name = c.Name,
                VatClassification = (int)c.VatClassification,
                ChargeRowId = chargeRowId,
                EndTimeDate = c.EndDateTime?.ToDateTimeUtc(),
                StartTimeDate = c.StartDateTime.ToDateTimeUtc(),
                ChargeOperationRowId = chargeOperationRowId,
            };
        }

        public static Domain.Charge MapChargeToChangeOfChargesMessage(Charge charge)
        {
            if (charge == null) throw new ArgumentNullException(nameof(charge));
            var validChargeDetails = charge.ChargePeriodDetails
                .OrderBy(x => Math.Abs((x.StartTimeDate - DateTime.UtcNow).Ticks)).First();

            return new Domain.Charge
            {
                    Id = charge.ChargeId,
                    Type = (ChargeType)charge.ChargeType,
                    Name = validChargeDetails.Name, // Description to be Name
                    Description = validChargeDetails.Description, // LongDescription to be Description
                    StartDateTime = Instant.FromDateTimeUtc(validChargeDetails.StartTimeDate),
                    Owner = charge.Owner,
                    Resolution = (Resolution)charge.ResolutionType,
                    Status = (OperationType)1, // TODO: LRN Missing?
                    TaxIndicator = Convert.ToBoolean(charge.TaxIndicator),
                    TransparentInvoicing = Convert.ToBoolean(charge.TransparentInvoicing),
                    VatClassification = (VatClassification)validChargeDetails.VatClassification,
                    BusinessReasonCode = (BusinessReasonCode)1, // TODO: LRN Missing?,
                    ChargeOperationId = charge.ChargeId,
                    LastUpdatedBy = "MISSING?", // TODO: LRN MISSING?
                    EndDateTime = validChargeDetails.EndTimeDate != null ? Instant.FromDateTimeUtc(validChargeDetails.EndTimeDate.Value) : null,
                    Points = charge.ChargePrices.OrderByDescending(x => x.RowId).Select(x => new Point
                    {
                        Position = x.RowId,
                        Price = x.Price,
                        Time = Instant.FromUnixTimeTicks(x.Time),
                    }).ToList(),
            };
        }
    }
}
