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

using System;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommands;
using GreenEnergyHub.Charges.Domain.Dtos.Validation;
using GreenEnergyHub.Charges.MessageHub.Models.AvailableData;
using GreenEnergyHub.Charges.MessageHub.Models.Shared;

namespace GreenEnergyHub.Charges.MessageHub.Models.AvailableChargeReceiptData
{
    public class AvailableChargeReceiptValidationErrorFactory : IAvailableChargeReceiptValidationErrorFactory
    {
        private readonly ICimValidationErrorCodeFactory _cimValidationErrorCodeFactory;
        private readonly ICimValidationErrorTextFactory<ChargeCommand, ChargeInformationDto> _chargeInformationCimValidationErrorTextFactory;
        private readonly ICimValidationErrorTextFactory<ChargeCommand, ChargePriceDto> _chargePriceCimValidationErrorTextFactory;

        public AvailableChargeReceiptValidationErrorFactory(
            ICimValidationErrorCodeFactory cimValidationErrorCodeFactory,
            ICimValidationErrorTextFactory<ChargeCommand, ChargeInformationDto> chargeInformationCimValidationErrorTextFactory,
            ICimValidationErrorTextFactory<ChargeCommand, ChargePriceDto> chargePriceCimValidationErrorTextFactory)
        {
            _cimValidationErrorCodeFactory = cimValidationErrorCodeFactory;
            _chargeInformationCimValidationErrorTextFactory = chargeInformationCimValidationErrorTextFactory;
            _chargePriceCimValidationErrorTextFactory = chargePriceCimValidationErrorTextFactory;
        }

        public AvailableReceiptValidationError Create(
            ValidationError validationError,
            ChargeCommand command,
            ChargeOperation chargeOperation)
        {
            var reasonCode = _cimValidationErrorCodeFactory.Create(validationError.ValidationRuleIdentifier);

            var reasonText = chargeOperation switch
            {
                ChargeInformationDto chargeInformationDto =>
                    _chargeInformationCimValidationErrorTextFactory.Create(validationError, command, chargeInformationDto),

                ChargePriceDto chargePriceDto =>
                    _chargePriceCimValidationErrorTextFactory.Create(validationError, command, chargePriceDto),

                _ => throw new InvalidOperationException(
                    $"Operation must be {nameof(ChargeInformationDto)} or {nameof(ChargePriceDto)}"),
            };

            return new AvailableReceiptValidationError(reasonCode, reasonText);
        }
    }
}
