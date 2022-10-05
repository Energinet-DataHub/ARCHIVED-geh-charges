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
using GreenEnergyHub.Charges.Domain.Dtos.SharedDtos;
using GreenEnergyHub.Charges.Infrastructure.Core.Cim.MarketDocument;
using GreenEnergyHub.Charges.MessageHub.Models.AvailableData;
using NodaTime;

namespace GreenEnergyHub.Charges.MessageHub.Infrastructure.Cim
{
    public abstract class CimXmlSerializer<T> : ICimXmlSerializer<T>
        where T : AvailableDataBase
    {
        protected CimXmlSerializer(IClock clock, ICimIdProvider cimIdProvider)
        {
            Clock = clock;
            CimIdProvider = cimIdProvider;
        }

        public IClock Clock { get; }

        public ICimIdProvider CimIdProvider { get; }

        public async Task SerializeToStreamAsync(
            IEnumerable<T> availableData,
            Stream stream,
            BusinessReasonCode businessReasonCode,
            string senderId,
            MarketParticipantRole senderRole,
            string recipientId,
            MarketParticipantRole recipientRole)
        {
            var document = GetDocument(availableData, businessReasonCode, senderId, senderRole, recipientId, recipientRole);
            await document.SaveAsync(stream, SaveOptions.None, CancellationToken.None).ConfigureAwait(false);

            stream.Position = 0;
        }

        public virtual IEnumerable<XElement> GetAdditionalDocumentFields(
            XNamespace cimNamespace,
            IEnumerable<T> records)
        {
            return new List<XElement>();
        }

        protected abstract XNamespace GetNamespace(IEnumerable<T> records);

        protected abstract XNamespace GetSchemaLocation(IEnumerable<T> records);

        protected abstract string GetRootElementName(IEnumerable<T> records);

        protected abstract DocumentType GetDocumentType(IEnumerable<T> records);

        protected abstract XElement GetActivityRecord(XNamespace cimNamespace, T record);

        private XDocument GetDocument(
            IEnumerable<T> records,
            BusinessReasonCode businessReasonCode,
            string senderId,
            MarketParticipantRole senderRole,
            string recipientId,
            MarketParticipantRole recipientRole)
        {
            var recordList = records.ToList();
            var cimNamespace = GetNamespace(recordList);

            // Note: The list will always have same recipient, business reason code and receipt status,
            // so we just take those values from the first element
            return new XDocument(
                new XElement(
                    cimNamespace + GetRootElementName(recordList),
                    new XAttribute(
                        XNamespace.Xmlns + CimMarketDocumentConstants.CimNamespaceAbbreviation, cimNamespace),
                    MarketDocumentSerializationHelper.Serialize(
                        cimNamespace,
                        CimIdProvider,
                        GetDocumentType(recordList),
                        businessReasonCode,
                        senderId,
                        senderRole,
                        recipientId,
                        recipientRole,
                        Clock),
                    GetAdditionalDocumentFields(cimNamespace, recordList),
                    GetActivityRecords(cimNamespace, recordList)));
        }

        private IEnumerable<XElement> GetActivityRecords(XNamespace cimNamespace, IEnumerable<T> records)
        {
            return records.Select(record => GetActivityRecord(cimNamespace, record));
        }
    }
}
