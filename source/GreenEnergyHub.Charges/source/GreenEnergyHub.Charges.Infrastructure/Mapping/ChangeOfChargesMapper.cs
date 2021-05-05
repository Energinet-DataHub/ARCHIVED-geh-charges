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
                Description = chargeCommand.ChargeNew.Name,
                LastUpdatedBy = chargeCommand.ChargeEvent.LastUpdatedBy,
                LastUpdatedByCorrelationId = chargeCommand.ChargeEvent.CorrelationId,
                LastUpdatedByTransactionId = chargeCommand.ChargeEvent.Id,
                Name = chargeCommand.ChargeNew.Name,
                RequestDateTime = chargeCommand.ChargeEvent.RequestDate.ToUnixTimeTicks(),
                ResolutionType = resolutionType,
                StartDate = chargeCommand.ChargeEvent.StartDateTime.ToUnixTimeTicks(),
                EndDate = chargeCommand.ChargeEvent.EndDateTime?.ToUnixTimeTicks(),
                Status = (byte)chargeCommand.ChargeEvent.Status,
                TaxIndicator = chargeCommand.ChargeNew.Tax,
                TransparentInvoicing = chargeCommand.ChargeNew.TransparentInvoicing,
                VatPayer = vatPayerType,
                MRid = chargeCommand.ChargeNew.Id,
                Currency = "DKK",
            };

            foreach (var point in chargeCommand.ChargeNew.Points)
            {
                var newChargePrice = new ChargePrice
                {
                    Time = point.Time.ToUnixTimeTicks(),
                    Amount = point.PriceAmount,
                    LastUpdatedByCorrelationId = chargeCommand.ChargeEvent.CorrelationId,
                    LastUpdatedByTransactionId = chargeCommand.ChargeEvent.Id,
                    LastUpdatedBy = chargeCommand.ChargeEvent.LastUpdatedBy,
                    RequestDateTime = chargeCommand.ChargeEvent.RequestDate.ToUnixTimeTicks(),
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
                ChargeNew = new ChargeNew
                {
                    Id = charge.MRid,
                    Type = Enum.Parse<GreenEnergyHub.Charges.Domain.ChangeOfCharges.Transaction.ChargeType>(charge.ChargeType.Code!),
                    Name = charge.Name, // Description to be Name
                    Description = charge.Description, // LongDescription to be Description
                    Vat = Enum.Parse<Vat>(charge.VatPayer.Name),
                    TransparentInvoicing = charge.TransparentInvoicing,
                    Tax = charge.TaxIndicator,
                    Owner = charge.ChargeTypeOwner.MRid,
                    Resolution = Enum.Parse<Resolution>(charge.ResolutionType.Name!),
                },
                ChargeEvent = new ChargeEvent
                {
                    Id = charge.LastUpdatedByTransactionId,
                    StartDateTime = Instant.FromUnixTimeTicks(charge.StartDate),
                    EndDateTime = charge.EndDate != null ? Instant.FromUnixTimeTicks(charge.EndDate.Value) : null,
                    Status = (ChargeEventFunction)charge.Status,
                    RequestDate = Instant.FromUnixTimeTicks(charge.RequestDateTime),
                },
            };
        }
    }
}
