﻿// Copyright 2020 Energinet DataHub A/S
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
using GreenEnergyHub.Charges.Application.Validation;
using GreenEnergyHub.Charges.Application.Validation.BusinessValidation.ValidationRules;
using GreenEnergyHub.Charges.Domain.ChangeOfCharges.Transaction;
using GreenEnergyHub.Charges.Domain.Common;
using GreenEnergyHub.Charges.TestCore;
using GreenEnergyHub.TestHelpers;
using JetBrains.Annotations;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Application.Validation.BusinessValidation.ValidationRules
{
    [UnitTest]
    public class CommandSenderMustBeAnExistingMarketParticipantRuleTests
    {
        [Theory]
        [AutoMoqData]
        public void IsValid_WhenSenderHasValue_IsTrue(MarketParticipant sender)
        {
            // Arrange
            var sut = new CommandSenderMustBeAnExistingMarketParticipantRule(sender);

            // Act
            sut.IsValid.Should().BeTrue();
        }

        [Fact]
        public void IsValid_WhenSenderIsNull_IsFalse()
        {
            // Arrange
            MarketParticipant? sender = null;
            var sut = new CommandSenderMustBeAnExistingMarketParticipantRule(sender);

            // Assert
            sut.IsValid.Should().BeFalse();
        }

        [Theory]
        [InlineAutoDomainData]
        public void ValidationRuleIdentifier_ShouldBe_EqualTo([NotNull] MarketParticipant sender)
        {
            var sut = new CommandSenderMustBeAnExistingMarketParticipantRule(sender);
            sut.ValidationRuleIdentifier.Should().Be(ValidationRuleIdentifier.CommandSenderMustBeAnExistingMarketParticipant);
        }
    }
}
