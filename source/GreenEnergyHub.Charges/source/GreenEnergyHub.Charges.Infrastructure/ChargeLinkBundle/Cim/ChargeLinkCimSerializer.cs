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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using GreenEnergyHub.Charges.Core.DateTime;
using GreenEnergyHub.Charges.Domain.AvailableChargeLinksData;
using GreenEnergyHub.Charges.Domain.MarketParticipants;
using GreenEnergyHub.Charges.Infrastructure.Cim;
using GreenEnergyHub.Charges.Infrastructure.Configuration;
using GreenEnergyHub.Charges.Infrastructure.MarketDocument.Cim;
using NodaTime;

namespace GreenEnergyHub.Charges.Infrastructure.ChargeLinkBundle.Cim
{
    public class ChargeLinkCimSerializer : IChargeLinkCimSerializer
    {
        private IHubSenderConfiguration _hubSenderConfiguration;
        private IClock _clock;
        private ICimIdProvider _cimIdProvider;

        public ChargeLinkCimSerializer(
            IHubSenderConfiguration hubSenderConfiguration,
            IClock clock,
            ICimIdProvider cimIdProvider)
        {
            _hubSenderConfiguration = hubSenderConfiguration;
            _clock = clock;
            _cimIdProvider = cimIdProvider;
        }

        public async Task SerializeToStreamAsync(IEnumerable<AvailableChargeLinksData> chargeLinks, Stream stream)
        {
            var document = GetDocument(chargeLinks);
            await document.SaveAsync(stream, SaveOptions.None, CancellationToken.None);

            stream.Position = 0;
        }

        private XDocument GetDocument(IEnumerable<AvailableChargeLinksData> chargeLinks)
        {
            XNamespace cimNamespace = CimChargeLinkConstants.NotifyNamespace;
            XNamespace xmlSchemaNamespace = CimMarketDocumentConstants.SchemaValidationNamespace;
            XNamespace xmlSchemaLocation = CimChargeLinkConstants.NotifySchemaLocation;

            return new XDocument(
                new XElement(
                    cimNamespace + CimChargeLinkConstants.NotifyRootElement,
                    new XAttribute(
                        XNamespace.Xmlns + CimMarketDocumentConstants.SchemaNamespaceAbbreviation,
                        xmlSchemaNamespace),
                    new XAttribute(
                        XNamespace.Xmlns + CimMarketDocumentConstants.CimNamespaceAbbreviation,
                        cimNamespace),
                    new XAttribute(
                        xmlSchemaNamespace + CimMarketDocumentConstants.SchemaLocation,
                        xmlSchemaLocation),
                    // Note: The list will always have same recipient and business reason code,
                    // so we just take those values from the first element
                    GetMarketDocumentHeader(cimNamespace, chargeLinks.First()),
                    GetActivityRecords(cimNamespace, chargeLinks)));
        }

        private IEnumerable<XElement> GetMarketDocumentHeader(XNamespace cimNamespace, AvailableChargeLinksData chargeLink)
        {
            return new List<XElement>()
            {
                new XElement(cimNamespace + CimMarketDocumentConstants.Id, _cimIdProvider.GetUniqueId()),
                new XElement(
                    cimNamespace + CimMarketDocumentConstants.Type,
                    DocumentTypeMapper.Map(DocumentType.NotifyBillingMasterData)),
                new XElement(
                    cimNamespace +
                    CimMarketDocumentConstants.BusinessReasonCode,
                    BusinessReasonCodeMapper.Map(chargeLink.BusinessReasonCode)),
                new XElement(
                    cimNamespace + CimMarketDocumentConstants.IndustryClassification,
                    IndustryClassificationMapper.Map(IndustryClassification.Electricity)),
                new XElement(
                    cimNamespace + CimMarketDocumentConstants.SenderId,
                    new XAttribute(
                        CimMarketDocumentConstants.CodingScheme,
                        CodingSchemeMapper.Map(CodingScheme.GS1)),
                    _hubSenderConfiguration.GetSenderMarketParticipant().Id),
                new XElement(
                    cimNamespace + CimMarketDocumentConstants.SenderBusinessProcessRole,
                    MarketParticipantRoleMapper.Map(
                        _hubSenderConfiguration.GetSenderMarketParticipant().BusinessProcessRole)),
                new XElement(
                    cimNamespace + CimMarketDocumentConstants.RecipientId,
                    new XAttribute(
                        CimMarketDocumentConstants.CodingScheme,
                        CodingSchemeMapper.Map(CodingScheme.GS1)),
                    chargeLink.RecipientId),
                new XElement(
                    cimNamespace + CimMarketDocumentConstants.RecipientBusinessProcessRole,
                    MarketParticipantRoleMapper.Map(chargeLink.RecipientRole)),
                new XElement(
                    cimNamespace + CimMarketDocumentConstants.CreatedDateTime,
                    _clock.GetCurrentInstant().ToString()),
            };
        }

        private IEnumerable<XElement> GetActivityRecords(
            XNamespace cimNamespace,
            IEnumerable<AvailableChargeLinksData> chargeLinks)
        {
            return chargeLinks.Select(chargeLink => GetActivityRecord(cimNamespace, chargeLink));
        }

        private XElement GetActivityRecord(
            XNamespace cimNamespace,
            AvailableChargeLinksData chargeLink)
        {
            return new XElement(
                cimNamespace + CimMarketDocumentConstants.MarketActivityRecord,
                new XElement(cimNamespace + CimChargeLinkConstants.MarketActivityRecordId, _cimIdProvider.GetUniqueId()),
                new XElement(
                    cimNamespace + CimChargeLinkConstants.MeteringPointId,
                    new XAttribute(
                        CimMarketDocumentConstants.CodingScheme,
                        CodingSchemeMapper.Map(CodingScheme.GS1)),
                    chargeLink.MeteringPointId),
                new XElement(cimNamespace + CimChargeLinkConstants.StartDateTime, chargeLink.StartDateTime.ToString()),
                CimHelper.GetElementIfNeeded(
                    cimNamespace,
                    chargeLink.EndDateTime.IsEndDefault(),
                    CimChargeLinkConstants.EndDateTime,
                    () => chargeLink.EndDateTime.ToString()),
                GetChargeGroupElement(cimNamespace, chargeLink));
        }

        private static XElement GetChargeGroupElement(
            XNamespace cimNamespace,
            AvailableChargeLinksData chargeLink)
        {
            return new XElement(
                cimNamespace + CimChargeLinkConstants.ChargeGroup,
                GetChargeTypeElement(cimNamespace, chargeLink));
        }

        private static XElement GetChargeTypeElement(
            XNamespace cimNamespace,
            AvailableChargeLinksData chargeLink)
        {
            return new XElement(
                cimNamespace + CimChargeLinkConstants.ChargeTypeElement,
                new XElement(
                    cimNamespace + CimChargeLinkConstants.Owner,
                    new XAttribute(
                        CimMarketDocumentConstants.CodingScheme,
                        CodingSchemeMapper.Map(CodingScheme.GS1)),
                    chargeLink.ChargeOwner),
                new XElement(cimNamespace + CimChargeLinkConstants.ChargeType, ChargeTypeMapper.Map(chargeLink.ChargeType)),
                new XElement(cimNamespace + CimChargeLinkConstants.ChargeId, chargeLink.ChargeId),
                new XElement(cimNamespace + CimChargeLinkConstants.Factor, chargeLink.Factor));
        }
    }
}
