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
using GreenEnergyHub.Charges.Domain.Dtos.Validation;
using GreenEnergyHub.Charges.Domain.Dtos.Validation.DocumentValidation.ValidationRules;
using GreenEnergyHub.Charges.Tests.Builders.Command;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Domain.Dtos.DocumentValidation.ValidationRules
{
    [UnitTest]
    public class SenderMustMatchChargeOwnersRuleTests
    {
        [Theory]
        [InlineData("senderId", "senderId", "senderId", true)]
        [InlineData("senderId", "senderId", "invalidSenderId", false)]
        public void GivenSenderMustMatchChargeOwnersRule_ThenSenderMustMatchChargeOwnersToBeValid(
            string senderId,
            string chargeOwnerOne,
            string chargeOwnerTwo,
            bool isValid)
        {
            var marketParticipant = new MarketParticipantDtoBuilder().WithId(senderId).Build();
            var document = new DocumentDtoBuilder().WithSender(marketParticipant).Build();
            var owners = new[] { chargeOwnerOne, chargeOwnerTwo };
            var sut = new SenderMustMatchChargeOwnersRule(document, owners);
            sut.IsValid.Should().Be(isValid);
        }

        [Fact]
        public void ValidationRuleIdentifier_ShouldBe_EqualTo()
        {
            var document = new DocumentDtoBuilder().Build();
            var sut = new SenderMustMatchChargeOwnersRule(document, new[] { "senderId", });
            sut.ValidationRuleIdentifier.Should().Be(ValidationRuleIdentifier.SenderMustMatchChargeOwners);
        }
    }
}
