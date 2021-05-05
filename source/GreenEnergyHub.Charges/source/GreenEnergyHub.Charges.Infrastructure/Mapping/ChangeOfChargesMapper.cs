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
            [NotNull]ChargeCommand chargeCommand,
            ChargeType chargeType,
            MarketParticipant chargeTypeOwnerMRid,
            ResolutionType resolutionType,
            VatPayerType vatPayerType)
        {
            if (chargeCommand == null) throw new ArgumentNullException(nameof(chargeCommand));

            var charge = new Charge
            {
                ChargeType = chargeType,
                ChargeTypeOwner = chargeTypeOwnerMRid,
                Description = chargeCommand.Charge.Description,
                LastUpdatedBy = chargeCommand.ChargeEvent.LastUpdatedBy,
                LastUpdatedByCorrelationId = chargeCommand.ChargeEvent.CorrelationId,
                LastUpdatedByTransactionId = chargeCommand.ChargeEvent.Id,
                Name = chargeCommand.Charge.Description,
                RequestDateTime = chargeCommand.Charge.RequestDate.ToUnixTimeTicks(),
                ResolutionType = resolutionType,
                StartDate = chargeCommand.ChargeEvent.StartDateTime.ToUnixTimeTicks(),
                EndDate = chargeCommand.ChargeEvent.EndDateTime?.ToUnixTimeTicks(),
                Status = (byte)chargeCommand.ChargeEvent.Status,
                TaxIndicator = chargeCommand.Charge.Tax,
                TransparentInvoicing = chargeCommand.Charge.TransparentInvoicing,
                VatPayer = vatPayerType,
                MRid = chargeCommand.Charge.Id,
                Currency = "DKK",
            };

            foreach (var point in chargeCommand.Charge.Points)
            {
                var newChargePrice = new ChargePrice
                {
                    Time = point.Time.ToUnixTimeTicks(),
                    Amount = point.PriceAmount,
                    LastUpdatedByCorrelationId = chargeCommand.ChargeEvent.CorrelationId,
                    LastUpdatedByTransactionId = chargeCommand.ChargeEvent.Id,
                    LastUpdatedBy = chargeCommand.ChargeEvent.LastUpdatedBy,
                    RequestDateTime = chargeCommand.Charge.RequestDate.ToUnixTimeTicks(),
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
                Charge = new ChargeNew
                {
                    Description = charge.Name,
                    Id = charge.MRid,
                    Owner = charge.ChargeTypeOwner.MRid,
                    Tax = charge.TaxIndicator,
                    //Type = charge.ChargeType.Name,
                    LongDescription = charge.Description,
                    Vat = charge.VatPayer.Name,
                    RequestDate = charge.RequestDateTime,
                    TransparentInvoicing = charge.TransparentInvoicing,
                    Resolution = charge.ResolutionType.Name!
                },
                Document = new Document
                {
                    Id = charge.
                },

                // ChargeTypeMRid = charge.MRid,
                // ChargeEvent = new ChargeEvent
                // {
                //     Status = (ChargeEventFuction)charge.Status,
                //     ChargeType = new Domain.ChangeOfCharges.Transaction.ChargeType()
                //     {
                //         Name = charge.Name,
                //         TaxIndicator = charge.TaxIndicator,
                //         VatPayer = charge.VatPayer.Name,
                //         Description = charge.Description,
                //         TransparentInvoicing = charge.TransparentInvoicing,
                //     },
                //     StartDateTime = Instant.FromUnixTimeTicks(charge.StartDate),
                //     EndDateTime = charge.EndDate != null ? Instant.FromUnixTimeTicks(charge.EndDate.Value) : null as Instant?,
                // },
                // RequestDate = Instant.FromUnixTimeTicks(charge.RequestDateTime),
                // LastUpdatedBy = charge.LastUpdatedBy,
                // CorrelationId = charge.LastUpdatedByCorrelationId,
                // ChargeTypeOwnerMRid = charge.ChargeTypeOwner.MRid,
            };
        }
    }
}
