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
            CheckChargeCommand(chargeCommand);
            CheckChargeCommandPeriod(chargeCommand.Period);
            CheckChargeCommandMarketDocument(chargeCommand.Document);
            CheckChargeCommandMktActivityRecord(chargeCommand.ChargeEvent);
        }

        private static void CheckChargeCommand(ChargeCommand chargeCommand)
        {
            if (chargeCommand == null) throw new ArgumentNullException(nameof(chargeCommand));
            if (string.IsNullOrWhiteSpace(chargeCommand.CorrelationId)) throw new ArgumentException(chargeCommand.CorrelationId);
            if (string.IsNullOrWhiteSpace(chargeCommand.LastUpdatedBy)) throw new ArgumentException(chargeCommand.LastUpdatedBy);
            if (string.IsNullOrWhiteSpace(chargeCommand.Type)) throw new ArgumentException(chargeCommand.Type);
            if (string.IsNullOrWhiteSpace(chargeCommand.ChargeTypeOwnerMRid)) throw new ArgumentException(chargeCommand.ChargeTypeOwnerMRid);
            if (string.IsNullOrWhiteSpace(chargeCommand.ChargeTypeMRid)) throw new ArgumentException(chargeCommand.ChargeTypeOwnerMRid);
        }

        private static void CheckChargeCommandMktActivityRecord(ChargeEvent chargeEvent)
        {
            if (chargeEvent == null) throw new ArgumentNullException(nameof(chargeEvent));
            if (string.IsNullOrWhiteSpace(chargeEvent.Id)) throw new ArgumentException(chargeEvent.Id);
            CheckMktActivityRecordChargeType(chargeEvent.ChargeType);
        }

        private static void CheckMktActivityRecordChargeType(ChargeType chargeType)
        {
            if (chargeType == null) throw new ArgumentNullException(nameof(chargeType));
            if (string.IsNullOrWhiteSpace(chargeType.Name)) throw new ArgumentException(chargeType.Name);
            if (string.IsNullOrWhiteSpace(chargeType.VatPayer)) throw new ArgumentException(chargeType.VatPayer);
            if (string.IsNullOrWhiteSpace(chargeType.Description)) throw new ArgumentException(chargeType.Description);
        }

        private static void CheckChargeCommandMarketDocument(Document document)
        {
            if (document == null) throw new ArgumentNullException(nameof(document));
            if (string.IsNullOrWhiteSpace(document.Id)) throw new ArgumentException(document.Id);
            CheckMarketDocumentMarketParticipant(document.Recipient);
            CheckMarketDocumentMarketParticipant(document.Sender);
        }

        private static void CheckMarketDocumentMarketParticipant(MarketParticipant marketParticipant)
        {
            if (marketParticipant == null) throw new ArgumentNullException(nameof(marketParticipant));
            if (string.IsNullOrWhiteSpace(marketParticipant.MRid)) throw new ArgumentException(marketParticipant.MRid);
        }

        private static void CheckChargeCommandPeriod(ChargeTypePeriod chargeTypePeriod)
        {
            if (chargeTypePeriod == null) throw new ArgumentNullException(nameof(chargeTypePeriod));
            if (string.IsNullOrWhiteSpace(chargeTypePeriod.Resolution)) throw new ArgumentException(chargeTypePeriod.Resolution);
        }
    }
}
