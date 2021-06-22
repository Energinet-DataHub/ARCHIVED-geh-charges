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

using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using GreenEnergyHub.Charges.Application.Validation;
using GreenEnergyHub.Charges.Application.Validation.InputValidation.ValidationRules;
using GreenEnergyHub.Charges.Domain.ChangeOfCharges.Transaction;
using GreenEnergyHub.Charges.TestCore;
using GreenEnergyHub.TestHelpers;
using NodaTime;
using NodaTime.Text;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Application.Validation.InputValidation.ValidationRules
{
    [UnitTest]
    public class StartDateTimeRequiredValidationRuleTests
    {
        [Theory]
        [InlineAutoMoqData("2020-05-10T13:00:00Z", true)]
        [InlineAutoMoqData("1970-01-01T00:00:00Z", false)]
        public void StartDateTimeRequiredValidationRule_NegativeTest(
            string startDateTime,
            bool expected,
            [NotNull] ChargeCommand command)
        {
            command.ChargeOperation.StartDateTime = InstantPattern.General.Parse(startDateTime).Value;
            var sut = new StartDateTimeRequiredValidationRule(command);
            Assert.Equal(expected, sut.IsValid);
        }

        [Theory]
        [InlineAutoDomainData]
        public void ValidationRuleIdentifier_ShouldBe_EqualTo([NotNull] ChargeCommand command)
        {
            var sut = new StartDateTimeRequiredValidationRule(command);
            sut.ValidationRuleIdentifier.Should().Be(ValidationRuleIdentifier.StartDateTimeRequiredValidation);
        }
    }
}
