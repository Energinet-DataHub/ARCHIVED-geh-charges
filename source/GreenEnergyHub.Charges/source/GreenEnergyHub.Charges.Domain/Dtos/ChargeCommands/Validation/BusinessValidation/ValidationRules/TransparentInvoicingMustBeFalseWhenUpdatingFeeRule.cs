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

using GreenEnergyHub.Charges.Domain.Charges;
using GreenEnergyHub.Charges.Domain.Dtos.Validation;

namespace GreenEnergyHub.Charges.Domain.Dtos.ChargeCommands.Validation.BusinessValidation.ValidationRules
{
    public class TransparentInvoicingMustBeFalseWhenUpdatingFeeRule : IValidationRule
    {
        private readonly ChargeType _chargeType;
        private readonly TransparentInvoicing _transParentInvoicing;

        public TransparentInvoicingMustBeFalseWhenUpdatingFeeRule(ChargeType chargeType, TransparentInvoicing transParentInvoicing)
        {
            _chargeType = chargeType;
            _transParentInvoicing = transParentInvoicing;
        }

        public bool IsValid => _chargeType is not ChargeType.Fee ||
                               (_chargeType is ChargeType.Fee && _transParentInvoicing is TransparentInvoicing.NonTransparent);

        public ValidationRuleIdentifier ValidationRuleIdentifier =>
            ValidationRuleIdentifier.TransparentInvoicingMustBeFalseWhenUpdatingFee;
    }
}
