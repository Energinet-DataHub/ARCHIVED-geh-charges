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

namespace GreenEnergyHub.Charges.Domain.Dtos.Validation
{
    public enum ValidationErrorMessageParameterType
    {
        ChargeType = 1,                         // CIMValidationErrorMessageParameterType.ChargeType
        ChargeOwner = 2,                        // CIMValidationErrorMessageParameterType.ChargeTypeOwner
        ChargeName = 3,                         // CIMValidationErrorMessageParameterType.Description
        ChargeDescription = 4,                  // CIMValidationErrorMessageParameterType.LongDescriptionMaxLength100
        ChargeStartDateTime = 5,                // CIMValidationErrorMessageParameterType.Occurrence
        ChargeResolution = 6,                   // CIMValidationErrorMessageParameterType.ResolutionDuration
        ChargeTaxIndicator = 7,                 // CIMValidationErrorMessageParameterType.TaxIndicator
        ChargeVatClass = 8,                     // CIMValidationErrorMessageParameterType.VatClass
        ChargePointsCount = 9,                  // CIMValidationErrorMessageParameterType.MaxOfPosition
        ChargePointPosition = 10,               // CIMValidationErrorMessageParameterType.Position
        ChargePointPrice = 11,                  // CIMValidationErrorMessageParameterType.EnergyPrice
        DocumentType = 12,                      // CIMValidationErrorMessageParameterType.DocumentType
        DocumentBusinessReasonCode = 13,        // CIMValidationErrorMessageParameterType.EnergyBusinessProcess
        DocumentId = 14,                        // CIMValidationErrorMessageParameterType.MessageId
        DocumentSenderProvidedChargeId = 15,    // CIMValidationErrorMessageParameterType.PartyChargeTypeId
        DocumentSenderId = 16,                  // CIMValidationErrorMessageParameterType.SenderId
    }
}
