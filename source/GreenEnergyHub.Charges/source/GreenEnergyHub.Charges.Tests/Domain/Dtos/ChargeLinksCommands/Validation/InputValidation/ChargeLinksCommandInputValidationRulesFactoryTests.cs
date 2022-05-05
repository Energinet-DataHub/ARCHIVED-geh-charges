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
using System.Linq;
using FluentAssertions;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeLinksCommands.Validation.InputValidation.Factories;
using GreenEnergyHub.Charges.Tests.Builders.Command;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Domain.Dtos.ChargeLinksCommands.Validation.InputValidation
{
    [UnitTest]
    public class ChargeLinksCommandInputValidationRulesFactoryTests
    {
        [Fact]
        public void CreateRulesForChargeLinksCommand_ShouldContainRulesTest()
        {
            // Arrange
            var sut = new ChargeLinksCommandInputValidationRulesFactory();
            var chargeLinksCommand = new ChargeLinksCommandBuilder().Build();

            // Act
            var actualRuleTypes = sut.CreateRulesForCommand(chargeLinksCommand)
                .GetRules().Select(r => r.GetType()).ToList();

            // Assert
            actualRuleTypes.Count.Should().Be(0);
        }

        [Fact]
        public void CreateRulesForChargeLinksCommand_ShouldThrowArgumentNullException_WhenCalledWithNull()
        {
            // Arrange
            var sut = new ChargeLinksCommandInputValidationRulesFactory();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => sut.CreateRulesForCommand(null!));
        }
    }
}
