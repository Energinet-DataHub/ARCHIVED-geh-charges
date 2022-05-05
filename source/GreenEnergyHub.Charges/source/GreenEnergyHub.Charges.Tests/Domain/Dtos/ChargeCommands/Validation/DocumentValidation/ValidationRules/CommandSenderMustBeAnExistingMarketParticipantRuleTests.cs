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

using FluentAssertions;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommands.Validation.DocumentValidation.ValidationRules;
using GreenEnergyHub.Charges.Domain.Dtos.Validation;
using GreenEnergyHub.Charges.TestCore.Attributes;
using GreenEnergyHub.Charges.Tests.Builders.Testables;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Domain.Dtos.ChargeCommands.Validation.DocumentValidation.ValidationRules
{
    [UnitTest]
    public class CommandSenderMustBeAnExistingMarketParticipantRuleTests
    {
        [Theory]
        [AutoMoqData]
        public void IsValid_WhenSenderHasValue_IsTrue(TestMarketParticipant sender)
        {
            var sut = new CommandSenderMustBeAnExistingMarketParticipantRule(sender);
            sut.IsValid.Should().BeTrue();
        }

        [Fact]
        public void IsValid_WhenSenderIsNull_IsFalse()
        {
            var sut = CreateInvalidRule();
            sut.IsValid.Should().BeFalse();
        }

        [Fact]
        public void ValidationRuleIdentifier_ShouldBe_EqualTo()
        {
            var sut = CreateInvalidRule();
            sut.ValidationRuleIdentifier.Should().Be(ValidationRuleIdentifier.CommandSenderMustBeAnExistingMarketParticipant);
        }

        private static CommandSenderMustBeAnExistingMarketParticipantRule CreateInvalidRule()
        {
            return new(null);
        }
    }
}
