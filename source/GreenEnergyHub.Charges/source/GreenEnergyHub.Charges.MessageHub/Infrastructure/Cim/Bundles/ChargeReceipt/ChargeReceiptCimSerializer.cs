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

using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using GreenEnergyHub.Charges.Domain.MarketParticipants;
using GreenEnergyHub.Charges.Infrastructure.Core.Cim.MarketActivityRecord;
using GreenEnergyHub.Charges.Infrastructure.Core.Cim.MarketDocument;
using GreenEnergyHub.Charges.MessageHub.Models.AvailableChargeReceiptData;
using GreenEnergyHub.Charges.MessageHub.Models.Configuration;
using NodaTime;

namespace GreenEnergyHub.Charges.MessageHub.Infrastructure.Cim.Bundles.ChargeReceipt
{
    public class ChargeReceiptCimSerializer
        : CimSerializer<AvailableChargeReceiptData>
    {
        public ChargeReceiptCimSerializer(
            IHubSenderConfiguration hubSenderConfiguration,
            IClock clock,
            ICimIdProvider cimIdProvider)
            : base(hubSenderConfiguration, clock, cimIdProvider)
        {
        }

        public override IEnumerable<XElement> GetAdditionalDocumentFields(XNamespace cimNamespace, IEnumerable<AvailableChargeReceiptData> records)
        {
            return new List<XElement>
            {
                // Due to the nature of the interface to the MessageHub and the use of MessageType in that
                // BusinessReasonCode, RecipientId, RecipientRole and ReceiptStatus will always be the same value
                // on all records in the list. We can simply take it from the first record.
                new XElement(
                    cimNamespace + CimChargeReceiptConstants.ReceiptStatus,
                    ReceiptStatusMapper.Map(records.First().ReceiptStatus)),
            };
        }

        protected override XNamespace GetNamespace(IEnumerable<AvailableChargeReceiptData> records)
        {
            return IsConfirmation(records) ?
                CimChargeReceiptConstants.ConfirmNamespace : CimChargeReceiptConstants.RejectNamespace;
        }

        protected override XNamespace GetSchemaLocation(IEnumerable<AvailableChargeReceiptData> records)
        {
            return IsConfirmation(records) ?
                CimChargeReceiptConstants.ConfirmSchemaLocation : CimChargeReceiptConstants.RejectSchemaLocation;
        }

        protected override string GetRootElementName(IEnumerable<AvailableChargeReceiptData> records)
        {
            return IsConfirmation(records) ?
                CimChargeReceiptConstants.ConfirmRootElement : CimChargeReceiptConstants.RejectRootElement;
        }

        protected override DocumentType GetDocumentType(IEnumerable<AvailableChargeReceiptData> records)
        {
            return DocumentType.ChargeReceipt;
        }

        protected override XElement GetActivityRecord(
            XNamespace cimNamespace,
            AvailableChargeReceiptData receipt)
        {
            return new XElement(
                cimNamespace + CimMarketDocumentConstants.MarketActivityRecord,
                new XElement(
                    cimNamespace + CimChargeReceiptConstants.Id,
                    CimIdProvider.GetUniqueId()),
                new XElement(
                    cimNamespace + CimChargeReceiptConstants.OriginalOperationId,
                    receipt.OriginalOperationId),
                GetReasonCodes(cimNamespace, receipt));
        }

        private IEnumerable<XElement> GetReasonCodes(
            XNamespace cimNamespace,
            AvailableChargeReceiptData receipt)
        {
            var result = new List<XElement>();
            if (receipt.ReceiptStatus != ReceiptStatus.Rejected) return result;

            result.AddRange(receipt.ReasonCodes.Select(reasonCode => GetReasonCode(cimNamespace, reasonCode)));

            return result;
        }

        private XElement GetReasonCode(
            XNamespace cimNamespace,
            AvailableChargeReceiptDataReasonCode reasonCode)
        {
            return new XElement(
                cimNamespace + CimChargeReceiptConstants.ReasonElement,
                new XElement(cimNamespace + CimChargeReceiptConstants.ReasonCode, ReasonCodeMapper.Map(reasonCode.ReasonCode)),
                CimHelper.GetElementIfNeeded(
                    cimNamespace,
                    string.IsNullOrWhiteSpace(reasonCode.Text),
                    CimChargeReceiptConstants.ReasonText,
                    () => reasonCode.Text));
        }

        private bool IsConfirmation(IEnumerable<AvailableChargeReceiptData> receipts)
        {
            // Due to the nature of the interface to the MessageHub and the use of MessageType in that
            // BusinessReasonCode, RecipientId, RecipientRole and ReceiptStatus will always be the same value
            // on all records in the list. We can simply take it from the first record.
            return receipts.First().ReceiptStatus == ReceiptStatus.Confirmed;
        }
    }
}
