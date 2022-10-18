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

namespace GreenEnergyHub.Charges.IntegrationTest.Core.TestHelpers
{
    public static class CimMessageConstants
    {
        public const string NotifyPriceListRootElement = "NotifyPriceList_MarketDocument";
        public const string RejectRequestChangeOfPriceListRootElement = "RejectRequestChangeOfPriceList_MarketDocument";
        public const string ConfirmRequestChangeOfPriceListRootElement = "ConfirmRequestChangeOfPriceList_MarketDocument";
        public const string MarketActivityRecord = "MktActivityRecord";
        public const string DocumentType = "type";
        public const string BusinessReasonCode = "process.processType";
        public const string ReceiverRole = "receiver_MarketParticipant.marketRole.type";
        public const string ChargeGroup = "ChargeGroup";
        public const string ChargeType = "ChargeType";
        public const string ChargeId = "mRID";
        public const string ChargeDescription = "description";
        public const string SeriesPeriod = "Series_Period";
        public const string Point = "Point";
        public const string Price = "price.amount";
        public const string OriginalTransactionId = "originalTransactionIDReference_MktActivityRecord.mRID";
    }
}
