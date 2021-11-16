﻿// Copyright 2020 Energinet DataHub A/S
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
using GreenEnergyHub.Charges.Domain.MarketParticipants;
using GreenEnergyHub.Charges.Infrastructure.Configuration;
using GreenEnergyHub.Charges.Infrastructure.MarketDocument.Cim;
using NodaTime;

namespace GreenEnergyHub.Charges.Infrastructure.Cim
{
    public abstract class CimSerializer<T>
    {
        public CimSerializer(
            IHubSenderConfiguration hubSenderConfiguration,
            IClock clock,
            ICimIdProvider cimIdProvider)
        {
            HubSenderConfiguration = hubSenderConfiguration;
            Clock = clock;
            CimIdProvider = cimIdProvider;
        }

        public IHubSenderConfiguration HubSenderConfiguration { get; }

        public IClock Clock { get; }

        public ICimIdProvider CimIdProvider { get; }

        public async Task SerializeToStreamAsync(
            IEnumerable<T> records,
            Stream stream,
            BusinessReasonCode businessReasonCode,
            string recipientId,
            MarketParticipantRole recipientRole)
        {
            var document = GetDocument(records, businessReasonCode, recipientId, recipientRole);
            await document.SaveAsync(stream, SaveOptions.None, CancellationToken.None);

            stream.Position = 0;
        }

        public virtual IEnumerable<XElement> GetAdditionalDocumentFields(IEnumerable<T> records)
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
            string recipientId,
            MarketParticipantRole recipientRole)
        {
            XNamespace cimNamespace = GetNamespace(records);
            XNamespace xmlSchemaNamespace = CimMarketDocumentConstants.SchemaValidationNamespace;
            XNamespace xmlSchemaLocation = GetSchemaLocation(records);

            return new XDocument(
                new XElement(
                    cimNamespace + GetRootElementName(records),
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
                        CimIdProvider,
                        GetDocumentType(records),
                        businessReasonCode,
                        HubSenderConfiguration,
                        recipientId,
                        recipientRole,
                        Clock),
                    GetAdditionalDocumentFields(records),
                    GetActivityRecords(cimNamespace, records)));
        }

        private IEnumerable<XElement> GetActivityRecords(
            XNamespace cimNamespace,
            IEnumerable<T> records)
        {
            return records.Select(record => GetActivityRecord(cimNamespace, record));
        }
    }
}
