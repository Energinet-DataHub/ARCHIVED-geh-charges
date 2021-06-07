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
            var actualRuleTypes = sut.CreateRulesForChargeCreateCommand(createCommand).GetRules().Select(r => r.GetType()).ToList();
            var expectedRuleTypes = GetMandatoryRules().Select(r => r.GetType()).ToList();

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
            var actualRuleTypes = sut.CreateRulesForChargeUpdateCommand(updateCommand).GetRules().Select(r => r.GetType()).ToList();
            var expectedRuleTypes = GetMandatoryRules().Select(r => r.GetType()).ToList();

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
            var actualRuleTypes = sut.CreateRulesForChargeStopCommand(stopCommand).GetRules().Select(r => r.GetType()).ToList();
            var expectedRuleTypes = GetMandatoryRules().Select(r => r.GetType()).ToList();

            // Assert
            actualRuleTypes.Should().Contain(expectedRuleTypes);
        }

        private static IEnumerable<IValidationRule> GetMandatoryRules()
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
