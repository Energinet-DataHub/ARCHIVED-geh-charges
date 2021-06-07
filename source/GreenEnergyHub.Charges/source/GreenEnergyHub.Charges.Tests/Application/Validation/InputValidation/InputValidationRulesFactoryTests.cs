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
using FluentAssertions;
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
        public void CreateRulesForChargeCreateCommand_ShouldContainMandatoryRulesTest()
        {
            // Arrange
            var sut = new InputValidationRulesFactory();
            var createCommand = new TestableChargeCommand { ChargeOperation = { OperationType = OperationType.Create } };

            // Act
            var actualRuleTypes = sut.CreateRulesForChargeCreateCommand(createCommand).GetRules()
                .Select(r => r.GetType()).ToList();
            var expectedRuleTypes = GetExpectedMandatoryRules().Select(r => r.GetType()).ToList();

            // Assert
            actualRuleTypes.Should().Contain(expectedRuleTypes);
        }

        [Fact]
        public void CreateRulesForChargeUpdateCommand_ShouldContainMandatoryRulesTest()
        {
            // Arrange
            var sut = new InputValidationRulesFactory();
            var updateCommand = new TestableChargeCommand { ChargeOperation = { OperationType = OperationType.Update } };

            // Act
            var actualRuleTypes = sut.CreateRulesForChargeUpdateCommand(updateCommand).GetRules()
                .Select(r => r.GetType()).ToList();
            var expectedRuleTypes = GetExpectedMandatoryRules().Select(r => r.GetType()).ToList();

            // Assert
            actualRuleTypes.Should().Contain(expectedRuleTypes);
        }

        [Fact]
        public void CreateRulesForChargeStopCommand_ShouldContainMandatoryRulesTest()
        {
            // Arrange
            var sut = new InputValidationRulesFactory();
            var stopCommand = new TestableChargeCommand { ChargeOperation = { OperationType = OperationType.Stop } };

            // Act
            var actualRuleTypes = sut.CreateRulesForChargeStopCommand(stopCommand).GetRules()
                .Select(r => r.GetType()).ToList();
            var expectedRuleTypes = GetExpectedMandatoryRules().Select(r => r.GetType()).ToList();

            // Assert
            actualRuleTypes.Should().Contain(expectedRuleTypes);
        }

        [Fact]
        public void CreateRulesForChargeCreateCommand_ShouldContainCreateRulesTest()
        {
            // Arrange
            var sut = new InputValidationRulesFactory();
            var createCommand = new TestableChargeCommand { ChargeOperation = { OperationType = OperationType.Create } };

            // Act
            var actualRuleTypes = sut.CreateRulesForChargeCreateCommand(createCommand).GetRules()
                .Select(r => r.GetType()).ToList();
            var expectedRuleTypes = GetExpectedCreateRules().Select(r => r.GetType()).ToList();

            // Assert
            actualRuleTypes.Should().Contain(expectedRuleTypes);
        }

        [Fact]
        public void CreateRulesForChargeUpdateCommand_ShouldContainUpdateRulesTest()
        {
            // Arrange
            var sut = new InputValidationRulesFactory();
            var updateCommand = new TestableChargeCommand { ChargeOperation = { OperationType = OperationType.Update } };

            // Act
            var actualRuleTypes = sut.CreateRulesForChargeUpdateCommand(updateCommand).GetRules()
                .Select(r => r.GetType()).ToList();
            var expectedRuleTypes = GetExpectedUpdateRules().Select(r => r.GetType()).ToList();

            // Assert
            actualRuleTypes.Should().Contain(expectedRuleTypes);
        }

        [Fact]
        public void CreateRulesForChargeUnknownCommand_ShouldContainUnknownRulesTest()
        {
            // Arrange
            var sut = new InputValidationRulesFactory();
            var updateCommand = new TestableChargeCommand { ChargeOperation = { OperationType = OperationType.Unknown } };

            // Act
            var actualRuleTypes = sut.CreateRulesForChargeUnknownCommand(updateCommand).GetRules()
                .Select(r => r.GetType()).ToList();
            var expectedRuleTypes = GetExpectedUnknownRules().Select(r => r.GetType()).ToList();

            // Assert
            actualRuleTypes.Should().Contain(expectedRuleTypes);
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

        private static IEnumerable<IValidationRule> GetExpectedCreateRules()
        {
            var testableChargeCommand = new TestableChargeCommand();

            var rules = new List<IValidationRule>
            {
                new ProcessTypeIsKnownValidationRule(testableChargeCommand),
                new SenderIsMandatoryTypeValidationRule(testableChargeCommand),
                new RecipientIsMandatoryTypeValidationRule(testableChargeCommand),
                new VatClassificationValidationRule(testableChargeCommand),
                new ResolutionTariffValidationRule(testableChargeCommand),
                new ResolutionFeeValidationRule(testableChargeCommand),
                new ResolutionSubscriptionValidationRule(testableChargeCommand),
                new ChargeNameHasMaximumLengthRule(testableChargeCommand),
                new ChargeDescriptionHasMaximumLengthRule(testableChargeCommand),
                new ChargeTypeTariffPriceCountRule(testableChargeCommand),
                new MaximumPriceRule(testableChargeCommand),
                new ChargePriceMaximumDigitsAndDecimalsRule(testableChargeCommand),
                new FeeMustHaveSinglePriceRule(testableChargeCommand),
                new SubscriptionMustHaveSinglePriceRule(testableChargeCommand),
            };

            return rules;
        }

        private static IEnumerable<IValidationRule> GetExpectedUpdateRules()
        {
            var testableChargeCommand = new TestableChargeCommand();

            var rules = new List<IValidationRule>
            {
                new ChargeNameHasMaximumLengthRule(testableChargeCommand),
                new ChargeDescriptionHasMaximumLengthRule(testableChargeCommand),
                new ChargeTypeTariffPriceCountRule(testableChargeCommand),
                new MaximumPriceRule(testableChargeCommand),
                new ChargePriceMaximumDigitsAndDecimalsRule(testableChargeCommand),
                new FeeMustHaveSinglePriceRule(testableChargeCommand),
                new SubscriptionMustHaveSinglePriceRule(testableChargeCommand),
            };

            return rules;
        }

        private static IEnumerable<IValidationRule> GetExpectedUnknownRules()
        {
            var testableChargeCommand = new TestableChargeCommand();

            var rules = new List<IValidationRule>
            {
                new ChargeTypeIsKnownValidationRule(testableChargeCommand),
                new OperationTypeValidationRule(testableChargeCommand),
            };

            return rules;
        }
    }
}
