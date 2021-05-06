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
            if (string.IsNullOrWhiteSpace(chargeCommand.CorrelationId)) throw new ArgumentException(chargeCommand.CorrelationId);

            CheckCharge(chargeCommand.ChargeOperation);
            CheckDocument(chargeCommand.Document);
            CheckEvent(chargeCommand.ChargeEvent);
        }

        private static void CheckCharge(ChargeOperation chargeOperation)
        {
            if (chargeOperation == null) throw new ArgumentNullException(nameof(chargeOperation));

            if (string.IsNullOrWhiteSpace(chargeOperation.Owner)) throw new ArgumentException(chargeOperation.Owner);
            if (string.IsNullOrWhiteSpace(chargeOperation.Id)) throw new ArgumentException(chargeOperation.Id);
            if (string.IsNullOrWhiteSpace(chargeOperation.Name)) throw new ArgumentException(chargeOperation.Name);
            if (string.IsNullOrWhiteSpace(chargeOperation.Description)) throw new ArgumentException(chargeOperation.Description);
        }

        private static void CheckEvent(ChargeEvent chargeEvent)
        {
            if (chargeEvent == null) throw new ArgumentNullException(nameof(chargeEvent));
            if (string.IsNullOrWhiteSpace(chargeEvent.Id)) throw new ArgumentException(chargeEvent.Id);
            if (string.IsNullOrWhiteSpace(chargeEvent.LastUpdatedBy)) throw new ArgumentException(chargeEvent.LastUpdatedBy);
        }

        private static void CheckDocument(Document document)
        {
            if (document == null) throw new ArgumentNullException(nameof(document));
            if (string.IsNullOrWhiteSpace(document.Id)) throw new ArgumentException(document.Id);
            if (string.IsNullOrWhiteSpace(document.Type)) throw new ArgumentException(document.Type);
            CheckMarketParticipant(document.Recipient);
            CheckMarketParticipant(document.Sender);
        }

        private static void CheckMarketParticipant(MarketParticipant marketParticipant)
        {
            if (marketParticipant == null) throw new ArgumentNullException(nameof(marketParticipant));
            if (string.IsNullOrWhiteSpace(marketParticipant.MRid)) throw new ArgumentException(marketParticipant.MRid);
        }
    }
}
