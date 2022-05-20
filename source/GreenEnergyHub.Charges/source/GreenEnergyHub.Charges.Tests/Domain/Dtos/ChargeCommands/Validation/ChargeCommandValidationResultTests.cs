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
using GreenEnergyHub.Charges.Domain.Dtos.Validation;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Domain.Dtos.ChargeCommands.Validation
{
    [UnitTest]
    public class ChargeCommandValidationResultTests
    {
        [Fact]
        public void CreateSuccess_ReturnsValidResult()
        {
            var validationResult = ValidationResult.CreateSuccess();
            Assert.False(validationResult.IsFailed);
        }

        [Fact]
        public void CreateFailure_WhenCreatedWithInvalidRules_ReturnsInvalidResult()
        {
            var invalidRules = CreateInvalidRules();
            var validationResult = ValidationResult.CreateFailure(invalidRules);
            Assert.True(validationResult.IsFailed);
        }

        [Fact]
        public void CreateFailure_WhenCreatedWithAnyValidRule_ThrowsArgumentException()
        {
            var validRules = CreateValidRules();
            Assert.Throws<ArgumentException>(() => ValidationResult.CreateFailure(validRules));
        }

        [Fact]
        public void CreateFailure_WhenCreatedWithValidAndInvalidRules_ThrowsArgumentException()
        {
            // Arrange
            var validRules = CreateValidRules();
            var invalidRules = CreateInvalidRules();
            var allRules = validRules.Concat(invalidRules).ToList();

            // Act and assert
            Assert.Throws<ArgumentException>(() => ValidationResult.CreateFailure(allRules));
        }

        private static List<IValidationRuleContainer> CreateValidRules()
        {
            return new List<IValidationRuleContainer>
            {
                new DocumentValidationRuleContainer(
                    new TestValidationRule(true, ValidationRuleIdentifier.StartDateValidation)),
            };
        }

        private static List<IValidationRuleContainer> CreateInvalidRules()
        {
            return new List<IValidationRuleContainer>
            {
                new DocumentValidationRuleContainer(
                    new TestValidationRule(false, ValidationRuleIdentifier.StartDateValidation)),
            };
        }
    }
}
