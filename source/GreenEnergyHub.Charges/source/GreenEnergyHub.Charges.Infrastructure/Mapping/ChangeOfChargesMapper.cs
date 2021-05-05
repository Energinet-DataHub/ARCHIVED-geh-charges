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
using GreenEnergyHub.Charges.Domain.ChangeOfCharges.Transaction;
using GreenEnergyHub.Charges.Infrastructure.Context.Model;
using JetBrains.Annotations;
using NodaTime;
using ChargeType = GreenEnergyHub.Charges.Infrastructure.Context.Model.ChargeType;
using MarketParticipant = GreenEnergyHub.Charges.Infrastructure.Context.Model.MarketParticipant;

namespace GreenEnergyHub.Charges.Infrastructure.Mapping
{
    public static class ChangeOfChargesMapper
    {
        public static Charge MapChangeOfChargesTransactionToCharge(
            [NotNull]Domain.Charge chargeModel,
            ChargeType chargeType,
            MarketParticipant chargeTypeOwnerMRid,
            ResolutionType resolutionType,
            VatPayerType vatPayerType)
        {
            if (chargeModel == null) throw new ArgumentNullException(nameof(chargeModel));

            var charge = new Charge
            {
                ChargeType = chargeType,
                ChargeTypeOwner = chargeTypeOwnerMRid,
                Description = chargeModel.MktActivityRecord.ChargeType.Description,
                LastUpdatedBy = chargeModel.LastUpdatedBy,
                LastUpdatedByCorrelationId = chargeModel.CorrelationId,
                LastUpdatedByTransactionId = chargeModel.MktActivityRecord.MRid,
                Name = chargeModel.MktActivityRecord.ChargeType.Name,
                RequestDateTime = chargeModel.RequestDate.ToUnixTimeTicks(),
                ResolutionType = resolutionType,
                StartDate = chargeModel.MktActivityRecord.ValidityStartDate.ToUnixTimeTicks(),
                EndDate = chargeModel.MktActivityRecord.ValidityEndDate?.ToUnixTimeTicks(),
                Status = (byte)chargeModel.MktActivityRecord.Status,
                TaxIndicator = chargeModel.MktActivityRecord.ChargeType.TaxIndicator,
                TransparentInvoicing = chargeModel.MktActivityRecord.ChargeType.TransparentInvoicing,
                VatPayer = vatPayerType,
                MRid = chargeModel.ChargeTypeMRid,
                Currency = "DKK",
            };

            foreach (var point in chargeModel.Period.Points)
            {
                var newChargePrice = new ChargePrice
                {
                    Time = point.Time.ToUnixTimeTicks(),
                    Amount = point.PriceAmount,
                    LastUpdatedByCorrelationId = chargeModel.CorrelationId,
                    LastUpdatedByTransactionId = chargeModel.MktActivityRecord.MRid,
                    LastUpdatedBy = chargeModel.LastUpdatedBy,
                    RequestDateTime = chargeModel.RequestDate.ToUnixTimeTicks(),
                };

                charge.ChargePrices.Add(newChargePrice);
            }

            return charge;
        }

        public static Domain.Charge MapChargeToChangeOfChargesMessage(Charge charge)
        {
            if (charge == null) throw new ArgumentNullException(nameof(charge));

            return new Domain.Charge
            {
                ChargeTypeMRid = charge.MRid,
                MktActivityRecord = new MktActivityRecord
                {
                    Status = (MktActivityRecordStatus)charge.Status,
                    ChargeType = new Domain.ChangeOfCharges.Transaction.ChargeType()
                    {
                        Name = charge.Name,
                        TaxIndicator = charge.TaxIndicator,
                        VatPayer = charge.VatPayer.Name,
                        Description = charge.Description,
                        TransparentInvoicing = charge.TransparentInvoicing,
                    },
                    ValidityStartDate = Instant.FromUnixTimeTicks(charge.StartDate),
                    ValidityEndDate = charge.EndDate != null ? Instant.FromUnixTimeTicks(charge.EndDate.Value) : null,
                },
                RequestDate = Instant.FromUnixTimeTicks(charge.RequestDateTime),
                LastUpdatedBy = charge.LastUpdatedBy,
                CorrelationId = charge.LastUpdatedByCorrelationId,
                ChargeTypeOwnerMRid = charge.ChargeTypeOwner.MRid,
            };
        }
    }
}
