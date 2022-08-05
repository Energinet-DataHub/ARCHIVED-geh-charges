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

namespace GreenEnergyHub.Charges.Domain.Dtos.ChargeInformationCommands.Validation.InputValidation.ValidationRules
{
    public class PriceListMustStartAndStopAtMidnightValidationRule : IValidationRule
    {
        private readonly IZonedDateTimeService _zonedDateTimeService;
        private readonly ChargeInformationOperationDto _chargeInformationOperation;

        public PriceListMustStartAndStopAtMidnightValidationRule(
            IZonedDateTimeService zonedDateTimeService,
            ChargeInformationOperationDto chargeInformationOperation)
        {
            _zonedDateTimeService = zonedDateTimeService;
            _chargeInformationOperation = chargeInformationOperation;
        }

        public bool IsValid => (GetPointsIntervalStartDateTime() is null ||
                                GetPointsIntervalStartDateTime().GetValueOrDefault().TickOfDay == 0) &&
                               (GetPointsIntervalEndDateTime() is null ||
                                GetPointsIntervalEndDateTime().GetValueOrDefault().TickOfDay == 0);

        public ValidationRuleIdentifier ValidationRuleIdentifier =>
            ValidationRuleIdentifier.PriceListMustStartAndStopAtMidnightValidationRule;

        private ZonedDateTime? GetPointsIntervalStartDateTime()
        {
            return _chargeInformationOperation.PointsStartInterval is null
                ? null
                : _zonedDateTimeService.GetZonedDateTime(_chargeInformationOperation.PointsStartInterval.GetValueOrDefault());
        }

        private ZonedDateTime? GetPointsIntervalEndDateTime()
        {
            return _chargeInformationOperation.PointsEndInterval is null
                ? null
                : _zonedDateTimeService.GetZonedDateTime(_chargeInformationOperation.PointsEndInterval.GetValueOrDefault());
        }
    }
}
