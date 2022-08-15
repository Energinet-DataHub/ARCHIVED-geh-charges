﻿// Copyright 2020 Energinet DataHub A/S
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

using GreenEnergyHub.Charges.Domain.Dtos.ChargePriceCommands;
using GreenEnergyHub.Charges.Domain.Dtos.Validation;
using GreenEnergyHub.Charges.MessageHub.Models.AvailableData;
using GreenEnergyHub.Charges.MessageHub.Models.Shared;

namespace GreenEnergyHub.Charges.MessageHub.Models.AvailableOperationReceiptData
{
    public class AvailableChargeReceiptValidationErrorFactory : IAvailableOperationReceiptValidationErrorFactory
    {
        private readonly ICimValidationErrorCodeFactory _cimValidationErrorCodeFactory;
        private readonly ICimValidationErrorTextFactory<ChargePriceCommand, ChargePriceOperationDto> _cimValidationErrorTextFactory;

        public AvailableChargeReceiptValidationErrorFactory(
            ICimValidationErrorCodeFactory cimValidationErrorCodeFactory,
            ICimValidationErrorTextFactory<ChargePriceCommand, ChargePriceOperationDto> cimValidationErrorTextFactory)
        {
            _cimValidationErrorCodeFactory = cimValidationErrorCodeFactory;
            _cimValidationErrorTextFactory = cimValidationErrorTextFactory;
        }

        public AvailableReceiptValidationError Create(
            ValidationError validationError,
            ChargePriceCommand command,
            ChargePriceOperationDto chargePriceOperationDto)
        {
            var reasonCode = _cimValidationErrorCodeFactory.Create(validationError.ValidationRuleIdentifier);
            var reasonText = _cimValidationErrorTextFactory.Create(validationError, command, chargePriceOperationDto);

            return new AvailableReceiptValidationError(reasonCode, reasonText);
        }
    }
}
