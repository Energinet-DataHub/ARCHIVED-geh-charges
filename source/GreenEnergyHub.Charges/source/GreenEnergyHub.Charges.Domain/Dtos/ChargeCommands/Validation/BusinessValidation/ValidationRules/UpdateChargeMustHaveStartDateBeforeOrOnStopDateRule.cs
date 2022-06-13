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

using System.Linq;
using GreenEnergyHub.Charges.Domain.Charges;
using GreenEnergyHub.Charges.Domain.Dtos.Validation;
using NodaTime;

namespace GreenEnergyHub.Charges.Domain.Dtos.ChargeCommands.Validation.BusinessValidation.ValidationRules
{
    public class UpdateChargeMustHaveStartDateBeforeOrOnStopDateRule : IValidationRule
    {
        private readonly Instant _lastPeriodEndDateOnExistingCharge;
        private readonly Instant _incomingStartDateDate;

        public UpdateChargeMustHaveStartDateBeforeOrOnStopDateRule(
            Instant existingLastPeriod,
            Instant incomingStartDateDate)
        {
            _lastPeriodEndDateOnExistingCharge = existingLastPeriod;
            _incomingStartDateDate = incomingStartDateDate;
        }

        public ValidationRuleIdentifier ValidationRuleIdentifier =>
            ValidationRuleIdentifier.UpdateChargeMustHaveEffectiveDateBeforeOrOnStopDate;

        public bool IsValid => _incomingStartDateDate <= _lastPeriodEndDateOnExistingCharge;
    }
}
