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
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommands.Validation;
using GreenEnergyHub.Charges.Infrastructure.Core.Cim.ValidationErrors;

namespace GreenEnergyHub.Charges.MessageHub.Models.AvailableChargeReceiptData
{
    public class CimValidationErrorTextFactory : ICimValidationErrorTextFactory
    {
        private readonly ICimValidationErrorMessageProvider _cimValidationErrorMessageProvider;

        public CimValidationErrorTextFactory(ICimValidationErrorMessageProvider cimValidationErrorMessageProvider)
        {
            _cimValidationErrorMessageProvider = cimValidationErrorMessageProvider;
        }

        public string Create(ValidationError validationError)
        {
            return GetMergedErrorMessage(validationError);
        }

        private string GetMergedErrorMessage(ValidationError validationError)
        {
            var errorMessage = _cimValidationErrorMessageProvider
                .GetCimValidationErrorMessage(validationError.ValidationRuleIdentifier);

            var mergedErrorMessage =
                MergeErrorMessage(errorMessage, validationError.ValidationErrorMessageParameters);

            return mergedErrorMessage;
        }

        private string MergeErrorMessage(
            string errorMessage,
            List<ValidationErrorMessageParameter> validationErrorMessageParameters)
        {
            var index = 1;
            foreach (var validationErrorMessageParameter in validationErrorMessageParameters)
            {
                errorMessage = errorMessage.Replace(
                    $"{{{{$mergeField{index}}}}}",
                    validationErrorMessageParameter.ParameterValue);

                index++;
            }

            return errorMessage;
        }
    }
}
