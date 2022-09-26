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
using GreenEnergyHub.Charges.Domain.Dtos.ChargeInformationCommands;
using GreenEnergyHub.Charges.Domain.Dtos.Validation;
using GreenEnergyHub.Charges.Domain.Dtos.Validation.DocumentValidation.ValidationRules;
using GreenEnergyHub.Charges.TestCore.Attributes;
using GreenEnergyHub.Charges.TestCore.Builders.Command;
using GreenEnergyHub.TestHelpers;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Domain.Dtos.DocumentValidation.ValidationRules
{
    [UnitTest]
    public class SenderIsMandatoryValidationRuleTests
    {
        [Theory]
        [InlineAutoMoqData(null!, false)]
        [InlineAutoMoqData("", false)]
        [InlineAutoMoqData("content", true)]
        public void SenderIsMandatoryValidationRule_Test(
            string id,
            bool expected,
            MarketParticipantDtoBuilder marketParticipantDtoBuilder,
            DocumentDtoBuilder documentDtoBuilder)
        {
            var sender = marketParticipantDtoBuilder.WithMarketParticipantId(id).Build();
            var document = documentDtoBuilder.WithSender(sender).Build();
            var sut = new SenderIsMandatoryTypeValidationRule(document);
            Assert.Equal(expected, sut.IsValid);
            sut.IsValid.Should().Be(expected);
        }

        [Theory]
        [InlineAutoDomainData]
        public void ValidationRuleIdentifier_ShouldBe_EqualTo(
            DocumentDtoBuilder documentDtoBuilder)
        {
            var document = documentDtoBuilder.Build();
            var sut = new SenderIsMandatoryTypeValidationRule(document);
            sut.ValidationRuleIdentifier.Should().Be(ValidationRuleIdentifier.SenderIsMandatoryTypeValidation);
        }
    }
}
