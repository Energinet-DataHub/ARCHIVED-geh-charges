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
using GreenEnergyHub.Charges.Domain.Dtos.SharedDtos;
using GreenEnergyHub.Charges.Infrastructure.Core.Cim.MarketActivityRecord;
using GreenEnergyHub.Charges.Infrastructure.Core.Cim.MarketDocument;
using GreenEnergyHub.Charges.MessageHub.Models.AvailableChargeReceiptData;
using GreenEnergyHub.Charges.MessageHub.Models.AvailableData;
using NodaTime;

namespace GreenEnergyHub.Charges.MessageHub.Infrastructure.Cim.Bundles.ChargeReceipt
{
    public class ChargeReceiptCimXmlSerializer
        : CimXmlSerializer<AvailableChargeReceiptData>
    {
        public ChargeReceiptCimXmlSerializer(
            IClock clock,
            ICimIdProvider cimIdProvider)
            : base(clock, cimIdProvider)
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
            return IsConfirmation(records) ?
                DocumentType.ConfirmRequestChangeOfPriceList : DocumentType.RejectRequestChangeOfPriceList;
        }

        protected override XElement GetActivityRecord(XNamespace cimNamespace, AvailableChargeReceiptData record)
        {
            return new XElement(
                cimNamespace + CimMarketDocumentConstants.MarketActivityRecord,
                new XElement(
                    cimNamespace + CimChargeReceiptConstants.Id,
                    CimIdProvider.GetUniqueId()),
                new XElement(
                    cimNamespace + CimChargeReceiptConstants.OriginalOperationId,
                    record.OriginalOperationId),
                GetReasonCodes(cimNamespace, record));
        }

        private IEnumerable<XElement> GetReasonCodes(XNamespace cimNamespace, AvailableChargeReceiptData receipt)
        {
            var result = new List<XElement>();
            if (receipt.ReceiptStatus != ReceiptStatus.Rejected) return result;

            result.AddRange(receipt.ValidationErrors
                .Select(validationError => GetReasonCode(cimNamespace, validationError)));

            return result;
        }

        private XElement GetReasonCode(XNamespace cimNamespace, AvailableReceiptValidationError validationError)
        {
            return new XElement(
                cimNamespace + CimChargeReceiptConstants.ReasonElement,
                new XElement(cimNamespace + CimChargeReceiptConstants.ReasonCode, ReasonCodeMapper.Map(validationError.ReasonCode)),
                CimXmlHelper.GetElementIfNeeded(
                    cimNamespace,
                    string.IsNullOrWhiteSpace(validationError.Text),
                    CimChargeReceiptConstants.ReasonText,
                    () => validationError.Text));
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
