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
using GreenEnergyHub.Charges.Domain.Configuration;
using GreenEnergyHub.Charges.Domain.MarketParticipants;
using GreenEnergyHub.Charges.Infrastructure.Core.Cim.MarketActivityRecord;
using GreenEnergyHub.Charges.Infrastructure.Core.Cim.MarketDocument;
using GreenEnergyHub.Charges.MessageHub.Models.AvailableChargeLinkReceiptData;
using NodaTime;

namespace GreenEnergyHub.Charges.MessageHub.Infrastructure.Cim.Bundles.ChargeLinkReceipt
{
    public class ChargeLinkReceiptCimSerializer
        : CimSerializer<AvailableChargeLinkReceiptData>
    {
        public ChargeLinkReceiptCimSerializer(
            IHubSenderConfiguration hubSenderConfiguration,
            IClock clock,
            ICimIdProvider cimIdProvider)
            : base(hubSenderConfiguration, clock, cimIdProvider)
        {
        }

        public override IEnumerable<XElement> GetAdditionalDocumentFields(XNamespace cimNamespace, IEnumerable<AvailableChargeLinkReceiptData> records)
        {
            return new List<XElement>
            {
                // Due to the nature of the interface to the MessageHub and the use of MessageType in that
                // BusinessReasonCode, RecipientId, RecipientRole and ReceiptStatus will always be the same value
                // on all records in the list. We can simply take it from the first record.
                new XElement(
                    cimNamespace + CimChargeLinkReceiptConstants.ReceiptStatus,
                    ReceiptStatusMapper.Map(records.First().ReceiptStatus)),
            };
        }

        protected override XNamespace GetNamespace(IEnumerable<AvailableChargeLinkReceiptData> records)
        {
            return IsConfirmation(records) ?
                CimChargeLinkReceiptConstants.ConfirmNamespace : CimChargeLinkReceiptConstants.RejectNamespace;
        }

        protected override XNamespace GetSchemaLocation(IEnumerable<AvailableChargeLinkReceiptData> records)
        {
            return IsConfirmation(records) ?
                CimChargeLinkReceiptConstants.ConfirmSchemaLocation : CimChargeLinkReceiptConstants.RejectSchemaLocation;
        }

        protected override string GetRootElementName(IEnumerable<AvailableChargeLinkReceiptData> records)
        {
            return IsConfirmation(records) ?
                CimChargeLinkReceiptConstants.ConfirmRootElement : CimChargeLinkReceiptConstants.RejectRootElement;
        }

        protected override DocumentType GetDocumentType(IEnumerable<AvailableChargeLinkReceiptData> records)
        {
            return DocumentType.ChargeLinkReceipt;
        }

        protected override XElement GetActivityRecord(
            XNamespace cimNamespace,
            AvailableChargeLinkReceiptData receipt)
        {
            return new XElement(
                cimNamespace + CimMarketDocumentConstants.MarketActivityRecord,
                new XElement(
                    cimNamespace + CimChargeLinkReceiptConstants.MarketActivityRecordId,
                    CimIdProvider.GetUniqueId()),
                new XElement(
                    cimNamespace + CimChargeLinkReceiptConstants.OriginalOperationId,
                    receipt.OriginalOperationId),
                new XElement(
                    cimNamespace + CimChargeLinkReceiptConstants.MeteringPointId,
                    new XAttribute(
                        CimMarketDocumentConstants.CodingScheme,
                        CodingSchemeMapper.Map(CodingScheme.GS1)),
                    receipt.MeteringPointId),
                GetReasonCodes(cimNamespace, receipt));
        }

        private IEnumerable<XElement> GetReasonCodes(
            XNamespace cimNamespace,
            AvailableChargeLinkReceiptData receipt)
        {
            var result = new List<XElement>();
            if (receipt.ReceiptStatus != ReceiptStatus.Rejected) return result;

            result.AddRange(receipt.ReasonCodes.Select(reasonCode => GetReasonCode(cimNamespace, reasonCode)));

            return result;
        }

        private XElement GetReasonCode(
            XNamespace cimNamespace,
            AvailableChargeLinkReceiptDataReasonCode reasonCode)
        {
            return new XElement(
                cimNamespace + CimChargeLinkReceiptConstants.ReasonElement,
                new XElement(cimNamespace + CimChargeLinkReceiptConstants.ReasonCode, ReasonCodeMapper.Map(reasonCode.ReasonCode)),
                CimHelper.GetElementIfNeeded(
                    cimNamespace,
                    string.IsNullOrWhiteSpace(reasonCode.Text),
                    CimChargeLinkReceiptConstants.ReasonText,
                    () => reasonCode.Text));
        }

        private bool IsConfirmation(IEnumerable<AvailableChargeLinkReceiptData> receipts)
        {
            // Due to the nature of the interface to the MessageHub and the use of MessageType in that
            // BusinessReasonCode, RecipientId, RecipientRole and ReceiptStatus will always be the same value
            // on all records in the list. We can simply take it from the first record.
            return receipts.First().ReceiptStatus == ReceiptStatus.Confirmed;
        }
    }
}
