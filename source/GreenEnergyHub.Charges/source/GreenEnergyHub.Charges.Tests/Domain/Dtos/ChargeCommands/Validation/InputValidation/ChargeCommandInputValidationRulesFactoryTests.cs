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
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommands;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommands.Validation.InputValidation.Factories;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommands.Validation.InputValidation.ValidationRules;
using GreenEnergyHub.Charges.Domain.Dtos.Validation;
using GreenEnergyHub.Charges.Infrastructure.Core.Cim.ValidationErrors;
using GreenEnergyHub.Charges.MessageHub.Models.Shared;
using GreenEnergyHub.Charges.TestCore.Attributes;
using GreenEnergyHub.Charges.Tests.Builders;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Domain.Dtos.ChargeCommands.Validation.InputValidation
{
    [UnitTest]
    public class ChargeCommandInputValidationRulesFactoryTests
    {
        [Fact]
        public void CreateRulesForChargeCommand_ShouldContainRulesTest()
        {
            // Arrange
            var sut = new ChargeCommandInputValidationRulesFactory();
            var chargeCommand = new ChargeCommandBuilder().Build();
            var expectedRules = new List<IValidationRule>
            {
                new BusinessReasonCodeMustBeUpdateChargeInformationRule(chargeCommand),
                new ChargeDescriptionHasMaximumLengthRule(chargeCommand),
                new ChargeIdLengthValidationRule(chargeCommand),
                new ChargeIdRequiredValidationRule(chargeCommand),
                new ChargeNameHasMaximumLengthRule(chargeCommand),
                new ChargeOperationIdRequiredRule(chargeCommand),
                new ChargeOwnerIsRequiredValidationRule(chargeCommand),
                new ChargePriceMaximumDigitsAndDecimalsRule(chargeCommand),
                new ChargeTypeIsKnownValidationRule(chargeCommand),
                new ChargeTypeTariffPriceCountRule(chargeCommand),
                new DocumentTypeMustBeRequestUpdateChargeInformationRule(chargeCommand),
                new FeeMustHaveSinglePriceRule(chargeCommand),
                new MaximumPriceRule(chargeCommand),
                new RecipientIsMandatoryTypeValidationRule(chargeCommand),
                new ResolutionFeeValidationRule(chargeCommand),
                new ResolutionSubscriptionValidationRule(chargeCommand),
                new ResolutionTariffValidationRule(chargeCommand),
                new SenderIsMandatoryTypeValidationRule(chargeCommand),
                new StartDateTimeRequiredValidationRule(chargeCommand),
                new StopChargeNotYetSupportedValidationRule(chargeCommand),
                new SubscriptionMustHaveSinglePriceRule(chargeCommand),
                new VatClassificationValidationRule(chargeCommand),
            };

            // Act
            var actualRuleTypes = sut.CreateRulesForCommand(chargeCommand).GetRules()
                .Select(r => r.GetType()).ToList();

            var expectedRuleTypes = expectedRules.Select(r => r.GetType()).ToList();

            // Assert
            Assert.True(actualRuleTypes.SequenceEqual(expectedRuleTypes));
        }

        [Fact]
        public void CreateRulesForChargeCommand_ShouldThrowArgumentNullException_WhenCalledWithNull()
        {
            // Arrange
            var sut = new ChargeCommandInputValidationRulesFactory();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => sut.CreateRulesForCommand(null!));
        }

        [Theory]
        [InlineAutoMoqData(CimValidationErrorTextToken.ChargePointPosition)]
        [InlineAutoMoqData(CimValidationErrorTextToken.ChargePointPrice)]
        public void CreateRulesForChargeCommand_AllRulesThatNeedTriggeredByForErrorMessage_MustImplementIValidationRuleWithExtendedData(
            CimValidationErrorTextToken cimValidationErrorTextToken,
            ChargeCommandInputValidationRulesFactory sut,
            ChargeCommand chargeCommand)
        {
            // Arrange
            // Act
            var validationRules = sut.CreateRulesForCommand(chargeCommand).GetRules();

            // Assert
            var type = typeof(CimValidationErrorTextTemplateMessages);
            foreach (var fieldInfo in type.GetFields(BindingFlags.Static | BindingFlags.Public))
            {
                if (!fieldInfo.GetCustomAttributes().Any()) continue;

                var errorMessageForAttribute = (ErrorMessageForAttribute)fieldInfo.GetCustomAttributes()
                    .Single(x => x.GetType() == typeof(ErrorMessageForAttribute));

                var validationRuleIdentifier = errorMessageForAttribute.ValidationRuleIdentifier;
                var errorText = fieldInfo.GetValue(null)!.ToString();
                var validationErrorTextTokens = CimValidationErrorTextTokenMatcher.GetTokens(errorText!);
                var validationRule = validationRules.SingleOrDefault(x => x.ValidationRuleIdentifier == validationRuleIdentifier);

                if (validationErrorTextTokens.Contains(cimValidationErrorTextToken) && validationRule != null)
                    Assert.True(validationRule is IValidationRuleWithExtendedData);
            }
        }
    }
}
