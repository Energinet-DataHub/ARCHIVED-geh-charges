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

namespace GreenEnergyHub.Charges.Infrastructure.Core.Cim.ValidationErrors
{
    public enum CimValidationErrorDescriptionToken
    {
        ChargeDescription = 1,
        ChargeName = 2,
        ChargeOwner = 3,
        ChargePointPosition = 4,
        ChargePointPrice = 5,
        ChargePointsCount = 6,
        ChargeResolution = 7,
        ChargeStartDateTime = 8,
        ChargeTaxIndicator = 9,
        ChargeType = 10,
        ChargeVatClass = 11,
        DocumentBusinessReasonCode = 12,
        DocumentId = 13,
        DocumentSenderId = 14,
        DocumentSenderProvidedChargeId = 15,
        DocumentType = 16,
    }
}
