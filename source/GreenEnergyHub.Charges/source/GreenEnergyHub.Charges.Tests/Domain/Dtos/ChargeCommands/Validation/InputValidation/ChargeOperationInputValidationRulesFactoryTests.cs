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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeInformationCommands.Validation.InputValidation.Factories;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeInformationCommands.Validation.InputValidation.ValidationRules;
using GreenEnergyHub.Charges.Domain.Dtos.SharedDtos;
using GreenEnergyHub.Charges.Domain.Dtos.Validation;
using GreenEnergyHub.Charges.Domain.Dtos.Validation.InputValidation;
using GreenEnergyHub.Charges.Infrastructure.Core.Cim.ValidationErrors;
using GreenEnergyHub.Charges.MessageHub.Models.Shared;
using GreenEnergyHub.Charges.TestCore.Attributes;
using GreenEnergyHub.Charges.Tests.Builders.Command;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Domain.Dtos.ChargeCommands.Validation.InputValidation
{
    [UnitTest]
    public class ChargeOperationInputValidationRulesFactoryTests
    {
        [Theory]
        [InlineAutoMoqData]
        public void CreateRules_WhenChargeInformationOperation_ShouldContainRules(
            ChargeOperationInputValidationRulesFactory sut,
            ChargeInformationOperationDtoBuilder chargeInformationOperationDtoBuilder,
            DocumentDtoBuilder documentDtoBuilder)
        {
            // Arrange
            var document = documentDtoBuilder
                .WithBusinessReasonCode(BusinessReasonCode.UpdateChargeInformation)
                .Build();
            var chargeOperationDto = chargeInformationOperationDtoBuilder.Build();
            var expectedRulesTypes = GetExpectedRulesForChargeInformationOperation()
                .OrderBy(r => r.FullName);

            // Act
            var actualRuleTypes = sut.CreateRules(chargeOperationDto, document).GetRules()
                .Select(r => r.ValidationRule.GetType())
                .OrderBy(r => r.FullName)
                .ToList();

            // Assert
            actualRuleTypes.Should().Equal(expectedRulesTypes);
        }

        [Theory]
        [InlineAutoMoqData]
        public void CreateRules_ShouldThrowArgumentNullException_WhenCalledWithNull(
            ChargeOperationInputValidationRulesFactory sut)
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => sut.CreateRules(null!, null!));
        }

        private static List<Type> GetExpectedRulesForChargeInformationOperation()
        {
            var expectedRules = new List<Type>
            {
                typeof(ChargeIdLengthValidationRule),
                typeof(ChargeIdRequiredValidationRule),
                typeof(ChargeOperationIdLengthValidationRule),
                typeof(ChargeOperationIdRequiredRule),
                typeof(ChargeOwnerIsRequiredValidationRule),
                typeof(ChargeTypeIsKnownValidationRule),
                typeof(StartDateTimeRequiredValidationRule),
                typeof(ResolutionFeeValidationRule),
                typeof(ResolutionSubscriptionValidationRule),
                typeof(ResolutionTariffValidationRule),
                typeof(ChargeNameHasMaximumLengthRule),
                typeof(ChargeDescriptionHasMaximumLengthRule),
                typeof(VatClassificationValidationRule),
                typeof(TransparentInvoicingIsNotAllowedForFeeValidationRule),
                typeof(StartDateValidationRule),
                typeof(ChargeNameRequiredRule),
                typeof(ChargeDescriptionRequiredRule),
                typeof(ResolutionIsRequiredRule),
                typeof(TransparentInvoicingIsRequiredValidationRule),
                typeof(TaxIndicatorIsRequiredValidationRule),
                typeof(TerminationDateMustMatchEffectiveDateValidationRule),
                typeof(TaxIndicatorMustBeFalseForFeeValidationRule),
                typeof(TaxIndicatorMustBeFalseForSubscriptionValidationRule),
                typeof(ChargeOwnerMustMatchSenderRule),
                typeof(ChargeTypeTariffTaxIndicatorOnlyAllowedBySystemOperatorValidationRule),
            };
            return expectedRules;
        }
    }
}
