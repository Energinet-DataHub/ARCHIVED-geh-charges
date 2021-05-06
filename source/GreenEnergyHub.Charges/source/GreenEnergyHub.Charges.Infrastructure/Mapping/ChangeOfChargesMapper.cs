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
using GreenEnergyHub.Charges.Domain.Common;
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
                Charge = new ChargeDto
                {
                    Id = charge.MRid,
                    Type = Enum.Parse<GreenEnergyHub.Charges.Domain.ChangeOfCharges.Transaction.ChargeType>(charge.ChargeType.Code!),
                    Name = charge.Name, // Description to be Name
                    Description = charge.Description, // LongDescription to be Description
                    StartDateTime = Instant.FromUnixTimeTicks(charge.StartDate),
                    EndDateTime = charge.EndDate != null ? Instant.FromUnixTimeTicks(charge.EndDate.Value) : (Instant?)null,
                    Vat = Enum.Parse<Vat>(charge.VatPayer.Name),
                    TransparentInvoicing = charge.TransparentInvoicing,
                    Tax = charge.TaxIndicator,
                    Owner = charge.ChargeTypeOwner.MRid,
                    Resolution = Enum.Parse<Resolution>(charge.ResolutionType.Name!),
                },
                ChargeEvent = new ChargeEvent
                {
                    Id = charge.LastUpdatedByTransactionId,
                    Status = (ChargeEventFunction)charge.Status,
                },
                Document = new Document
                {
                    RequestDate = Instant.FromUnixTimeTicks(charge.RequestDateTime),
                },
            };
        }
    }
}
