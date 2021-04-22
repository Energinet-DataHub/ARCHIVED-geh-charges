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

using System.Diagnostics.CodeAnalysis;
using GreenEnergyHub.Charges.Domain.ChangeOfCharges.Transaction;
using NodaTime;

namespace GreenEnergyHub.Charges.Application.Validation.BusinessValidation.Rules
{
    public class StartDateVr209ValidationRule : IBusinessValidationRule
    {
        private readonly Instant _startOfValidInterval;
        private readonly Instant _endOfValidInterval;
        private readonly Instant _validityStartDate;

        public StartDateVr209ValidationRule([NotNull]ChargeCommand command, [NotNull]StartDateVr209ValidationRuleConfiguration configuration)
        {
            _validityStartDate = command.MktActivityRecord!.ValidityStartDate;

            _startOfValidInterval = _validityStartDate.Plus(Duration.FromDays(configuration.ValidIntervalFromNowInDays.Start));
            _endOfValidInterval = _validityStartDate.Plus(Duration.FromDays(configuration.ValidIntervalFromNowInDays.End));
        }

        public bool IsValid => _validityStartDate >= _startOfValidInterval && _validityStartDate <= _endOfValidInterval;
    }
}
