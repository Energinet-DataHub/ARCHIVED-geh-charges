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
            if (chargeCommand == null)
            {
                throw new ArgumentNullException(nameof(chargeCommand));
            }

            var charge = new Charge
            {
                ChargeType = chargeType,
                ChargeTypeOwner = chargeTypeOwnerMRid,
                Description = chargeCommand.Charge.Name,
                LastUpdatedBy = chargeCommand.ChargeEvent.LastUpdatedBy,
                LastUpdatedByCorrelationId = chargeCommand.Document.CorrelationId,
                LastUpdatedByTransactionId = chargeCommand.ChargeEvent.Id,
                Name = chargeCommand.Charge.Name,
                RequestDateTime = chargeCommand.Document.RequestDate.ToUnixTimeTicks(),
                ResolutionType = resolutionType,
                StartDate = chargeCommand.Charge.StartDateTime.ToUnixTimeTicks(),
                EndDate = chargeCommand.Charge.EndDateTime?.ToUnixTimeTicks(),
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
                    Amount = point.Price,
                    LastUpdatedByCorrelationId = chargeCommand.Document.CorrelationId,
                    LastUpdatedByTransactionId = chargeCommand.ChargeEvent.Id,
                    LastUpdatedBy = chargeCommand.ChargeEvent.LastUpdatedBy,
                    RequestDateTime = chargeCommand.Document.RequestDate.ToUnixTimeTicks(),
                };

                charge.ChargePrices.Add(newChargePrice);
            }

            return charge;
        }

        public static Domain.Charge MapChargeToChangeOfChargesMessage(Charge charge)
        {
            if (charge == null)
            {
                throw new ArgumentNullException(nameof(charge));
            }

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
