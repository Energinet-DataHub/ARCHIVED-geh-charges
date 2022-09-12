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
using GreenEnergyHub.Charges.Domain.Dtos.SharedDtos;
using GreenEnergyHub.Charges.Infrastructure.Core.Cim.Charges;
using GreenEnergyHub.Charges.Infrastructure.Core.Cim.MarketDocument;
using GreenEnergyHub.Charges.MessageHub.Models.AvailableChargeData;
using GreenEnergyHub.Iso8601;
using NodaTime;

namespace GreenEnergyHub.Charges.MessageHub.Infrastructure.Cim.Bundles.Charges
{
    public class ChargePriceCimSerializer : CimSerializer<AvailableChargePriceData>
    {
        private readonly IIso8601Durations _iso8601Durations;

        public ChargePriceCimSerializer(
            IClock clock,
            IIso8601Durations iso8601Durations,
            ICimIdProvider cimIdProvider)
            : base(clock, cimIdProvider)
        {
            _iso8601Durations = iso8601Durations;
        }

        protected override XNamespace GetNamespace(IEnumerable<AvailableChargePriceData> records)
        {
            return CimChargeConstants.NotifyNamespace;
        }

        protected override XNamespace GetSchemaLocation(IEnumerable<AvailableChargePriceData> records)
        {
            return CimChargeConstants.NotifySchemaLocation;
        }

        protected override string GetRootElementName(IEnumerable<AvailableChargePriceData> records)
        {
            return CimChargeConstants.NotifyRootElement;
        }

        protected override DocumentType GetDocumentType(IEnumerable<AvailableChargePriceData> records)
        {
            return DocumentType.NotifyPriceList;
        }

        protected override XElement GetActivityRecord(
            XNamespace cimNamespace,
            AvailableChargePriceData record)
        {
            return new XElement(
                cimNamespace + CimMarketDocumentConstants.MarketActivityRecord,
                new XElement(cimNamespace + CimChargeConstants.MarketActivityRecordId, CimIdProvider.GetUniqueId()),
                new XElement(cimNamespace + CimChargeConstants.SnapshotDateTime, record.RequestDateTime.ToString()),
                GetChargeGroupElement(cimNamespace, record));
        }

        private XElement GetChargeGroupElement(
            XNamespace cimNamespace,
            AvailableChargePriceData chargePrice)
        {
                return new XElement(
                    cimNamespace + CimChargeConstants.ChargeGroup,
                    GetChargePricesTypeElement(cimNamespace, chargePrice));
        }

        private XElement GetChargePricesTypeElement(
            XNamespace cimNamespace,
            AvailableChargePriceData chargePrice)
        {
            return new XElement(
                cimNamespace + CimChargeConstants.ChargeTypeElement,
                new XElement(
                    cimNamespace + CimChargeConstants.ChargeOwner,
                    new XAttribute(
                        CimMarketDocumentConstants.CodingScheme,
                        CodingSchemeMapper.Map(CodingScheme.GS1)),
                    chargePrice.ChargeOwner),
                new XElement(cimNamespace + CimChargeConstants.ChargeType, ChargeTypeMapper.Map(chargePrice.ChargeType)),
                new XElement(cimNamespace + CimChargeConstants.ChargeId, chargePrice.ChargeId),
                // EffectiveDate
                new XElement(cimNamespace + CimChargeConstants.EffectiveDate, chargePrice.StartDateTime.ToString()),
                GetSeriesPeriod(cimNamespace, chargePrice));
        }

        private IEnumerable<XElement> GetSeriesPeriod(XNamespace cimNamespace, AvailableChargePriceData chargePrice)
        {
            var seriesPeriod = new List<XElement>();

            if (chargePrice.Points.Count > 0)
            {
                seriesPeriod.Add(
                    new XElement(
                        cimNamespace + CimChargeConstants.SeriesPeriod,
                        new XElement(
                            cimNamespace + CimChargeConstants.PeriodResolution,
                            ResolutionMapper.Map(chargePrice.Resolution)),
                        GetTimeInterval(cimNamespace, chargePrice),
                        chargePrice.Points.OrderBy(p => p.Position).Select(p => GetPoint(cimNamespace, p))));
            }

            return seriesPeriod;
        }

        private XElement GetTimeInterval(XNamespace cimNamespace, AvailableChargePriceData chargePrice)
        {
            return new XElement(
                cimNamespace + CimChargeConstants.TimeInterval,
                new XElement(cimNamespace + CimChargeConstants.TimeIntervalStart, chargePrice.StartDateTime.GetTimeAndPriceSeriesDateTimeFormat()),
                new XElement(
                    cimNamespace + CimChargeConstants.TimeIntervalEnd,
                    _iso8601Durations.GetTimeFixedToDuration(
                        chargePrice.StartDateTime,
                        ResolutionMapper.Map(chargePrice.Resolution),
                        chargePrice.Points.Count)
                        .GetTimeAndPriceSeriesDateTimeFormat()));
        }

        private static XElement GetPoint(XNamespace cimNamespace, AvailableChargePriceDataPoint point)
        {
            return new XElement(
                cimNamespace + CimChargeConstants.Point,
                new XElement(cimNamespace + CimChargeConstants.Position, point.Position),
                new XElement(cimNamespace + CimChargeConstants.Price, point.Price));
        }
    }
}
