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
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommands.Validation.InputValidation.Factories;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommands.Validation.InputValidation.ValidationRules;
using GreenEnergyHub.Charges.Domain.Dtos.Validation;
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
        public void CreateRules_WhenOperationContainsPoints_ShouldContainRules(
            ChargeOperationInputValidationRulesFactory sut)
        {
            // Arrange
            var chargeOperationDto = new ChargeOperationDtoBuilder().WithPoint(0, 1.00m).Build();
            var expectedRulesTypes = GetExpectedRulesForChargePriceOperation().ToList();

            // Act
            var actualRuleTypes = sut.CreateRules(chargeOperationDto)
                .GetRules().Select(r => r.ValidationRule.GetType()).ToList();

            // Assert
            actualRuleTypes.Should().Equal(expectedRulesTypes);
        }

        [Theory]
        [InlineAutoMoqData]
        public void CreateRules_WhenOperationContainsNoPoints_ShouldContainRules(
            ChargeOperationInputValidationRulesFactory sut)
        {
            // Arrange
            var chargeOperationDto = new ChargeOperationDtoBuilder().Build();
            var expectedRulesTypes = GetExpectedRulesForChargeInformationOperation()
                .OrderBy(r => r.FullName);

            // Act
            var actualRuleTypes = sut.CreateRules(chargeOperationDto).GetRules()
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
            Assert.Throws<ArgumentNullException>(() => sut.CreateRules(null!));
        }

        [Theory]
        [InlineAutoMoqData(CimValidationErrorTextToken.ChargePointPosition)]
        [InlineAutoMoqData(CimValidationErrorTextToken.ChargePointPrice)]
        public void CreateRulesForChargeCommand_AllRulesThatNeedTriggeredByForErrorMessage_MustImplementIValidationRuleWithExtendedData(
            CimValidationErrorTextToken cimValidationErrorTextToken,
            ChargeOperationInputValidationRulesFactory sut,
            ChargeOperationDtoBuilder chargeOperationDtoBuilder)
        {
            // Arrange
            var chargeOperationDto = chargeOperationDtoBuilder.Build();

            // Act
            var validationRules = sut.CreateRules(chargeOperationDto).GetRules();

            // Assert
            AssertAllRulesThatNeedTriggeredByForErrorMessageImplementsIValidationRuleWithExtendedData(
                cimValidationErrorTextToken, validationRules);
        }

        private static List<Type> GetExpectedRulesForChargeInformationOperation()
        {
            var expectedRules = new List<Type>
            {
                typeof(ChargeIdLengthValidationRule),
                typeof(ChargeIdRequiredValidationRule),
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
                typeof(ChargePriceMaximumDigitsAndDecimalsRule),
                typeof(ChargeTypeTariffPriceCountRule),
                typeof(StartDateValidationRule),
                typeof(MaximumPriceRule),
            };
            return expectedRules;
        }

        private static IEnumerable<Type> GetExpectedRulesForChargePriceOperation()
        {
            return new List<Type>
            {
                typeof(ChargeIdLengthValidationRule),
                typeof(ChargeIdRequiredValidationRule),
                typeof(ChargeOperationIdRequiredRule),
                typeof(ChargeOwnerIsRequiredValidationRule),
                typeof(ChargeTypeIsKnownValidationRule),
                typeof(StartDateTimeRequiredValidationRule),
                typeof(ChargePriceMaximumDigitsAndDecimalsRule),
                typeof(ChargeTypeTariffPriceCountRule),
                typeof(MaximumPriceRule),
                typeof(NumberOfPointsMatchTimeIntervalAndResolutionRule),
            };
        }

        private static void AssertAllRulesThatNeedTriggeredByForErrorMessageImplementsIValidationRuleWithExtendedData(
            CimValidationErrorTextToken cimValidationErrorTextToken,
            IReadOnlyCollection<IValidationRuleContainer> validationRules)
        {
            var type = typeof(CimValidationErrorTextTemplateMessages);
            foreach (var fieldInfo in type.GetFields(BindingFlags.Static | BindingFlags.Public))
            {
                if (!fieldInfo.GetCustomAttributes().Any()) continue;

                var errorMessageForAttribute = (ErrorMessageForAttribute)fieldInfo.GetCustomAttributes()
                    .Single(x => x.GetType() == typeof(ErrorMessageForAttribute));

                var validationRuleIdentifier = errorMessageForAttribute.ValidationRuleIdentifier;
                var errorText = fieldInfo.GetValue(null)!.ToString();
                var validationErrorTextTokens = CimValidationErrorTextTokenMatcher.GetTokens(errorText!);
                var validationRuleContainer = validationRules
                    .FirstOrDefault(x => x.ValidationRule.ValidationRuleIdentifier == validationRuleIdentifier);

                if (validationErrorTextTokens.Contains(cimValidationErrorTextToken) && validationRuleContainer != null)
                    Assert.True(validationRuleContainer.ValidationRule is IValidationRuleWithExtendedData);
            }
        }
    }
}
