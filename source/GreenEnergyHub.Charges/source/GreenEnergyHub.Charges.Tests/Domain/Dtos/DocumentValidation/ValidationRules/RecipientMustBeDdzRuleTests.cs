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
using GreenEnergyHub.Charges.Domain.Dtos.SharedDtos;
using GreenEnergyHub.Charges.Domain.Dtos.Validation.DocumentValidation.ValidationRules;
using GreenEnergyHub.Charges.TestCore.Builders.Command;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Domain.Dtos.DocumentValidation.ValidationRules
{
    [UnitTest]
    public class RecipientMustBeDdzRuleTests
    {
        [Fact]
        public void IsValid_WhenSenderIsDdz_ShouldReturnTrue()
        {
            // Arrange
            var recipient = new MarketParticipantDtoBuilder()
                .WithMarketParticipantRole(MarketParticipantRole.MeteringPointAdministrator)
                .Build();
            var documentDto = new DocumentDtoBuilder().WithRecipient(recipient).Build();
            var sut = new RecipientMustBeDdzRule(documentDto);

            // Act
            var actually = sut.IsValid;

            // Assert
            actually.Should().BeTrue();
        }

        [Fact]
        public void IsValid_WhenSenderIsNotDdz_ShouldReturnFalse()
        {
            // Arrange
            var recipient = new MarketParticipantDtoBuilder()
                .WithMarketParticipantRole(MarketParticipantRole.EnergyAgency)
                .Build();
            var documentDto = new DocumentDtoBuilder().WithRecipient(recipient).Build();
            var sut = new RecipientMustBeDdzRule(documentDto);

            // Act
            var actually = sut.IsValid;

            // Assert
            actually.Should().BeFalse();
        }
    }
}
