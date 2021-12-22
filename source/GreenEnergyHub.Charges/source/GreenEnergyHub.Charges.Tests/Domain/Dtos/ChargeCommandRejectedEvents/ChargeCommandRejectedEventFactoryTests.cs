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
using AutoFixture.Xunit2;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommandRejectedEvents;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommands;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommands.Validation;
using GreenEnergyHub.Charges.TestCore.Attributes;
using Moq;
using NodaTime;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Domain.Dtos.ChargeCommandRejectedEvents
{
    [UnitTest]
    public class ChargeCommandRejectedEventFactoryTests
    {
        [Theory]
        [InlineAutoMoqData]
        public void CreateEvent_WhenCalledWithValidationResult_CreatesEventWithCorrectFailures(
            [Frozen] Mock<IClock> clock,
            ChargeCommand command,
            Mock<ValidationError> validationError,
            Mock<IValidationRule> validationRule,
            Mock<IValidationRuleSet> validationRuleSetMock,
            ChargeCommandRejectedEventFactory sut)
        {
            // Arrange
            var currentTime = Instant.FromUtc(2021, 7, 7, 7, 50, 49);
            clock.Setup(c => c.GetCurrentInstant()).Returns(currentTime);
            validationRule.Setup(x => x.IsValid).Returns(false);
            validationRule.Setup(y => y.ValidationError).Returns(validationError.Object);
            validationRuleSetMock.Setup(x => x.GetRules()).Returns(new List<IValidationRule> { validationRule.Object });

            var failedRules = validationRuleSetMock.Object.GetRules().ToList();
            var validationResult = ValidationResult.CreateFailure(failedRules);

            // Act
            var result = sut.CreateEvent(command, validationResult);

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(failedRules);
            Assert.Equal(currentTime, result.PublishedTime);
            Assert.Equal(failedRules.Count, result.RejectReasons.Count());
            foreach (var failedRule in failedRules)
            {
                Assert.Contains(failedRule.ValidationError!.ValidationRuleIdentifier.ToString(), result.RejectReasons);
            }
        }

        [Theory]
        [InlineAutoMoqData]
        public void CreateEvent_WhenCalledException_CreatesEventWithOneReason(
            [Frozen] Mock<IClock> clock,
            ChargeCommand command,
            Exception exception,
            ChargeCommandRejectedEventFactory sut)
        {
            // Arrange
            var currentTime = Instant.FromUtc(2021, 7, 7, 7, 50, 49);
            clock.Setup(
                    c => c.GetCurrentInstant())
                .Returns(currentTime);

            // Act
            var result = sut.CreateEvent(command, exception);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(currentTime, result.PublishedTime);
            Assert.Single(result.RejectReasons);
            Assert.Equal(exception.Message, result.RejectReasons.First());
        }
    }
}
