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
using GreenEnergyHub.Charges.Application.Validation;
using GreenEnergyHub.Charges.Application.Validation.InputValidation;
using GreenEnergyHub.Charges.Application.Validation.InputValidation.ValidationRules;
using GreenEnergyHub.Charges.Domain.ChangeOfCharges.Transaction;
using GreenEnergyHub.Charges.Tests.Application.Validation.BusinessValidation;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Application.Validation.InputValidation
{
    [UnitTest]
    public class InputValidationRulesFactoryTests
    {
        [Fact]
        public void CreateRulesForChargeCreateCommand_ShouldContainRulesTest()
        {
            // Arrange
            var sut = new InputValidationRulesFactory();
            var createCommand = new TestableChargeCommand { ChargeOperation = { OperationType = OperationType.Create } };
            var mandatoryRules = GetExpectedMandatoryRules();
            var createRules = new List<IValidationRule>
            {
                new ProcessTypeIsKnownValidationRule(createCommand),
                new SenderIsMandatoryTypeValidationRule(createCommand),
                new RecipientIsMandatoryTypeValidationRule(createCommand),
                new VatClassificationValidationRule(createCommand),
                new ResolutionTariffValidationRule(createCommand),
                new ResolutionFeeValidationRule(createCommand),
                new ResolutionSubscriptionValidationRule(createCommand),
                new ChargeNameHasMaximumLengthRule(createCommand),
                new ChargeDescriptionHasMaximumLengthRule(createCommand),
                new ChargeTypeTariffPriceCountRule(createCommand),
                new MaximumPriceRule(createCommand),
                new ChargePriceMaximumDigitsAndDecimalsRule(createCommand),
                new FeeMustHaveSinglePriceRule(createCommand),
                new SubscriptionMustHaveSinglePriceRule(createCommand),
            };

            // Act
            var actualRuleTypes = sut.CreateRulesForChargeCreateCommand(createCommand).GetRules()
                .Select(r => r.GetType()).ToList();

            var expectedRuleTypes = createRules.Union(mandatoryRules).Select(r => r.GetType()).ToList();

            // Assert
            Assert.True(actualRuleTypes.SequenceEqual(expectedRuleTypes));
        }

        [Fact]
        public void CreateRulesForChargeUpdateCommand_ShouldContainRulesTest()
        {
            // Arrange
            var sut = new InputValidationRulesFactory();
            var updateCommand = new TestableChargeCommand { ChargeOperation = { OperationType = OperationType.Update } };
            var mandatoryRules = GetExpectedMandatoryRules();
            var updateRules = new List<IValidationRule>
            {
                new ChargeNameHasMaximumLengthRule(updateCommand),
                new ChargeDescriptionHasMaximumLengthRule(updateCommand),
                new ChargeTypeTariffPriceCountRule(updateCommand),
                new MaximumPriceRule(updateCommand),
                new ChargePriceMaximumDigitsAndDecimalsRule(updateCommand),
                new FeeMustHaveSinglePriceRule(updateCommand),
                new SubscriptionMustHaveSinglePriceRule(updateCommand),
            };

            // Act
            var actualRuleTypes = sut.CreateRulesForChargeUpdateCommand(updateCommand).GetRules()
                .Select(r => r.GetType()).ToList();

            var expectedRuleTypes = updateRules.Union(mandatoryRules).Select(r => r.GetType()).ToList();

            // Assert
            Assert.True(actualRuleTypes.SequenceEqual(expectedRuleTypes));
        }

        [Fact]
        public void CreateRulesForChargeStopCommand_ShouldContainRulesTest()
        {
            // Arrange
            var sut = new InputValidationRulesFactory();
            var stopCommand = new TestableChargeCommand { ChargeOperation = { OperationType = OperationType.Stop } };

            // Act
            var actualRuleTypes = sut.CreateRulesForChargeStopCommand(stopCommand).GetRules()
                .Select(r => r.GetType()).ToList();
            var expectedRuleTypes = GetExpectedMandatoryRules().Select(r => r.GetType()).ToList();

            // Assert
            Assert.True(actualRuleTypes.SequenceEqual(expectedRuleTypes));
        }

        [Fact]
        public void CreateRulesForChargeUnknownCommand_ShouldContainUnknownRulesTest()
        {
            // Arrange
            var sut = new InputValidationRulesFactory();
            var unknownCommand = new TestableChargeCommand { ChargeOperation = { OperationType = OperationType.Unknown } };
            var unknownRules = new List<IValidationRule>
            {
                new ChargeTypeIsKnownValidationRule(unknownCommand),
                new OperationTypeValidationRule(unknownCommand),
            };

            // Act
            var actualRuleTypes = sut.CreateRulesForChargeUnknownCommand(unknownCommand).GetRules()
                .Select(r => r.GetType()).ToList();

            var expectedRuleTypes = unknownRules.Select(r => r.GetType()).ToList();

            // Assert
            Assert.True(actualRuleTypes.SequenceEqual(expectedRuleTypes));
        }

        [Fact]
        public void CreateRulesForChargeUpdateCommand_ShouldThrowArgumentNullException_WhenCalledWithNull()
        {
            // Arrange
            var sut = new InputValidationRulesFactory();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => sut.CreateRulesForChargeUpdateCommand(null!));
        }

        [Fact]
        public void CreateRulesForChargeCreateCommand_ShouldThrowArgumentNullException_WhenCalledWithNull()
        {
            // Arrange
            var sut = new InputValidationRulesFactory();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => sut.CreateRulesForChargeCreateCommand(null!));
        }

        [Fact]
        public void CreateRulesForChargeStopCommand_ShouldThrowArgumentNullException_WhenCalledWithNull()
        {
            // Arrange
            var sut = new InputValidationRulesFactory();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => sut.CreateRulesForChargeStopCommand(null!));
        }

        [Fact]
        public void CreateRulesForChargeUnknownCommand_ShouldThrowArgumentNullException_WhenCalledWithNull()
        {
            // Arrange
            var sut = new InputValidationRulesFactory();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => sut.CreateRulesForChargeUnknownCommand(null!));
        }

        private static IEnumerable<IValidationRule> GetExpectedMandatoryRules()
        {
            var testableChargeCommand = new TestableChargeCommand();

            var rules = new List<IValidationRule>
            {
                new ChargeOperationIdRequiredRule(testableChargeCommand),
                new ChargeIdRequiredValidationRule(testableChargeCommand),
                new BusinessReasonCodeMustBeUpdateChargeInformationRule(testableChargeCommand),
                new DocumentTypeMustBeRequestUpdateChargeInformationRule(testableChargeCommand),
                new ChargeTypeIsKnownValidationRule(testableChargeCommand),
                new ChargeIdLengthValidationRule(testableChargeCommand),
                new StartDateTimeRequiredValidationRule(testableChargeCommand),
                new OperationTypeValidationRule(testableChargeCommand),
                new ChargeOwnerIsRequiredValidationRule(testableChargeCommand),
            };

            return rules;
        }
    }
}
