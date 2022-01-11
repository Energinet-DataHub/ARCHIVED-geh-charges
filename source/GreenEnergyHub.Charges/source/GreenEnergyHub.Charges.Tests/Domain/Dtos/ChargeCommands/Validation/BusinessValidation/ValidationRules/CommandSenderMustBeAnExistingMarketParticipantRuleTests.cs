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

using System.Linq;
using FluentAssertions;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommands;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommands.Validation;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommands.Validation.BusinessValidation.ValidationRules;
using GreenEnergyHub.Charges.Domain.Dtos.Validation;
using GreenEnergyHub.Charges.Domain.MarketParticipants;
using GreenEnergyHub.Charges.TestCore.Attributes;
using GreenEnergyHub.TestHelpers;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Domain.Dtos.ChargeCommands.Validation.BusinessValidation.ValidationRules
{
    [UnitTest]
    public class CommandSenderMustBeAnExistingMarketParticipantRuleTests
    {
        [Theory]
        [AutoMoqData]
        public void IsValid_WhenSenderHasValue_IsTrue(MarketParticipant sender, ChargeCommand command)
        {
            var sut = new CommandSenderMustBeAnExistingMarketParticipantRule(command, sender);
            sut.IsValid.Should().BeTrue();
        }

        [Theory]
        [AutoMoqData]
        public void IsValid_WhenSenderIsNull_IsFalse(ChargeCommand command)
        {
            var sut = CreateInvalidRule(command);
            sut.IsValid.Should().BeFalse();
        }

        [Theory]
        [InlineAutoDomainData]
        public void ValidationRuleIdentifier_ShouldBe_EqualTo(ChargeCommand command)
        {
            var sut = CreateInvalidRule(command);
            sut.ValidationRuleIdentifier.Should().Be(ValidationRuleIdentifier.CommandSenderMustBeAnExistingMarketParticipant);
        }

        private static CommandSenderMustBeAnExistingMarketParticipantRule CreateInvalidRule(ChargeCommand command)
        {
            return new(command, null);
        }
    }
}
