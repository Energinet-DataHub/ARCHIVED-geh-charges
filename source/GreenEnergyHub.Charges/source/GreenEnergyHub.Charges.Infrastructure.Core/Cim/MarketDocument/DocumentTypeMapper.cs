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

using System.ComponentModel;
using GreenEnergyHub.Charges.Domain.MarketParticipants;

namespace GreenEnergyHub.Charges.Infrastructure.Core.Cim.MarketDocument
{
    public static class DocumentTypeMapper
    {
        // These values are ebix values which are used temporarily until CIM code lists are available
        private const string CimChargeLinkReceipt = "D06";
        private const string CimNotifyBillingMasterData = "D07";
        private const string CimChargeReceipt = "D11";
        private const string CimNotifyPriceList = "D12";
        private const string CimRequestChangeBillingMasterData = "D05";
        private const string CimRequestChangeOfPriceList = "D10";

        public static DocumentType Map(string value)
        {
            return value switch
            {
                CimChargeLinkReceipt => DocumentType.ChargeLinkReceipt,
                CimNotifyBillingMasterData => DocumentType.NotifyBillingMasterData,
                CimChargeReceipt => DocumentType.ChargeReceipt,
                CimNotifyPriceList => DocumentType.NotifyPriceList,
                CimRequestChangeOfPriceList => DocumentType.RequestChangeOfPriceList,
                CimRequestChangeBillingMasterData => DocumentType.RequestChangeBillingMasterData,
                _ => DocumentType.Unknown,
            };
        }

        public static string Map(DocumentType documentType)
        {
            return documentType switch
            {
                DocumentType.ChargeLinkReceipt => CimChargeLinkReceipt,
                DocumentType.NotifyBillingMasterData => CimNotifyBillingMasterData,
                DocumentType.ChargeReceipt => CimChargeReceipt,
                DocumentType.NotifyPriceList => CimNotifyPriceList,
                DocumentType.RequestChangeOfPriceList => CimRequestChangeOfPriceList,
                DocumentType.RequestChangeBillingMasterData => CimRequestChangeBillingMasterData,
                _ => throw new InvalidEnumArgumentException($"Provided DocumentType value '{documentType}' is invalid and cannot be mapped."),
            };
        }
    }
}
