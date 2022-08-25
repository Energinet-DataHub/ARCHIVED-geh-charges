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

namespace GreenEnergyHub.Charges.Domain.Dtos.ChargePriceCommands.Validation.InputValidation.ValidationRules
{
    public class PriceListMustStartAndStopAtMidnightValidationRule : IValidationRule
    {
        private readonly IZonedDateTimeService _zonedDateTimeService;
        private readonly ChargePriceOperationDto _chargePriceOperationDto;

        public PriceListMustStartAndStopAtMidnightValidationRule(
            IZonedDateTimeService zonedDateTimeService,
            ChargePriceOperationDto chargePriceOperationDto)
        {
            _zonedDateTimeService = zonedDateTimeService;
            _chargePriceOperationDto = chargePriceOperationDto;
        }

        public bool IsValid => GetPointsIntervalStartDateTime().GetValueOrDefault().TickOfDay == 0 &&
                               GetPointsIntervalEndDateTime().GetValueOrDefault().TickOfDay == 0;

        public ValidationRuleIdentifier ValidationRuleIdentifier =>
            ValidationRuleIdentifier.PriceListMustStartAndStopAtMidnightValidationRule;

        private ZonedDateTime? GetPointsIntervalStartDateTime()
        {
            return _zonedDateTimeService.GetZonedDateTime(_chargePriceOperationDto.PointsStartInterval);
        }

        private ZonedDateTime? GetPointsIntervalEndDateTime()
        {
            return _zonedDateTimeService.GetZonedDateTime(_chargePriceOperationDto.PointsEndInterval);
        }
    }
}
