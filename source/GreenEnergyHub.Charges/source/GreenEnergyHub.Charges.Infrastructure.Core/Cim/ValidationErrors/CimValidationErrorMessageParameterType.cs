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

using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommands.Validation;

namespace GreenEnergyHub.Charges.Infrastructure.Core.Cim.ValidationErrors
{
    public enum CimValidationErrorMessageParameterType
    {
        /// <summary>
        /// Corresponds to <see cref="ValidationErrorMessageParameterType.ChargeType"/>
        /// </summary>
        ChargeType = 1,

        /// <summary>
        /// Corresponds to <see cref="ValidationErrorMessageParameterType.ChargeOwner"/>
        /// </summary>
        ChargeTypeOwner = 2,

        /// <summary>
        /// Corresponds to <see cref="ValidationErrorMessageParameterType.ChargeName"/>
        /// </summary>
        Description = 3,

        /// <summary>
        /// Corresponds to <see cref="ValidationErrorMessageParameterType.ChargeDescription"/>
        /// </summary>
        LongDescriptionMaxLength100 = 4,

        /// <summary>
        /// Corresponds to <see cref="ValidationErrorMessageParameterType.ChargeStartDateTime"/>
        /// </summary>
        Occurrence = 5,

        /// <summary>
        /// Corresponds to <see cref="ValidationErrorMessageParameterType.ChargeResolution"/>
        /// </summary>
        ResolutionDuration = 6,

        /// <summary>
        /// Corresponds to <see cref="ValidationErrorMessageParameterType.ChargeTaxIndicator"/>
        /// </summary>
        TaxIndicator = 7,

        /// <summary>
        /// Corresponds to <see cref="ValidationErrorMessageParameterType.ChargeVatClass"/>
        /// </summary>
        VatClass = 8,

        /// <summary>
        /// Corresponds to <see cref="ValidationErrorMessageParameterType.ChargePointsCount"/>
        /// </summary>
        MaxOfPosition = 9,

        /// <summary>
        /// Corresponds to <see cref="ValidationErrorMessageParameterType.ChargePointPosition"/>
        /// </summary>
        Position = 10,

        /// <summary>
        /// Corresponds to <see cref="ValidationErrorMessageParameterType.ChargePointPrice"/>
        /// </summary>
        EnergyPrice = 11,

        /// <summary>
        /// Corresponds to <see cref="ValidationErrorMessageParameterType.DocumentType"/>
        /// </summary>
        DocumentType = 12,

        /// <summary>
        /// Corresponds to <see cref="ValidationErrorMessageParameterType.DocumentBusinessReasonCode"/>
        /// </summary>
        EnergyBusinessProcess = 13,

        /// <summary>
        /// Corresponds to <see cref="ValidationErrorMessageParameterType.DocumentId"/>
        /// </summary>
        MessageId = 14,

        /// <summary>
        /// Corresponds to <see cref="ValidationErrorMessageParameterType.DocumentSenderProvidedChargeId"/>
        /// </summary>
        PartyChargeTypeId = 15,

        /// <summary>
        /// Corresponds to <see cref="ValidationErrorMessageParameterType.DocumentSenderId"/>
        /// </summary>
        SenderId = 16,
    }
}
