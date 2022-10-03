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

using GreenEnergyHub.Charges.Domain.Dtos.Validation;
using NodaTime;

namespace GreenEnergyHub.Charges.Domain.Dtos.ChargePriceCommands.Validation.BusinessValidation.ValidationRules
{
    public class MonthlyPriceSeriesEndDateMustBeFirstOfMonthRule : IValidationRule
    {
        private readonly Instant _priceEndDate;
        private readonly Instant? _stopDate;

        public MonthlyPriceSeriesEndDateMustBeFirstOfMonthRule(Instant priceEndDate, Instant? stopDate)
        {
            _priceEndDate = priceEndDate;
            _stopDate = stopDate;
        }

        public bool IsValid => _priceEndDate == _stopDate || _priceEndDate.InUtc().Day is 1;

        public ValidationRuleIdentifier ValidationRuleIdentifier { get; }
    }
}
