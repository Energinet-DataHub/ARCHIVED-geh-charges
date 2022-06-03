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
using System.Linq;
using System.Reflection;
using GreenEnergyHub.Charges.Domain.Dtos.Validation;
using GreenEnergyHub.Charges.Infrastructure.Core.Cim.ValidationErrors;
using GreenEnergyHub.Charges.MessageHub.Models.Shared;
using Xunit;

namespace GreenEnergyHub.Charges.Tests.Domain.Dtos.ChargeCommands.Validation.BusinessValidation
{
    public static class CimValidationErrorMessageAssertionHelper
    {
        public static void AssertAllRulesThatNeedTriggeredByForErrorMessageImplementsIValidationRuleWithExtendedData(
            CimValidationErrorTextToken cimValidationErrorTextToken,
            IReadOnlyCollection<IValidationRuleContainer> validationRules)
        {
            var type = typeof(CimValidationErrorTextTemplateMessages);
            foreach (var fieldInfo in type.GetFields(BindingFlags.Static | BindingFlags.Public))
            {
                if (!fieldInfo.GetCustomAttributes().Any()) continue;

                var errorMessageForAttribute = (ErrorMessageForAttribute)fieldInfo.GetCustomAttributes()
                    .Single(x => x.GetType() == typeof(ErrorMessageForAttribute));

                var validationRuleIdentifier = errorMessageForAttribute.ValidationRuleIdentifier;
                var errorText = fieldInfo.GetValue(null)!.ToString();
                var validationErrorTextTokens = CimValidationErrorTextTokenMatcher.GetTokens(errorText!);
                var validationRuleContainer = validationRules
                    .FirstOrDefault(x => x.ValidationRule.ValidationRuleIdentifier == validationRuleIdentifier);

                if (validationErrorTextTokens.Contains(cimValidationErrorTextToken) && validationRuleContainer != null)
                    Assert.True(validationRuleContainer.ValidationRule is IValidationRuleWithExtendedData);
            }
        }
    }
}
