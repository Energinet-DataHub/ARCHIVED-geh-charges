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
using System.Linq;
using GreenEnergyHub.Charges.Domain.ChangeOfCharges.Transaction;
using GreenEnergyHub.Charges.Domain.Common;
using MarketParticipant = GreenEnergyHub.Charges.Domain.Common.MarketParticipant;

namespace GreenEnergyHub.Charges.Application
{
    public static class ChargeCommandNullChecker
    {
        public static void ThrowExceptionIfRequiredPropertyIsNull(ChargeCommand chargeCommand)
        {
            if (chargeCommand == null) throw new ArgumentNullException(nameof(chargeCommand));
            CheckCharge(chargeCommand.Charge);
            CheckDocument(chargeCommand.Document);
            CheckEvent(chargeCommand.ChargeEvent);
        }

        private static void CheckCharge(ChargeNew charge)
        {
            if (charge == null) throw new ArgumentNullException(nameof(charge));

            if (string.IsNullOrWhiteSpace(charge.Type)) throw new ArgumentException(charge.Type);
            if (string.IsNullOrWhiteSpace(charge.Owner)) throw new ArgumentException(charge.Owner);
            if (string.IsNullOrWhiteSpace(charge.Id)) throw new ArgumentException(charge.Id);
            if (string.IsNullOrWhiteSpace(charge.Name)) throw new ArgumentException(charge.Name);
            if (string.IsNullOrWhiteSpace(charge.Vat)) throw new ArgumentException(charge.Vat);
            if (string.IsNullOrWhiteSpace(charge.Description)) throw new ArgumentException(charge.Description);
            if (string.IsNullOrWhiteSpace(charge.Resolution)) throw new ArgumentException(charge.Resolution);
        }

        private static void CheckEvent(ChargeEvent chargeEvent)
        {
            if (chargeEvent == null) throw new ArgumentNullException(nameof(chargeEvent));
            if (string.IsNullOrWhiteSpace(chargeEvent.Id)) throw new ArgumentException(chargeEvent.Id);
            if (string.IsNullOrWhiteSpace(chargeEvent.CorrelationId)) throw new ArgumentException(chargeEvent.CorrelationId);
            if (string.IsNullOrWhiteSpace(chargeEvent.LastUpdatedBy)) throw new ArgumentException(chargeEvent.LastUpdatedBy);
        }

        private static void CheckDocument(Document document)
        {
            if (document == null) throw new ArgumentNullException(nameof(document));
            if (string.IsNullOrWhiteSpace(document.Id)) throw new ArgumentException(document.Id);
            if (string.IsNullOrWhiteSpace(document.Type)) throw new ArgumentException(document.Id);
            CheckMarketDocumentMarketParticipant(document.Recipient);
            CheckMarketDocumentMarketParticipant(document.Sender);
        }

        private static void CheckMarketDocumentMarketParticipant(MarketParticipant marketParticipant)
        {
            if (marketParticipant == null) throw new ArgumentNullException(nameof(marketParticipant));
            if (string.IsNullOrWhiteSpace(marketParticipant.MRid)) throw new ArgumentException(marketParticipant.MRid);
        }
    }
}
