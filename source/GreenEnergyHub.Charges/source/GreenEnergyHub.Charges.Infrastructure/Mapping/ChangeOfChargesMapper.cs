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
using ChargeType = GreenEnergyHub.Charges.Infrastructure.Context.Model.ChargeType;
using MarketParticipant = GreenEnergyHub.Charges.Infrastructure.Context.Model.MarketParticipant;

namespace GreenEnergyHub.Charges.Infrastructure.Mapping
{
    public static class ChangeOfChargesMapper
    {
        public static Charge MapChangeOfChargesTransactionToCharge(
            [NotNull]ChargeCommand changeOfChargeTransaction,
            ChargeType chargeType,
            MarketParticipant chargeTypeOwnerMRid,
            ResolutionType resolutionType,
            VatPayerType vatPayerType)
        {
            if (changeOfChargeTransaction == null) throw new ArgumentNullException(nameof(changeOfChargeTransaction));
            if (string.IsNullOrWhiteSpace(changeOfChargeTransaction.ChargeTypeMRid)) throw new ArgumentException($"{nameof(changeOfChargeTransaction.ChargeTypeMRid)} must have value");
            if (string.IsNullOrWhiteSpace(changeOfChargeTransaction.CorrelationId)) throw new ArgumentException($"{nameof(changeOfChargeTransaction.CorrelationId)} must have value");
            if (string.IsNullOrWhiteSpace(changeOfChargeTransaction.LastUpdatedBy)) throw new ArgumentException($"{nameof(changeOfChargeTransaction.LastUpdatedBy)} must have value");
            if (changeOfChargeTransaction.MktActivityRecord?.ChargeType == null) throw new ArgumentException($"{nameof(changeOfChargeTransaction.MktActivityRecord.ChargeType)} can't be null");
            if (string.IsNullOrWhiteSpace(changeOfChargeTransaction.MktActivityRecord.ChargeType.Name)) throw new ArgumentException($"{nameof(changeOfChargeTransaction.MktActivityRecord.ChargeType.Name)} must have value");
            if (string.IsNullOrWhiteSpace(changeOfChargeTransaction.MktActivityRecord.ChargeType.Description)) throw new ArgumentException($"{nameof(changeOfChargeTransaction.MktActivityRecord.ChargeType.Description)} must have value");
            if (changeOfChargeTransaction.Period == null) throw new ArgumentException($"{nameof(changeOfChargeTransaction.Period)} can't be null");
            if (changeOfChargeTransaction.Period.Points == null) throw new ArgumentException($"{nameof(changeOfChargeTransaction.Period.Points)} can't be null");

            var charge = new Charge
            {
                ChargeType = chargeType,
                ChargeTypeOwner = chargeTypeOwnerMRid,
                Description = changeOfChargeTransaction.MktActivityRecord.ChargeType.Description,
                LastUpdatedBy = changeOfChargeTransaction.LastUpdatedBy,
                LastUpdatedByCorrelationId = changeOfChargeTransaction.CorrelationId,
                LastUpdatedByTransactionId = changeOfChargeTransaction.MRid,
                Name = changeOfChargeTransaction.MktActivityRecord.ChargeType.Name,
                RequestDateTime = changeOfChargeTransaction.RequestDate.ToUnixTimeTicks(),
                ResolutionType = resolutionType,
                StartDate = changeOfChargeTransaction.MktActivityRecord.ValidityStartDate.ToUnixTimeTicks(),
                EndDate = changeOfChargeTransaction.MktActivityRecord.ValidityEndDate?.ToUnixTimeTicks(),
                Status = (int)changeOfChargeTransaction.MktActivityRecord.Status,
                TaxIndicator = changeOfChargeTransaction.MktActivityRecord.ChargeType.TaxIndicator,
                TransparentInvoicing = changeOfChargeTransaction.MktActivityRecord.ChargeType.TransparentInvoicing,
                VatPayer = vatPayerType,
                MRid = changeOfChargeTransaction.ChargeTypeMRid,
                Currency = "DKK",
            };

            foreach (var point in changeOfChargeTransaction.Period.Points)
            {
                var newChargePrice = new ChargePrice
                {
                    Time = point.Time.ToUnixTimeTicks(),
                    Amount = point.PriceAmount,
                    LastUpdatedByCorrelationId = changeOfChargeTransaction.CorrelationId,
                    LastUpdatedByTransactionId = changeOfChargeTransaction.MRid,
                    LastUpdatedBy = changeOfChargeTransaction.LastUpdatedBy,
                    RequestDateTime = changeOfChargeTransaction.RequestDate.ToUnixTimeTicks(),
                };

                charge.ChargePrices.Add(newChargePrice);
            }

            return charge;
        }
    }
}
