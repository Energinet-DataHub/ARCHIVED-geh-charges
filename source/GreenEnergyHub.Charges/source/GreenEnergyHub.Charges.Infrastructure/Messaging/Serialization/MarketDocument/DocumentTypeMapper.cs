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

namespace GreenEnergyHub.Charges.Infrastructure.Messaging.Serialization.MarketDocument
{
    public static class DocumentTypeMapper
    {
        public static DocumentType Map(string value)
        {
            return value switch
            {
                "D05" => DocumentType.RequestChangeBillingMasterData,
                "D07" => DocumentType.NotifyBillingMasterData,
                "D10" => DocumentType.RequestUpdateChargeInformation,
                _ => DocumentType.Unknown,
            };
        }

        public static string Map(DocumentType documentType)
        {
            return documentType switch
            {
                DocumentType.NotifyBillingMasterData => "D07",
                DocumentType.RequestUpdateChargeInformation => "D10",
                DocumentType.RequestChangeBillingMasterData => "D05",
                _ => throw new InvalidEnumArgumentException($"Provided DocumentType value '{documentType}' is invalid and cannot be mapped.")
            };
        }
    }
}
