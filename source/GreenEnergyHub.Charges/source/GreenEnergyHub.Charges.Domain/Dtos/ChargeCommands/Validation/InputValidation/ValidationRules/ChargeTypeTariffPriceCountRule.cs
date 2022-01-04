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

namespace GreenEnergyHub.Charges.Domain.Dtos.ChargeCommands.Validation.InputValidation.ValidationRules
{
    public class ChargeTypeTariffPriceCountRule : IValidationRule
    {
        private const int PricePointsRequiredInP1D = 1;
        private const int PricePointsRequiredInPt1H = 24;
        private const int PricePointsRequiredInPt15M = 96;
        private readonly ChargeCommand _chargeCommand;

        public ChargeTypeTariffPriceCountRule(ChargeCommand chargeCommand)
        {
            _chargeCommand = chargeCommand;
        }

        public ValidationRuleIdentifier ValidationRuleIdentifier => ValidationRuleIdentifier.ChargeTypeTariffPriceCount;

        public bool IsValid => Validate();

        private bool Validate()
        {
            if (_chargeCommand.ChargeOperation.Type == ChargeType.Tariff)
            {
                switch (_chargeCommand.ChargeOperation.Resolution)
                {
                    case Resolution.PT15M:
                        return _chargeCommand.ChargeOperation.Points.Count == PricePointsRequiredInPt15M;
                    case Resolution.PT1H:
                        return _chargeCommand.ChargeOperation.Points.Count == PricePointsRequiredInPt1H;
                    case Resolution.P1D:
                        return _chargeCommand.ChargeOperation.Points.Count == PricePointsRequiredInP1D;
                    default:
                        throw new ArgumentException(nameof(_chargeCommand.ChargeOperation.Resolution));
                }
            }

            return true;
        }
    }
}
