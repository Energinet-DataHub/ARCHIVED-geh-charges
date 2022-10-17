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
using System.Text.Json;

namespace GreenEnergyHub.Charges.IntegrationTest.Core.TestHelpers
{
    public static class JsonDocumentExtensions
    {
        public static JsonDocument AsJsonDocument(string jsonString)
        {
            return JsonDocument.Parse(jsonString);
        }

        public static string GetDocumentType(this JsonDocument document)
        {
            return document.RootElement
                .GetProperty(CimMessageConstants.NotifyPriceListRootElement)
                .GetProperty(CimMessageConstants.DocumentType)
                .GetProperty("value")
                .ToString();
        }

        public static string GetBusinessReasonCode(this JsonDocument document)
        {
            return document.RootElement
                .GetProperty(CimMessageConstants.NotifyPriceListRootElement)
                .GetProperty(CimMessageConstants.BusinessReasonCode)
                .GetProperty("value")
                .ToString();
        }

        public static string GetReceiverRole(this JsonDocument document)
        {
            return document.RootElement
                .GetProperty(CimMessageConstants.NotifyPriceListRootElement)
                .GetProperty(CimMessageConstants.ReceiverRole)
                .GetProperty("value")
                .ToString();
        }

        public static IEnumerable<string> GetChargeIds(this JsonDocument document)
        {
            var mktActivityRecordElements = GetMktActivityRecords(document);
            var chargeIds = new List<string>();
            foreach (var chargeType in mktActivityRecordElements.EnumerateArray()
                         .Select(record => record.GetProperty(CimMessageConstants.ChargeGroup)
                             .GetProperty(CimMessageConstants.ChargeType)))
            {
                chargeIds.AddRange(chargeType.EnumerateArray().Select(ct => ct.GetProperty(CimMessageConstants.ChargeId).ToString()));
            }

            return chargeIds;
        }

        public static IEnumerable<string> GetChargeDescriptions(this JsonDocument document)
        {
            var mktActivityRecordElements = GetMktActivityRecords(document);
            var chargeDescriptions = new List<string>();
            foreach (var chargeType in mktActivityRecordElements.EnumerateArray()
                         .Select(record => record.GetProperty(CimMessageConstants.ChargeGroup)
                             .GetProperty(CimMessageConstants.ChargeType)))
            {
                chargeDescriptions.AddRange(chargeType.EnumerateArray().Select(ct => ct.GetProperty(CimMessageConstants.ChargeDescription).ToString()));
            }

            return chargeDescriptions;
        }

        public static IEnumerable<string> GetPrices(this JsonDocument document)
        {
            var prices = new List<string>();
            var mktActivityRecordElements = GetMktActivityRecords(document);
            foreach (var record in mktActivityRecordElements.EnumerateArray())
            {
                prices.AddRange(GetPrices(record));
            }

            return prices;
        }

        private static JsonElement GetMktActivityRecords(this JsonDocument document)
        {
            return document.RootElement
                .GetProperty(CimMessageConstants.NotifyPriceListRootElement)
                .GetProperty(CimMessageConstants.MarketActivityRecord);
        }

        private static IEnumerable<string> GetPrices(JsonElement marketActivityRecord)
        {
            var prices = new List<string>();

            var priceOperation = marketActivityRecord
                .GetProperty(CimMessageConstants.ChargeGroup)
                .GetProperty(CimMessageConstants.ChargeType);

            foreach (var operation in priceOperation.EnumerateArray()
                         .Select(ct => ct.GetProperty(CimMessageConstants.SeriesPeriod))
                         .SelectMany(seriesPeriod => seriesPeriod.EnumerateArray()
                             .Select(p => p.GetProperty(CimMessageConstants.Point))))
            {
                prices.AddRange(operation.EnumerateArray()
                    .Select(point => point.GetProperty(CimMessageConstants.Price).GetProperty("value").ToString()));
            }

            return prices;
        }
    }
}
