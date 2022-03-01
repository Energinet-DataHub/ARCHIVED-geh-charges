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
using GreenEnergyHub.Charges.Domain.Charges;
using GreenEnergyHub.Charges.Domain.Dtos.Validation;

namespace GreenEnergyHub.Charges.Domain.Dtos.ChargeCommands.Validation.InputValidation.ValidationRules
{
    public class ChargeTypeTariffPriceCountRule : IValidationRule
    {
        private const int PricePointsRequiredInP1D = 1;
        private const int PricePointsRequiredInPt1H = 24;
        private const int PricePointsRequiredInPt15M = 96;
        private readonly ChargeDto _chargeDto;

        public ChargeTypeTariffPriceCountRule(ChargeDto chargeDto)
        {
            _chargeDto = chargeDto;
        }

        public ValidationRuleIdentifier ValidationRuleIdentifier => ValidationRuleIdentifier.ChargeTypeTariffPriceCount;

        public bool IsValid => Validate();

        private bool Validate()
        {
            if (_chargeDto.ChargeOperation.Type == ChargeType.Tariff)
            {
                return _chargeDto.ChargeOperation.Resolution switch
                {
                    Resolution.PT15M => _chargeDto.ChargeOperation.Points.Count == PricePointsRequiredInPt15M,
                    Resolution.PT1H => _chargeDto.ChargeOperation.Points.Count == PricePointsRequiredInPt1H,
                    Resolution.P1D => _chargeDto.ChargeOperation.Points.Count == PricePointsRequiredInP1D,
                    _ => throw new ArgumentException(nameof(_chargeDto.ChargeOperation.Resolution)),
                };
            }

            return true;
        }
    }
}
