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
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using GreenEnergyHub.Charges.Domain.AvailableChargeLinkReceiptData;
using GreenEnergyHub.Charges.Domain.MarketParticipants;
using GreenEnergyHub.Charges.Infrastructure.Cim;
using GreenEnergyHub.Charges.Infrastructure.Configuration;
using GreenEnergyHub.Charges.Infrastructure.MarketDocument.Cim;
using NodaTime;

namespace GreenEnergyHub.Charges.Infrastructure.ChargeLinkReceiptBundle.Cim
{
    public class ChargeLinkReceiptCimSerializer : IChargeLinkReceiptCimSerializer
    {
        private IHubSenderConfiguration _hubSenderConfiguration;
        private IClock _clock;
        private ICimIdProvider _cimIdProvider;

        public ChargeLinkReceiptCimSerializer(
            IHubSenderConfiguration hubSenderConfiguration,
            IClock clock,
            ICimIdProvider cimIdProvider)
        {
            _hubSenderConfiguration = hubSenderConfiguration;
            _clock = clock;
            _cimIdProvider = cimIdProvider;
        }

        public async Task SerializeToStreamAsync(IEnumerable<AvailableChargeLinkReceiptData> receipts, Stream stream)
        {
            var document = GetDocument(receipts);
            await document.SaveAsync(stream, SaveOptions.None, CancellationToken.None);

            stream.Position = 0;
        }

        private XDocument GetDocument(IEnumerable<AvailableChargeLinkReceiptData> receipts)
        {
            XNamespace cimNamespace = CimChargeLinkReceiptConstants.ConfirmNamespace;
            XNamespace xmlSchemaNamespace = CimMarketDocumentConstants.SchemaValidationNamespace;
            XNamespace xmlSchemaLocation = CimChargeLinkReceiptConstants.ConfirmSchemaLocation;

            return new XDocument(
                new XElement(
                    cimNamespace + CimChargeLinkReceiptConstants.ConfirmRootElement,
                    new XAttribute(
                        XNamespace.Xmlns + CimMarketDocumentConstants.SchemaNamespaceAbbreviation,
                        xmlSchemaNamespace),
                    new XAttribute(
                        XNamespace.Xmlns + CimMarketDocumentConstants.CimNamespaceAbbreviation,
                        cimNamespace),
                    new XAttribute(
                        xmlSchemaNamespace + CimMarketDocumentConstants.SchemaLocation,
                        xmlSchemaLocation),
                    // Note: The list will always have same recipient, business reason code and receipt status,
                    // so we just take those values from the first element
                    MarketDocumentSerializationHelper.Serialize(
                        cimNamespace,
                        _cimIdProvider,
                        DocumentType.ChargeLinkReceipt,
                        receipts.First().BusinessReasonCode,
                        _hubSenderConfiguration,
                        receipts.First().RecipientId,
                        receipts.First().RecipientRole,
                        _clock),
                    new XElement(
                        cimNamespace + CimChargeLinkReceiptConstants.ReceiptStatus,
                        ReceiptStatusMapper.Map(receipts.First().ReceiptStatus)),
                    GetActivityRecords(cimNamespace, receipts)));
        }

        private IEnumerable<XElement> GetActivityRecords(
            XNamespace cimNamespace,
            IEnumerable<AvailableChargeLinkReceiptData> receipts)
        {
            return receipts.Select(receipt => GetActivityRecord(cimNamespace, receipt));
        }

        private XElement GetActivityRecord(
            XNamespace cimNamespace,
            AvailableChargeLinkReceiptData receipt)
        {
            return new XElement(
                cimNamespace + CimMarketDocumentConstants.MarketActivityRecord,
                new XElement(
                    cimNamespace + CimChargeLinkReceiptConstants.MarketActivityRecordId,
                    _cimIdProvider.GetUniqueId()),
                new XElement(
                    cimNamespace + CimChargeLinkReceiptConstants.OriginalOperationId,
                    receipt.OriginalOperationId),
                new XElement(
                    cimNamespace + CimChargeLinkReceiptConstants.MeteringPointId,
                    new XAttribute(
                        CimMarketDocumentConstants.CodingScheme,
                        CodingSchemeMapper.Map(CodingScheme.GS1)),
                    receipt.MeteringPointId));
        }
    }
}
