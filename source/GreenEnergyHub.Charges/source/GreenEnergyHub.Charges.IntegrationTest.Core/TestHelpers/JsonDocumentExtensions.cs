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
        private const string NotifyPriceList = "NotifyPriceList_MarketDocument";
        private const string MarketActivityRecord = "MktActivityRecord";
        private const string DocumentType = "type";
        private const string BusinessReasonCode = "process.processType";
        private const string ReceiverRole = "receiver_MarketParticipant.marketRole.type";
        private const string ChargeGroup = "ChargeGroup";
        private const string ChargeType = "ChargeType";
        private const string ChargeId = "mRID";
        private const string ChargeDescription = "description";

        public static JsonDocument AsJsonDocument(string jsonString)
        {
            return JsonDocument.Parse(jsonString);
        }

        public static string GetDocumentType(this JsonDocument document)
        {
            return document.RootElement
                .GetProperty(NotifyPriceList)
                .GetProperty(DocumentType)
                .GetProperty("value")
                .ToString();
        }

        public static string GetBusinessReasonCode(this JsonDocument document)
        {
            return document.RootElement
                .GetProperty(NotifyPriceList)
                .GetProperty(BusinessReasonCode)
                .GetProperty("value")
                .ToString();
        }

        public static string GetReceiverRole(this JsonDocument document)
        {
            return document.RootElement
                .GetProperty(NotifyPriceList)
                .GetProperty(ReceiverRole)
                .GetProperty("value")
                .ToString();
        }

        public static IEnumerable<string> GetChargeIds(this JsonDocument document)
        {
            var mktActivityRecordElements = GetMktActivityRecords(document);
            var chargeIds = new List<string>();
            foreach (var chargeType in mktActivityRecordElements.EnumerateArray().Select(record => record.GetProperty(ChargeGroup).GetProperty(ChargeType)))
            {
                chargeIds.AddRange(chargeType.EnumerateArray().Select(ct => ct.GetProperty(ChargeId).ToString()));
            }

            return chargeIds;
        }

        public static IEnumerable<string> GetChargeDescriptions(this JsonDocument document)
        {
            var mktActivityRecordElements = GetMktActivityRecords(document);
            var chargeDescriptions = new List<string>();
            foreach (var chargeType in mktActivityRecordElements.EnumerateArray().Select(record => record.GetProperty(ChargeGroup).GetProperty(ChargeType)))
            {
                chargeDescriptions.AddRange(chargeType.EnumerateArray().Select(ct => ct.GetProperty(ChargeDescription).ToString()));
            }

            return chargeDescriptions;
        }

        private static JsonElement GetMktActivityRecords(this JsonDocument document)
        {
            return document.RootElement
                .GetProperty(NotifyPriceList)
                .GetProperty(MarketActivityRecord);
        }
    }
}
