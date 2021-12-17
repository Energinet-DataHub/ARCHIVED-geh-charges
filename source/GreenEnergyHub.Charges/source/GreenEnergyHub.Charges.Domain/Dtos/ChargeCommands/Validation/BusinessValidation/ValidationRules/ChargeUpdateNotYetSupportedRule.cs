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

using GreenEnergyHub.Charges.Domain.Charges;

namespace GreenEnergyHub.Charges.Domain.Dtos.ChargeCommands.Validation.BusinessValidation.ValidationRules
{
    /// <summary>
    /// Temporary rule that stops both update and stops from taking place to charges until that is implemented
    /// </summary>
    public class ChargeUpdateNotYetSupportedRule : IValidationRule
    {
        private readonly Charge? _charge;

        public ChargeUpdateNotYetSupportedRule(Charge? charge)
        {
            _charge = charge;
        }

        public bool IsValid => _charge == null;

        public ValidationError ValidationError { get; } = new(ValidationRuleIdentifier.UpdateNotYetSupported);
    }
}
