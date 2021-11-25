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
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using GreenEnergyHub.Charges.Domain.AvailableChargeData;
using GreenEnergyHub.Charges.Domain.AvailableChargeLinkReceiptData;
using GreenEnergyHub.Charges.Domain.AvailableChargeReceiptData;

namespace GreenEnergyHub.Charges.IntegrationTests.IntegrationTests.Repositories
{
    public static class RepositoryAutoMoqDataFixer
    {
        public static AvailableChargeData GetAvailableChargeDataBasedOn([NotNull] AvailableChargeData availableChargeData)
        {
            return new AvailableChargeData(
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
                availableChargeData.Points.ToList());
        }

        public static AvailableChargeReceiptData GetAvailableChargeReceiptDataBasedOn(
            AvailableChargeReceiptData availableChargeReceiptData)
        {
            return new AvailableChargeReceiptData(
                availableChargeReceiptData.RecipientId.Substring(0, 34),
                availableChargeReceiptData.RecipientRole,
                availableChargeReceiptData.BusinessReasonCode,
                availableChargeReceiptData.RequestDateTime,
                availableChargeReceiptData.AvailableDataReferenceId,
                availableChargeReceiptData.ReceiptStatus,
                availableChargeReceiptData.OriginalOperationId.Substring(0, 34),
                availableChargeReceiptData.ReasonCodes.ToList());
        }

        public static List<AvailableChargeReceiptData> GetAvailableChargeReceiptDataListBasedOn(
            List<AvailableChargeReceiptData> availableList)
        {
            return availableList.Select(receipt => GetAvailableChargeReceiptDataBasedOn(receipt)).ToList();
        }

        public static AvailableChargeLinkReceiptData GetAvailableChargeLinkReceiptDataBasedOn(
            AvailableChargeLinkReceiptData availableChargeLinkReceiptData)
        {
            return new AvailableChargeLinkReceiptData(
                availableChargeLinkReceiptData.RecipientId.Substring(0, 34),
                availableChargeLinkReceiptData.RecipientRole,
                availableChargeLinkReceiptData.BusinessReasonCode,
                availableChargeLinkReceiptData.RequestDateTime,
                availableChargeLinkReceiptData.AvailableDataReferenceId,
                availableChargeLinkReceiptData.ReceiptStatus,
                availableChargeLinkReceiptData.OriginalOperationId.Substring(0, 34),
                availableChargeLinkReceiptData.MeteringPointId.Substring(0, 49),
                availableChargeLinkReceiptData.ReasonCodes.ToList());
        }

        public static List<AvailableChargeLinkReceiptData> GetAvailableChargeLinkReceiptDataListBasedOn(
            List<AvailableChargeLinkReceiptData> availableList)
        {
            return availableList.Select(receipt => GetAvailableChargeLinkReceiptDataBasedOn(receipt)).ToList();
        }
    }
}
