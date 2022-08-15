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

using Energinet.DataHub.Core.TestCommon.AutoFixture.Attributes;
using FluentAssertions;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeInformationCommands.Validation.InputValidation.ValidationRules;
using GreenEnergyHub.Charges.Tests.Builders.Command;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Domain.Dtos.ChargeCommands.Validation.InputValidation.ValidationRules
{
    [UnitTest]
    public class ChargeOwnerTextLengthRuleTests
    {
        [Theory]
        [InlineAutoMoqData("Short   name", false, "the length is less than 13 characters.")]
        [InlineAutoMoqData("Too     long name", false, "the length is longer than 16 characters.")]
        [InlineAutoMoqData("1234567890123", true, "13 character GLN names are supported.")]
        [InlineAutoMoqData("ThisIsA16charEic", true, "16 character EIC names are supported.")]
        public void IsValid_WhenCalled_ShouldReturnExpectedValue(
            string chargeOwner,
            bool expectedResult,
            string reason,
            ChargeInformationOperationDtoBuilder builder)
        {
            // Arrange
            var dto = builder.WithOwner(chargeOwner).Build();
            var sut = new ChargeOwnerTextLengthRule(dto);

            // Assert
            sut.IsValid.Should().Be(expectedResult, reason);
        }
    }
}
