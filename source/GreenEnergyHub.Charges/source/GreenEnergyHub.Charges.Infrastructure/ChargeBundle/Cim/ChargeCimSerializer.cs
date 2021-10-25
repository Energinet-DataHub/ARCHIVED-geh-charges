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
using GreenEnergyHub.Charges.Domain.AvailableChargeData;
using GreenEnergyHub.Charges.Domain.Charges;
using GreenEnergyHub.Charges.Domain.MarketParticipants;
using GreenEnergyHub.Charges.Infrastructure.ChargeLinkBundle.Cim;
using GreenEnergyHub.Charges.Infrastructure.Configuration;
using GreenEnergyHub.Charges.Infrastructure.MarketDocument.Cim;
using GreenEnergyHub.Iso8601;
using NodaTime;

namespace GreenEnergyHub.Charges.Infrastructure.ChargeBundle.Cim
{
    public class ChargeCimSerializer : IChargeCimSerializer
    {
        private IHubSenderConfiguration _hubSenderConfiguration;
        private IClock _clock;
        private IIso8601Durations _iso8601Durations;

        public ChargeCimSerializer(
            IHubSenderConfiguration hubSenderConfiguration,
            IClock clock,
            IIso8601Durations iso8601Durations)
        {
            _hubSenderConfiguration = hubSenderConfiguration;
            _clock = clock;
            _iso8601Durations = iso8601Durations;
        }

        public async Task SerializeToStreamAsync(IEnumerable<AvailableChargeData> charges, Stream stream)
        {
            var document = GetDocument(charges);
            await document.SaveAsync(stream, SaveOptions.None, CancellationToken.None);

            stream.Position = 0;
        }

        private XDocument GetDocument(IEnumerable<AvailableChargeData> charges)
        {
            XNamespace cimNamespace = CimChargeConstants.NotifyNamespace;
            XNamespace xmlSchemaNamespace = CimMarketDocumentConstants.SchemaValidationNamespace;
            XNamespace xmlSchemaLocation = CimChargeConstants.NotifySchemaLocation;

            return new XDocument(
                new XElement(
                    cimNamespace + CimChargeConstants.NotifyRootElement,
                    new XAttribute(
                        XNamespace.Xmlns + CimMarketDocumentConstants.SchemaNamespaceAbbreviation,
                        xmlSchemaNamespace),
                    new XAttribute(
                        XNamespace.Xmlns + CimMarketDocumentConstants.CimNamespaceAbbreviation,
                        cimNamespace),
                    new XAttribute(
                        xmlSchemaNamespace + CimMarketDocumentConstants.SchemaLocation,
                        xmlSchemaLocation),
                    GetMarketDocumentHeader(cimNamespace, charges.First()), // Note: The list will always have same recipient and business reason code, so we just take those values from the first element
                    GetActivityRecords(cimNamespace, charges)));
        }

        private IEnumerable<XElement> GetMarketDocumentHeader(XNamespace cimNamespace, AvailableChargeData charge)
        {
            return new List<XElement>()
            {
                new XElement(cimNamespace + CimMarketDocumentConstants.Id, Guid.NewGuid()),
                new XElement(
                    cimNamespace + CimMarketDocumentConstants.Type,
                    DocumentTypeMapper.Map(DocumentType.NotifyPriceList)),
                /*new XElement(
                    cimNamespace +
                    CimMarketDocumentConstants.BusinessReasonCode,
                    BusinessReasonCodeMapper.Map(charge.BusinessReasonCode)),*/
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
                /*new XElement(
                    cimNamespace + CimMarketDocumentConstants.RecipientId,
                    new XAttribute(
                        CimMarketDocumentConstants.CodingScheme,
                        CodingSchemeMapper.Map(CodingScheme.GS1)),
                    charge.RecipientId),
                new XElement(
                    cimNamespace + CimMarketDocumentConstants.RecipientBusinessProcessRole,
                    MarketParticipantRoleMapper.Map(charge.RecipientRole)),*/
                new XElement(
                    cimNamespace + CimMarketDocumentConstants.CreatedDateTime,
                    _clock.GetCurrentInstant().ToString()),
            };
        }

        private IEnumerable<XElement> GetActivityRecords(
            XNamespace cimNamespace,
            IEnumerable<AvailableChargeData> charges)
        {
            return charges.Select(charge => GetActivityRecord(cimNamespace, charge));
        }

        private XElement GetActivityRecord(
            XNamespace cimNamespace,
            AvailableChargeData charge)
        {
            return new XElement(
                cimNamespace + CimMarketDocumentConstants.MarketActivityRecord,
                new XElement(cimNamespace + CimChargeConstants.MarketActivityRecordId, Guid.NewGuid().ToString()),
                new XElement(cimNamespace + CimChargeConstants.SnapshotDateTime, charge.RequestTime.ToString()),
                /*new XElement(cimNamespace + CimChargeLinkConstants.StartDateTime, chargeLink.StartDateTime.ToString()),
                GetEndDateTimeOnlyIfNotEndDefault(cimNamespace, chargeLink.EndDateTime),*/
                GetChargeGroupElement(cimNamespace, charge));
        }

/*        private static IEnumerable<XElement> GetEndDateTimeOnlyIfNotEndDefault(XNamespace cimNamespace, Instant endDateTime)
        {
            return endDateTime.IsEndDefault()
                ? new List<XElement>()
                : new List<XElement>
                {
                    new XElement(cimNamespace + CimChargeLinkConstants.EndDateTime, endDateTime.ToString()),
                };
        }*/

        private XElement GetChargeGroupElement(
            XNamespace cimNamespace,
            AvailableChargeData charge)
        {
            return new XElement(
                cimNamespace + CimChargeConstants.ChargeGroup,
                GetChargeTypeElement(cimNamespace, charge));
        }

        private XElement GetChargeTypeElement(
            XNamespace cimNamespace,
            AvailableChargeData charge)
        {
            return new XElement(
                cimNamespace + CimChargeConstants.ChargeTypeElement,
                new XElement(
                    cimNamespace + CimChargeConstants.ChargeOwner,
                    new XAttribute(
                        CimMarketDocumentConstants.CodingScheme,
                        CodingSchemeMapper.Map(CodingScheme.GS1)),
                    charge.ChargeOwner),
                new XElement(cimNamespace + CimChargeConstants.ChargeType, ChargeTypeMapper.Map(charge.ChargeType)),
                /*new XElement(cimNamespace + CimChargeConstants.ChargeId, charge.ChargeId),*/
                // Charge name
                GetElementIfNeeded(
                    cimNamespace,
                    string.IsNullOrEmpty(charge.ChargeName),
                    CimChargeConstants.ChargeName,
                    () => charge.ChargeName),
                // Charge description
                GetElementIfNeeded(
                    cimNamespace,
                    string.IsNullOrEmpty(charge.ChargeDescription),
                    CimChargeConstants.ChargeDescription,
                    () => charge.ChargeDescription),
                // Charge resolution
                GetElementIfNeeded(
                    cimNamespace,
                    // Charge resolution is not needed if there are prices, as it will be added in that section
                    charge.Points.Count > 0,
                    CimChargeConstants.ChargeResolution,
                    /*() => ResolutionMapper.Map(charge.Resolution)*/ () => charge.Resolution.ToString()),
                new XElement(cimNamespace + CimChargeConstants.StartDateTime, charge.StartDateTime.ToString()),
                // EndDateTime
                GetElementIfNeeded(
                    cimNamespace,
                    charge.EndDateTime.IsEndDefault(),
                    CimChargeConstants.EndDateTime,
                    () => charge.EndDateTime.ToString()),
                // VatClassification
                GetElementIfNeeded(
                    cimNamespace,
                    charge.VatClassification == VatClassification.Unknown,
                    CimChargeConstants.VatClassification,
                    /*() => VatClassificationMapper.Map(charge.VatClassification)*/
                    () => charge.VatClassification.ToString()),
                // TransparentInvoicing
                GetElementIfNeeded(
                    cimNamespace,
                    // Right now, charge name is our best bet of determining whether to include transparent invoicing
                    string.IsNullOrEmpty(charge.ChargeName),
                    CimChargeConstants.TransparentInvoicing,
                    () => charge.TransparentInvoicing),
                // TaxIndicator
                GetElementIfNeeded(
                    cimNamespace,
                    // Right now, charge name is our best bet of determining whether to include tax indicator
                    string.IsNullOrEmpty(charge.ChargeName),
                    CimChargeConstants.TransparentInvoicing,
                    () => charge.TaxIndicator),
                GetSeriesPeriod(cimNamespace, charge));
        }

        private IEnumerable<XElement> GetSeriesPeriod(XNamespace cimNamespace, AvailableChargeData charge)
        {
            var seriesPeriod = new List<XElement>();

            if (charge.Points.Count > 0)
            {
                seriesPeriod.Add(
                    new XElement(
                        cimNamespace + CimChargeConstants.SeriesPeriod,
                        new XElement(cimNamespace + CimChargeConstants.PeriodResolution, /*ResolutionMapper.Map(charge.Resolution))*/ charge.Resolution.ToString()),
                        GetTimeInterval(cimNamespace, charge),
                        charge.Points.Select(p => GetPoint(cimNamespace, p))));
            }

            return seriesPeriod;
        }

        private XElement GetTimeInterval(XNamespace cimNamespace, AvailableChargeData charge)
        {
            return new XElement(
                cimNamespace + CimChargeConstants.TimeInterval,
                new XElement(cimNamespace + CimChargeConstants.TimeIntervalStart, charge.StartDateTime),
                new XElement(
                    cimNamespace + CimChargeConstants.TimeIntervalEnd,
                    _iso8601Durations.GetTimeFixedToDuration(
                        charge.StartDateTime,
                        /*ResolutionMapper.Map(charge.Resolution),*/charge.Resolution.ToString(),
                        charge.Points.Count)));
        }

        private static XElement GetPoint(XNamespace cimNamespace, AvailableChargeDataPoint point)
        {
            return new XElement(
                cimNamespace + CimChargeConstants.Point,
                new XElement(cimNamespace + CimChargeConstants.Position, point.Position),
                new XElement(cimNamespace + CimChargeConstants.Price, point.Price));
        }

        private static IEnumerable<XElement> GetElementIfNeeded(
            XNamespace cimNamespace,
            bool notNeeded,
            string elementName,
            Func<object> getValue)
        {
            return notNeeded
                ? new List<XElement>()
                : new List<XElement> { new XElement(cimNamespace + elementName, getValue.Invoke()) };
        }
    }
}
