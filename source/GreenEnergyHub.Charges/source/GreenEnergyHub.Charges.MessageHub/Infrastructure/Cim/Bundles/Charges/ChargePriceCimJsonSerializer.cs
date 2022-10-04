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
using System.Text.Json.Nodes;
using GreenEnergyHub.Charges.Core.DateTime;
using GreenEnergyHub.Charges.Domain.Dtos.SharedDtos;
using GreenEnergyHub.Charges.Infrastructure.Core.Cim.Charges;
using GreenEnergyHub.Charges.Infrastructure.Core.Cim.MarketDocument;
using GreenEnergyHub.Charges.MessageHub.Models.AvailableChargeData;
using GreenEnergyHub.Iso8601;
using NodaTime;

namespace GreenEnergyHub.Charges.MessageHub.Infrastructure.Cim.Bundles.Charges
{
    public class ChargePriceCimJsonSerializer : CimJsonSerializer<AvailableChargePriceData>
    {
        private readonly IIso8601Durations _iso8601Durations;

        public ChargePriceCimJsonSerializer(
            IClock clock,
            IIso8601Durations iso8601Durations,
            ICimIdProvider cimIdProvider)
            : base(clock, cimIdProvider)
        {
            _iso8601Durations = iso8601Durations;
        }

        protected override string GetRootElementName()
        {
            return CimChargeConstants.NotifyRootElement;
        }

        protected override DocumentType GetDocumentType()
        {
            return DocumentType.NotifyPriceList;
        }

        protected override JsonObject GetActivityRecord(AvailableChargePriceData record)
        {
            ArgumentNullException.ThrowIfNull(record);
            var activity = new JsonObject
            {
                { CimChargeConstants.MarketActivityRecordId, CimIdProvider.GetUniqueId() },
                { CimChargeConstants.ChargeGroup, GetChargeGroup(record) },
            };
            return activity;
        }

        private JsonObject GetChargeGroup(AvailableChargePriceData chargePrice)
        {
            var chargeGroup = new JsonObject() { { CimChargeConstants.ChargeTypeElement, GetChargeType(chargePrice) }, };

            return chargeGroup;
        }

        private JsonArray GetChargeType(AvailableChargePriceData chargePrice)
        {
            var chargeType = new JsonArray() { GetChargeInformation(chargePrice) };
            return chargeType;
        }

        private JsonObject GetChargeInformation(AvailableChargePriceData chargePrice)
        {
            var chargeInformation = new JsonObject()
            {
                { CimChargeConstants.ChargeId, chargePrice.ChargeId },
                {
                    CimChargeConstants.ChargeOwner,
                    CimJsonHelper.CreateValueObject(chargePrice.ChargeOwner, CodingScheme.GS1)
                },
                { CimChargeConstants.EffectiveDate, chargePrice.StartDateTime.ToDateTimeUtc() },
                {
                    CimChargeConstants.ChargeType,
                    CimJsonHelper.CreateValueObject(ChargeTypeMapper.Map(chargePrice.ChargeType))
                },
                { CimChargeConstants.SeriesPeriod, GetSeriesPeriod(chargePrice) },
            };
            return chargeInformation;
        }

        private JsonArray GetSeriesPeriod(AvailableChargePriceData chargePrice)
        {
            var seriesPeriod = new JsonArray() { GetTimeInterval(chargePrice) };
            return seriesPeriod;
        }

        private JsonObject GetTimeInterval(AvailableChargePriceData chargePrice)
        {
            var timeInterval = new JsonObject()
            {
                { CimChargeConstants.PeriodResolution, ResolutionMapper.Map(chargePrice.Resolution) },
                { CimChargeConstants.TimeInterval, GetTimeIntervalDates(chargePrice) },
                { CimChargeConstants.Point, GetPoints(chargePrice) },
            };
            return timeInterval;
        }

        private JsonObject GetTimeIntervalDates(AvailableChargePriceData chargePrice)
        {
            var timeIntervalDates = new JsonObject()
            {
                {
                    CimChargeConstants.TimeIntervalStart,
                    CimJsonHelper.CreateValueObject(chargePrice.StartDateTime.GetTimeAndPriceSeriesDateTimeFormat())
                },
                {
                    CimChargeConstants.TimeIntervalEnd, CimJsonHelper.CreateValueObject(_iso8601Durations
                        .GetTimeFixedToDuration(
                            chargePrice.StartDateTime,
                            ResolutionMapper.Map(chargePrice.Resolution),
                            chargePrice.Points.Count)
                        .GetTimeAndPriceSeriesDateTimeFormat())
                },
            };
            return timeIntervalDates;
        }

        private static JsonArray GetPoints(AvailableChargePriceData chargePrice)
        {
            var points = new JsonArray();
            foreach (var pricePoint in chargePrice.Points)
            {
                var point = new JsonObject()
                {
                    { CimChargeConstants.Position, CimJsonHelper.CreateValueObject(pricePoint.Position) },
                    { CimChargeConstants.Price, CimJsonHelper.CreateValueObject(pricePoint.Price) },
                };
                points.Add(point);
            }

            return points;
        }
    }
}
