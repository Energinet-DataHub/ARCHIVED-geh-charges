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
using MarketParticipant = GreenEnergyHub.Charges.Domain.Common.MarketParticipant;

namespace GreenEnergyHub.Charges.Application
{
    public static class ChargeCommandNullChecker
    {
        public static void ThrowExceptionIfRequiredPropertyIsNull(ChargeCommand chargeCommand)
        {
            if (chargeCommand == null) throw new ArgumentNullException(nameof(chargeCommand));
            if (string.IsNullOrWhiteSpace(chargeCommand.CorrelationId)) throw new ArgumentException(chargeCommand.CorrelationId);

            CheckDocument(chargeCommand.Document);
            CheckEvent(chargeCommand.ChargeOperation);
        }

        private static void CheckEvent(ChargeOperation chargeOperation)
        {
            if (chargeOperation == null) throw new ArgumentNullException(nameof(chargeOperation));
            if (string.IsNullOrWhiteSpace(chargeOperation.Id)) throw new ArgumentException(chargeOperation.Id);
            if (string.IsNullOrWhiteSpace(chargeOperation.ChargeOwner)) throw new ArgumentException(chargeOperation.ChargeOwner);
            if (string.IsNullOrWhiteSpace(chargeOperation.ChargeId)) throw new ArgumentException(chargeOperation.ChargeId);
            if (string.IsNullOrWhiteSpace(chargeOperation.ChargeName)) throw new ArgumentException(chargeOperation.ChargeName);
            if (string.IsNullOrWhiteSpace(chargeOperation.ChargeDescription)) throw new ArgumentException(chargeOperation.ChargeDescription);
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
