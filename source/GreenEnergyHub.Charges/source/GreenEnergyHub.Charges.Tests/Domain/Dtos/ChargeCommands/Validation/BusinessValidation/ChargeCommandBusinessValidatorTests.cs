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

using System.Threading.Tasks;
using AutoFixture.Xunit2;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommands;
using GreenEnergyHub.Charges.Domain.Dtos.Validation;
using GreenEnergyHub.Charges.TestCore.Attributes;
using Moq;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Domain.Dtos.ChargeCommands.Validation.BusinessValidation
{
    [UnitTest]
    public class ChargeCommandBusinessValidatorTests
    {
        [Theory]
        [InlineAutoMoqData]
        public async Task ValidateAsync_WhenCalled_UsesFactoryToFetchRulesAndUseRulesToGetResult(
            [Frozen] Mock<IBusinessValidationRulesFactory<ChargeCommand>> factory,
            Mock<IValidationRuleSet> rules,
            ChargeCommand command,
            ValidationResult validationResult,
            BusinessValidator<ChargeCommand> sut)
        {
            // Arrange
            factory.Setup(
                    f => f.CreateRulesAsync(command))
                .Returns(
                    Task.FromResult(rules.Object));

            rules.Setup(
                    r => r.Validate())
                .Returns(validationResult);

            // Act
            var result = await sut.ValidateAsync(command).ConfigureAwait(false);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(validationResult, result);
        }
    }
}
