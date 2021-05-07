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

using System.Diagnostics.CodeAnalysis;
using GreenEnergyHub.Charges.Core.DateTime;
using GreenEnergyHub.Charges.Domain.ChangeOfCharges.Transaction;
using NodaTime;

namespace GreenEnergyHub.Charges.Application.Validation.BusinessValidation.ValidationRules
{
    public class StartDateValidationRule : IValidationRule
    {
        private readonly Instant _validityStartDate;
        private readonly Instant _periodStart;
        private readonly Instant _periodEnd;

        public StartDateValidationRule(
            [NotNull] ChargeCommand command,
            [NotNull] StartDateValidationRuleConfiguration configuration,
            [NotNull] IZonedDateTimeService zonedDateTimeService)
        {
            _validityStartDate = command.ChargeOperation.StartDateTime;

            var today = zonedDateTimeService.GetZonedDateTime(command.Document.RequestDate).Date;
            _periodStart = CalculatePeriodPoint(configuration.ValidIntervalFromNowInDays.Start, zonedDateTimeService, today);
            _periodEnd = CalculatePeriodPoint(configuration.ValidIntervalFromNowInDays.End + 1, zonedDateTimeService, today);
        }

        public bool IsValid => _validityStartDate >= _periodStart && _validityStartDate < _periodEnd;

        public ValidationRuleIdentifier ValidationRuleIdentifier => ValidationRuleIdentifier.TimeLimitsNotFollowed;

        private static Instant CalculatePeriodPoint(
            int numberOfDays,
            IZonedDateTimeService zonedDateTimeService,
            LocalDate today)
        {
            var localDate = today.Plus(Period.FromDays(numberOfDays));
            return zonedDateTimeService
                .GetZonedDateTime(localDate.AtMidnight(), ResolutionStrategy.Leniently)
                .ToInstant();
        }
    }
}
