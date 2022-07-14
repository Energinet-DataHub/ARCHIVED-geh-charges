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

using GreenEnergyHub.Charges.Core.DateTime;
using GreenEnergyHub.Charges.Domain.Dtos.Validation;
using NodaTime;

namespace GreenEnergyHub.Charges.Domain.Dtos.ChargeCommands.Validation.InputValidation.ValidationRules
{
    public class PriceListMustStartAndStopAtMidnightValidationRule : IValidationRule
    {
        private readonly IZonedDateTimeService _zonedDateTimeService;
        private readonly ChargeOperationDto _chargeOperation;

        public PriceListMustStartAndStopAtMidnightValidationRule(
            IZonedDateTimeService zonedDateTimeService,
            ChargeOperationDto chargeOperation)
        {
            _zonedDateTimeService = zonedDateTimeService;
            _chargeOperation = chargeOperation;
        }

        public bool IsValid => (GetPointsIntervalStartDateTime() is null ||
                                GetPointsIntervalStartDateTime().GetValueOrDefault().TickOfDay == 0) &&
                               (GetPointsIntervalEndDateTime() is null ||
                                GetPointsIntervalEndDateTime().GetValueOrDefault().TickOfDay == 0);

        public ValidationRuleIdentifier ValidationRuleIdentifier =>
            ValidationRuleIdentifier.PriceListMustStartAndStopAtMidnightValidationRule;

        private ZonedDateTime? GetPointsIntervalStartDateTime()
        {
            return _chargeOperation.PointsStartInterval is null
                ? null
                : _zonedDateTimeService.GetZonedDateTime(_chargeOperation.PointsStartInterval.GetValueOrDefault());
        }

        private ZonedDateTime? GetPointsIntervalEndDateTime()
        {
            return _chargeOperation.EndDateTime is null
                ? null
                : _zonedDateTimeService.GetZonedDateTime(_chargeOperation.PointsEndInterval.GetValueOrDefault());
        }
    }
}
