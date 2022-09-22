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
using GreenEnergyHub.Charges.MessageHub.Models.AvailableChargeData;
using GreenEnergyHub.Charges.MessageHub.Models.AvailableChargeLinksData;
using GreenEnergyHub.Charges.MessageHub.Models.AvailableChargeLinksReceiptData;
using GreenEnergyHub.Charges.MessageHub.Models.AvailableChargeReceiptData;
using GreenEnergyHub.Charges.MessageHub.Models.AvailableData;
using GreenEnergyHub.Charges.TestCore;
using NodaTime;

namespace GreenEnergyHub.Charges.IntegrationTests.IntegrationTests.Repositories
{
    public static class RepositoryAutoMoqDataFixer
    {
        public static IEnumerable<AvailableDataBase> GetAvailableDataListBasedOn(List<AvailableDataBase> availableList)
        {
            return availableList
                .Select(availableData => (AvailableDataBase)CreateAvailableDataBasedOn((dynamic)availableData))
                .ToList();
        }

        public static AvailableDataBase GetAvailableDataBasedOn(AvailableDataBase availableData)
        {
            return CreateAvailableDataBasedOn((dynamic)availableData);
        }

        public static AvailableDataBase GetAvailableDataBasedOn(
            AvailableDataBase availableData, Instant requestDateTime, int operationOrder)
        {
            return CreateAvailableDataBasedOn((dynamic)availableData, requestDateTime, operationOrder);
        }

        private static AvailableChargeData CreateAvailableDataBasedOn(
            AvailableChargeData availableChargeData,
            Instant? requestDateTime = default,
            int? operationOrder = default)
        {
            return new AvailableChargeData(
                availableChargeData.SenderId[..34],
                availableChargeData.SenderRole,
                availableChargeData.RecipientId[..34],
                availableChargeData.RecipientRole,
                availableChargeData.BusinessReasonCode,
                GetRequestDateTime(availableChargeData, requestDateTime),
                availableChargeData.AvailableDataReferenceId,
                availableChargeData.ChargeId[..34],
                availableChargeData.ChargeOwner[..34],
                availableChargeData.ChargeType,
                availableChargeData.ChargeName,
                availableChargeData.ChargeDescription,
                availableChargeData.StartDateTime,
                availableChargeData.EndDateTime,
                availableChargeData.VatClassification,
                availableChargeData.TaxIndicator,
                availableChargeData.TransparentInvoicing,
                availableChargeData.Resolution,
                availableChargeData.DocumentType,
                GetOperationOrder(availableChargeData, operationOrder),
                SeededData.MarketParticipants.SystemOperator.Id);
        }

        private static AvailableChargeReceiptData CreateAvailableDataBasedOn(
            AvailableChargeReceiptData availableChargeReceiptData,
            Instant? requestDateTime = default,
            int? operationOrder = default)
        {
            return new AvailableChargeReceiptData(
                availableChargeReceiptData.SenderId[..34],
                availableChargeReceiptData.SenderRole,
                availableChargeReceiptData.RecipientId[..34],
                availableChargeReceiptData.RecipientRole,
                availableChargeReceiptData.BusinessReasonCode,
                GetRequestDateTime(availableChargeReceiptData, requestDateTime),
                availableChargeReceiptData.AvailableDataReferenceId,
                availableChargeReceiptData.ReceiptStatus,
                availableChargeReceiptData.OriginalOperationId[..34],
                availableChargeReceiptData.DocumentType,
                GetOperationOrder(availableChargeReceiptData, operationOrder),
                SeededData.MarketParticipants.SystemOperator.Id,
                availableChargeReceiptData.ValidationErrors.ToList());
        }

        private static AvailableChargeLinksData CreateAvailableDataBasedOn(
            AvailableChargeLinksData availableChargeLinksData,
            Instant? requestDateTime = default,
            int? operationOrder = default)
        {
            return new AvailableChargeLinksData(
                availableChargeLinksData.SenderId[..34],
                availableChargeLinksData.SenderRole,
                availableChargeLinksData.RecipientId[..34],
                availableChargeLinksData.RecipientRole,
                availableChargeLinksData.BusinessReasonCode,
                GetRequestDateTime(availableChargeLinksData, requestDateTime),
                availableChargeLinksData.AvailableDataReferenceId,
                availableChargeLinksData.ChargeId[..34],
                availableChargeLinksData.ChargeOwner[..34],
                availableChargeLinksData.ChargeType,
                availableChargeLinksData.MeteringPointId[..49],
                availableChargeLinksData.Factor,
                availableChargeLinksData.StartDateTime,
                availableChargeLinksData.EndDateTime,
                availableChargeLinksData.DocumentType,
                GetOperationOrder(availableChargeLinksData, operationOrder),
                SeededData.MarketParticipants.SystemOperator.Id);
        }

        private static AvailableChargeLinksReceiptData CreateAvailableDataBasedOn(
            AvailableChargeLinksReceiptData availableChargeLinksReceiptData,
            Instant? requestDateTime = default,
            int? operationOrder = default)
        {
            return new AvailableChargeLinksReceiptData(
                availableChargeLinksReceiptData.SenderId[..34],
                availableChargeLinksReceiptData.SenderRole,
                availableChargeLinksReceiptData.RecipientId[..34],
                availableChargeLinksReceiptData.RecipientRole,
                availableChargeLinksReceiptData.BusinessReasonCode,
                GetRequestDateTime(availableChargeLinksReceiptData, requestDateTime),
                availableChargeLinksReceiptData.AvailableDataReferenceId,
                availableChargeLinksReceiptData.ReceiptStatus,
                availableChargeLinksReceiptData.OriginalOperationId[..34],
                availableChargeLinksReceiptData.MeteringPointId[..49],
                availableChargeLinksReceiptData.DocumentType,
                GetOperationOrder(availableChargeLinksReceiptData, operationOrder),
                SeededData.MarketParticipants.SystemOperator.Id,
                availableChargeLinksReceiptData.ValidationErrors.ToList());
        }

        private static AvailableChargePriceData CreateAvailableDataBasedOn(
            AvailableChargePriceData availableChargePriceData,
            Instant? requestDateTime = default,
            int? operationOrder = default)
        {
            return new AvailableChargePriceData(
                availableChargePriceData.SenderId[..34],
                availableChargePriceData.SenderRole,
                availableChargePriceData.RecipientId[..34],
                availableChargePriceData.RecipientRole,
                availableChargePriceData.BusinessReasonCode,
                GetRequestDateTime(availableChargePriceData, requestDateTime),
                availableChargePriceData.AvailableDataReferenceId,
                availableChargePriceData.ChargeId[..34],
                availableChargePriceData.ChargeOwner[..34],
                availableChargePriceData.ChargeType,
                availableChargePriceData.StartDateTime,
                availableChargePriceData.Resolution,
                availableChargePriceData.DocumentType,
                GetOperationOrder(availableChargePriceData, operationOrder),
                SeededData.MarketParticipants.SystemOperator.Id,
                availableChargePriceData.Points.ToList());
        }

        private static int GetOperationOrder(AvailableDataBase availableData, int? operationOrder)
        {
            return operationOrder ?? availableData.OperationOrder;
        }

        private static Instant GetRequestDateTime(AvailableDataBase availableData, Instant? requestDateTime)
        {
            return requestDateTime ?? availableData.RequestDateTime;
        }
    }
}
