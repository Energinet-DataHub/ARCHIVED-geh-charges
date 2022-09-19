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

namespace GreenEnergyHub.Charges.IntegrationTests.IntegrationTests.Repositories
{
    public static class RepositoryAutoMoqDataFixer
    {
        public static List<AvailableDataBase> GetAvailableDataListBasedOn(
            List<AvailableDataBase> availableList)
        {
            return availableList
                .Select(availableData => (AvailableDataBase)GetAvailableDataBasedOn((dynamic)availableData))
                .ToList();
        }

        public static AvailableDataBase GetAvailableDataBasedOn(AvailableDataBase availableData)
        {
            return GetAvailableDataBasedOn((dynamic)availableData);
        }

        private static AvailableChargeData GetAvailableDataBasedOn(AvailableChargeData availableChargeData)
        {
            return new AvailableChargeData(
                availableChargeData.SenderId.Substring(0, 34),
                availableChargeData.SenderRole,
                availableChargeData.RecipientId.Substring(0, 34),
                availableChargeData.RecipientRole,
                availableChargeData.BusinessReasonCode,
                availableChargeData.RequestDateTime,
                availableChargeData.AvailableDataReferenceId,
                availableChargeData.ChargeId.Substring(0, 34),
                availableChargeData.ChargeOwner.Substring(0, 34),
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
                availableChargeData.OperationOrder,
                SeededData.MarketParticipants.SystemOperator.Id);
        }

        private static AvailableChargeReceiptData GetAvailableDataBasedOn(
            AvailableChargeReceiptData availableChargeReceiptData)
        {
            return new AvailableChargeReceiptData(
                availableChargeReceiptData.SenderId.Substring(0, 34),
                availableChargeReceiptData.SenderRole,
                availableChargeReceiptData.RecipientId.Substring(0, 34),
                availableChargeReceiptData.RecipientRole,
                availableChargeReceiptData.BusinessReasonCode,
                availableChargeReceiptData.RequestDateTime,
                availableChargeReceiptData.AvailableDataReferenceId,
                availableChargeReceiptData.ReceiptStatus,
                availableChargeReceiptData.OriginalOperationId.Substring(0, 34),
                availableChargeReceiptData.DocumentType,
                availableChargeReceiptData.OperationOrder,
                SeededData.MarketParticipants.SystemOperator.Id,
                availableChargeReceiptData.ValidationErrors.ToList());
        }

        private static AvailableChargeLinksData GetAvailableDataBasedOn(AvailableChargeLinksData availableChargeData)
        {
            return new AvailableChargeLinksData(
                availableChargeData.SenderId.Substring(0, 34),
                availableChargeData.SenderRole,
                availableChargeData.RecipientId.Substring(0, 34),
                availableChargeData.RecipientRole,
                availableChargeData.BusinessReasonCode,
                availableChargeData.RequestDateTime,
                availableChargeData.AvailableDataReferenceId,
                availableChargeData.ChargeId.Substring(0, 34),
                availableChargeData.ChargeOwner.Substring(0, 34),
                availableChargeData.ChargeType,
                availableChargeData.MeteringPointId.Substring(0, 49),
                availableChargeData.Factor,
                availableChargeData.StartDateTime,
                availableChargeData.EndDateTime,
                availableChargeData.DocumentType,
                availableChargeData.OperationOrder,
                SeededData.MarketParticipants.SystemOperator.Id);
        }

        private static AvailableChargeLinksReceiptData GetAvailableDataBasedOn(
            AvailableChargeLinksReceiptData availableChargeLinksReceiptData)
        {
            return new AvailableChargeLinksReceiptData(
                availableChargeLinksReceiptData.SenderId.Substring(0, 34),
                availableChargeLinksReceiptData.SenderRole,
                availableChargeLinksReceiptData.RecipientId.Substring(0, 34),
                availableChargeLinksReceiptData.RecipientRole,
                availableChargeLinksReceiptData.BusinessReasonCode,
                availableChargeLinksReceiptData.RequestDateTime,
                availableChargeLinksReceiptData.AvailableDataReferenceId,
                availableChargeLinksReceiptData.ReceiptStatus,
                availableChargeLinksReceiptData.OriginalOperationId.Substring(0, 34),
                availableChargeLinksReceiptData.MeteringPointId.Substring(0, 49),
                availableChargeLinksReceiptData.DocumentType,
                availableChargeLinksReceiptData.OperationOrder,
                SeededData.MarketParticipants.SystemOperator.Id,
                availableChargeLinksReceiptData.ValidationErrors.ToList());
        }

        private static AvailableChargePriceData GetAvailableDataBasedOn(
            AvailableChargePriceData availableChargePriceData)
        {
            return new AvailableChargePriceData(
                availableChargePriceData.SenderId.Substring(0, 34),
                availableChargePriceData.SenderRole,
                availableChargePriceData.RecipientId.Substring(0, 34),
                availableChargePriceData.RecipientRole,
                availableChargePriceData.BusinessReasonCode,
                availableChargePriceData.RequestDateTime,
                availableChargePriceData.AvailableDataReferenceId,
                availableChargePriceData.ChargeId.Substring(0, 34),
                availableChargePriceData.ChargeOwner.Substring(0, 34),
                availableChargePriceData.ChargeType,
                availableChargePriceData.StartDateTime,
                availableChargePriceData.Resolution,
                availableChargePriceData.DocumentType,
                availableChargePriceData.OperationOrder,
                SeededData.MarketParticipants.SystemOperator.Id,
                availableChargePriceData.Points.ToList());
        }
    }
}
