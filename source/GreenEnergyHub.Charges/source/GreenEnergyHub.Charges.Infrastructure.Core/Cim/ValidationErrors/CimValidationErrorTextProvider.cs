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
using System.Linq;
using System.Reflection;
using GreenEnergyHub.Charges.Domain.Dtos.Validation;

namespace GreenEnergyHub.Charges.Infrastructure.Core.Cim.ValidationErrors
{
    public class CimValidationErrorTextProvider : ICimValidationErrorTextProvider
    {
        public string GetCimValidationErrorText(ValidationRuleIdentifier validationRuleIdentifier)
        {
            var fields = typeof(CimValidationErrorTextTemplateMessages).GetFields();
            foreach (var field in fields)
            {
                var attribute = (ErrorMessageForAttribute)field.GetCustomAttributes()
                    .Single(x => x.GetType() == typeof(ErrorMessageForAttribute));
                var isFound = attribute.ValidationRuleIdentifier == validationRuleIdentifier;
                if (isFound) return field.GetValue(null)!.ToString()!;
            }

            throw new ArgumentOutOfRangeException(validationRuleIdentifier.ToString());
        }
    }
}
