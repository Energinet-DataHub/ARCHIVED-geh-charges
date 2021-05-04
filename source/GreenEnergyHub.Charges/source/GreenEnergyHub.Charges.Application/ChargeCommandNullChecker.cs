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
            CheckChargeCommand(chargeCommand);
            CheckChargeCommandPeriod(chargeCommand.Period);
            CheckChargeCommandMarketDocument(chargeCommand.MarketDocument);
            CheckChargeCommandMktActivityRecord(chargeCommand.MktActivityRecord);
        }

        private static void CheckChargeCommand(ChargeCommand chargeCommand)
        {
            if (chargeCommand == null) throw new ArgumentNullException(nameof(chargeCommand));
            if (chargeCommand.CorrelationId == null) throw new ArgumentNullException(chargeCommand.CorrelationId);
            if (chargeCommand.LastUpdatedBy == null) throw new ArgumentNullException(chargeCommand.LastUpdatedBy);
            if (chargeCommand.Type == null) throw new ArgumentNullException(chargeCommand.Type);
            if (chargeCommand.ChargeTypeOwnerMRid == null) throw new ArgumentNullException(chargeCommand.ChargeTypeOwnerMRid);
        }

        private static void CheckChargeCommandMktActivityRecord(MktActivityRecord mktActivityRecord)
        {
            if (mktActivityRecord == null) throw new ArgumentNullException(nameof(mktActivityRecord));
            if (mktActivityRecord.MRid == null) throw new ArgumentNullException(mktActivityRecord.MRid);
            CheckMktActivityRecordChargeType(mktActivityRecord.ChargeType);
        }

        private static void CheckMktActivityRecordChargeType(ChargeType chargeType)
        {
            if (chargeType == null) throw new ArgumentNullException(nameof(chargeType));
            if (chargeType.Name == null) throw new ArgumentNullException(chargeType.Name);
            if (chargeType.VatPayer == null) throw new ArgumentNullException(chargeType.VatPayer);
        }

        private static void CheckChargeCommandMarketDocument(MarketDocument marketDocument)
        {
            if (marketDocument == null) throw new ArgumentNullException(nameof(marketDocument));
            if (marketDocument.MRid == null) throw new ArgumentNullException(marketDocument.MRid);
            CheckMarketDocumentMarketParticipant(marketDocument.ReceiverMarketParticipant);
            CheckMarketDocumentMarketParticipant(marketDocument.SenderMarketParticipant);
        }

        private static void CheckMarketDocumentMarketParticipant(MarketParticipant marketParticipant)
        {
            if (marketParticipant == null) throw new ArgumentNullException(nameof(marketParticipant));
            if (marketParticipant.MRid == null) throw new ArgumentNullException(marketParticipant.MRid);
        }

        private static void CheckChargeCommandPeriod(ChargeTypePeriod chargeTypePeriod)
        {
            if (chargeTypePeriod == null) throw new ArgumentNullException(nameof(chargeTypePeriod));
            if (chargeTypePeriod.Resolution == null) throw new ArgumentNullException(chargeTypePeriod.Resolution);
        }
    }
}
