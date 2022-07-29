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
using GreenEnergyHub.Charges.Core.DateTime;
using GreenEnergyHub.Charges.Domain.AvailableData.AvailableChargeData;
using GreenEnergyHub.Charges.Domain.Charges;
using GreenEnergyHub.Charges.Domain.Dtos.SharedDtos;
using GreenEnergyHub.Charges.Infrastructure.Core.Cim.Charges;
using GreenEnergyHub.Charges.Infrastructure.Core.Cim.MarketDocument;
using GreenEnergyHub.Iso8601;
using NodaTime;

namespace GreenEnergyHub.Charges.MessageHub.Infrastructure.Cim.Bundles.Charges
{
    public class ChargeCimSerializer : CimSerializer<AvailableChargeData>
    {
        private readonly IIso8601Durations _iso8601Durations;

        public ChargeCimSerializer(
            IClock clock,
            IIso8601Durations iso8601Durations,
            ICimIdProvider cimIdProvider)
            : base(clock, cimIdProvider)
        {
            _iso8601Durations = iso8601Durations;
        }

        protected override XNamespace GetNamespace(IEnumerable<AvailableChargeData> records)
        {
            return CimChargeConstants.NotifyNamespace;
        }

        protected override XNamespace GetSchemaLocation(IEnumerable<AvailableChargeData> records)
        {
            return CimChargeConstants.NotifySchemaLocation;
        }

        protected override string GetRootElementName(IEnumerable<AvailableChargeData> records)
        {
            return CimChargeConstants.NotifyRootElement;
        }

        protected override DocumentType GetDocumentType(IEnumerable<AvailableChargeData> records)
        {
            return DocumentType.NotifyPriceList;
        }

        protected override XElement GetActivityRecord(
            XNamespace cimNamespace,
            AvailableChargeData charge)
        {
            return new XElement(
                cimNamespace + CimMarketDocumentConstants.MarketActivityRecord,
                new XElement(cimNamespace + CimChargeConstants.MarketActivityRecordId, CimIdProvider.GetUniqueId()),
                new XElement(cimNamespace + CimChargeConstants.SnapshotDateTime, charge.RequestDateTime.ToString()),
                GetChargeGroupElement(cimNamespace, charge));
        }

        private XElement GetChargeGroupElement(
            XNamespace cimNamespace,
            AvailableChargeData charge)
        {
            if (charge.BusinessReasonCode == BusinessReasonCode.UpdateChargeInformation)
            {
                return new XElement(
                    cimNamespace + CimChargeConstants.ChargeGroup,
                    GetChargeInformationTypeElement(cimNamespace, charge));
            }
            else
            {
                return new XElement(
                    cimNamespace + CimChargeConstants.ChargeGroup,
                    GetChargePricesTypeElement(cimNamespace, charge));
            }
        }

        private XElement GetChargeInformationTypeElement(
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
                new XElement(cimNamespace + CimChargeConstants.ChargeId, charge.ChargeId),
                // Charge name
                CimHelper.GetElementIfNeeded(
                    cimNamespace,
                    string.IsNullOrEmpty(charge.ChargeName),
                    CimChargeConstants.ChargeName,
                    () => charge.ChargeName),
                // Charge description
                CimHelper.GetElementIfNeeded(
                    cimNamespace,
                    string.IsNullOrEmpty(charge.ChargeDescription),
                    CimChargeConstants.ChargeDescription,
                    () => charge.ChargeDescription),
                // Charge resolution
                CimHelper.GetElementIfNeeded(
                    cimNamespace,
                    // Charge resolution is not needed if there are prices, as it will be added in that section
                    charge.Points.Count > 0,
                    CimChargeConstants.ChargeResolution,
                    () => ResolutionMapper.Map(charge.Resolution)),
                // EffectiveDate
                new XElement(cimNamespace + CimChargeConstants.EffectiveDate, charge.StartDateTime.ToString()),
                // TerminationDate
                CimHelper.GetElementIfNeeded(
                    cimNamespace,
                    charge.EndDateTime.IsEndDefault(),
                    CimChargeConstants.TerminationDate,
                    () => charge.EndDateTime.ToString()),
                // VatClassification
                CimHelper.GetElementIfNeeded(
                    cimNamespace,
                    charge.VatClassification == VatClassification.Unknown,
                    CimChargeConstants.VatClassification,
                    () => VatClassificationMapper.Map(charge.VatClassification)),
                // TransparentInvoicing
                CimHelper.GetElementIfNeeded(
                    cimNamespace,
                    // Right now, charge name is our best bet of determining whether to include transparent invoicing
                    string.IsNullOrEmpty(charge.ChargeName),
                    CimChargeConstants.TransparentInvoicing,
                    () => charge.TransparentInvoicing),
                // TaxIndicator
                CimHelper.GetElementIfNeeded(
                    cimNamespace,
                    // Right now, charge name is our best bet of determining whether to include tax indicator
                    string.IsNullOrEmpty(charge.ChargeName),
                    CimChargeConstants.TaxIndicator,
                    () => charge.TaxIndicator),
                GetSeriesPeriod(cimNamespace, charge));
        }

        private XElement GetChargePricesTypeElement(
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
                new XElement(cimNamespace + CimChargeConstants.ChargeId, charge.ChargeId),
                // EffectiveDate
                new XElement(cimNamespace + CimChargeConstants.EffectiveDate, charge.StartDateTime.ToString()),
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
                        new XElement(
                            cimNamespace + CimChargeConstants.PeriodResolution,
                            ResolutionMapper.Map(charge.Resolution)),
                        GetTimeInterval(cimNamespace, charge),
                        charge.Points.OrderBy(p => p.Position).Select(p => GetPoint(cimNamespace, p))));
            }

            return seriesPeriod;
        }

        private XElement GetTimeInterval(XNamespace cimNamespace, AvailableChargeData charge)
        {
            return new XElement(
                cimNamespace + CimChargeConstants.TimeInterval,
                new XElement(cimNamespace + CimChargeConstants.TimeIntervalStart, charge.StartDateTime.GetTimeAndPriceSeriesDateTimeFormat()),
                new XElement(
                    cimNamespace + CimChargeConstants.TimeIntervalEnd,
                    _iso8601Durations.GetTimeFixedToDuration(
                        charge.StartDateTime,
                        ResolutionMapper.Map(charge.Resolution),
                        charge.Points.Count)
                        .GetTimeAndPriceSeriesDateTimeFormat()));
        }

        private static XElement GetPoint(XNamespace cimNamespace, AvailableChargeDataPoint point)
        {
            return new XElement(
                cimNamespace + CimChargeConstants.Point,
                new XElement(cimNamespace + CimChargeConstants.Position, point.Position),
                new XElement(cimNamespace + CimChargeConstants.Price, point.Price));
        }
    }
}
