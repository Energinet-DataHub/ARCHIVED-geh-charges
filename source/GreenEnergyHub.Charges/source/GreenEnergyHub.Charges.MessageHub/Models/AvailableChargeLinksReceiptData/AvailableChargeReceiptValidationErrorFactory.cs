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

using GreenEnergyHub.Charges.Domain.Dtos.ChargeLinksCommands;
using GreenEnergyHub.Charges.Domain.Dtos.Validation;
using GreenEnergyHub.Charges.MessageHub.Models.AvailableChargeReceiptData;
using GreenEnergyHub.Charges.MessageHub.Models.AvailableData;

namespace GreenEnergyHub.Charges.MessageHub.Models.AvailableChargeLinksReceiptData
{
    public class AvailableChargeLinksReceiptValidationErrorFactory : IAvailableChargeLinksReceiptValidationErrorFactory
    {
        private readonly ICimValidationErrorCodeFactory _cimValidationErrorCodeFactory;
        private readonly ICimValidationErrorTextFactory<ChargeLinksCommand> _cimValidationErrorTextFactory;

        public AvailableChargeLinksReceiptValidationErrorFactory(
            ICimValidationErrorCodeFactory cimValidationErrorCodeFactory,
            ICimValidationErrorTextFactory<ChargeLinksCommand> cimValidationErrorTextFactory)
        {
            _cimValidationErrorCodeFactory = cimValidationErrorCodeFactory;
            _cimValidationErrorTextFactory = cimValidationErrorTextFactory;
        }

        public AvailableReceiptValidationError Create(ValidationError validationError, ChargeLinksCommand command)
        {
            var reasonCode = _cimValidationErrorCodeFactory.Create(validationError.ValidationRuleIdentifier);
            var reasonText = _cimValidationErrorTextFactory.Create(validationError, command);

            return new AvailableReceiptValidationError(reasonCode, reasonText);
        }
    }
}
