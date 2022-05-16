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
using GreenEnergyHub.Charges.Domain.Dtos.Validation;
using Moq;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Domain.Dtos.ChargeCommands.Validation
{
    [UnitTest]
    public class ValidationRuleSetTests
    {
        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(10)]
        public void Validate_WhenAllRulesAreValid_ResultIsNotFailed(int noOfRules)
        {
            // Arrange
            var rules = GetRules(noOfRules, 0);
            var sut = ValidationRuleSet.FromRules(rules);

            // Act
            var result = sut.Validate();

            // Assert
            Assert.NotNull(result);
            Assert.False(result.IsFailed);
        }

        [Theory]
        [InlineData(1, 1)]
        [InlineData(2, 1)]
        [InlineData(2, 2)]
        [InlineData(10, 1)]
        public void Validate_WhenOneOrMoreRulesAreInvalid_ResultIsFailed(
            int noOfRules,
            int noOfFailedRules)
        {
            // Arrange
            var rules = GetRules(noOfRules, noOfFailedRules);
            var sut = ValidationRuleSet.FromRules(rules);

            // Act
            var result = sut.Validate();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.IsFailed);
        }

        private static List<IValidationRuleContainer> GetRules(int desiredNumberOfRules, int failedRules)
        {
            var rules = new List<IValidationRuleContainer>();

            for (var i = 0; i < desiredNumberOfRules; i++)
            {
                rules.Add(CreateRule(i < failedRules));
            }

            return rules;
        }

        private static IValidationRuleContainer CreateRule(bool failed)
        {
            var rule = new Mock<IValidationRuleContainer>();
            rule.Setup(r => r.ValidationRule.IsValid).Returns(!failed);
            return rule.Object;
        }
    }
}
