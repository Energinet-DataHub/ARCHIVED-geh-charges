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

namespace GreenEnergyHub.Charges.Domain.Dtos.ChargePriceCommands.Validation.InputValidation.ValidationRules
{
    public class MaximumPriceRule : IValidationRuleWithExtendedData
    {
        private const int PriceUpperBound = 1000000;
        private readonly ChargePriceOperationDto _chargeOperationDto;

        public MaximumPriceRule(ChargePriceOperationDto chargeOperationDto)
        {
            _chargeOperationDto = chargeOperationDto;
        }

        public ValidationRuleIdentifier ValidationRuleIdentifier => ValidationRuleIdentifier.MaximumPrice;

        public bool IsValid => _chargeOperationDto.Points.All(Validate);

        /// <summary>
        /// This validation rule validates each Price in a list of Point(s). This property
        /// will tell which Point triggered the rule. The Point is identified by Position.
        /// </summary>
        public string TriggeredBy => _chargeOperationDto.Points.First(point => !Validate(point)).Position.ToString();

        private bool Validate(Point point)
        {
            return point.Price < PriceUpperBound;
        }
    }
}
