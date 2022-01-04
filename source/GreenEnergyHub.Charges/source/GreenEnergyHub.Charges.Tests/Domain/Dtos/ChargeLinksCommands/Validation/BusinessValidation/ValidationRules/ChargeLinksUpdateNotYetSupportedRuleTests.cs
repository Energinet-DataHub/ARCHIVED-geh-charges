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

using System.Collections.Generic;
using FluentAssertions;
using GreenEnergyHub.Charges.Domain.ChargeLinks;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeLinksCommands;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeLinksCommands.Validation.BusinessValidation.ValidationRules;
using GreenEnergyHub.Charges.Domain.Dtos.SharedDtos;
using GreenEnergyHub.Charges.TestCore.Attributes;
using GreenEnergyHub.Charges.Tests.Builders;
using NodaTime;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Domain.Dtos.ChargeLinksCommands.Validation.BusinessValidation.ValidationRules
{
    [UnitTest]
    public class ChargeLinksUpdateNotYetSupportedRuleTests
    {
        [Theory]
        [InlineAutoMoqData]
        public void IsValid_WhenCalledWithValidChargeLinks_ReturnsTrue(string meteringPointId, DocumentDto document)
        {
            // Arrange
            var newChargeLinks = GetChargeLinksWithoutOverlappingPeriods();
            var chargeLinkCommand = new ChargeLinksCommand(meteringPointId, document, newChargeLinks);

            var existingChargeLinks = GetExistingChargeLinks();

            var sut = new ChargeLinksUpdateNotYetSupportedRule(chargeLinkCommand, existingChargeLinks);

            // Act & Assert
            sut.IsValid.Should().BeTrue();
        }

        [Theory]
        [InlineAutoMoqData]
        public void IsValid_WhenCalledWithOverlappingChargeLinks_ReturnsFalse(string meteringPointId, DocumentDto document)
        {
            // Arrange
            var newChargeLinks = GetChargeLinksWithOverlappingPeriods();
            var chargeLinkCommand = new ChargeLinksCommand(meteringPointId, document, newChargeLinks);

            var existingChargeLinks = GetExistingChargeLinks();

            var sut = new ChargeLinksUpdateNotYetSupportedRule(chargeLinkCommand, existingChargeLinks);

            // Act & Assert
            sut.IsValid.Should().BeFalse();
        }

        private IReadOnlyCollection<ChargeLink> GetExistingChargeLinks()
        {
            var day1 = SystemClock.Instance.GetCurrentInstant().Minus(Duration.FromDays(20));
            var day2 = SystemClock.Instance.GetCurrentInstant().Minus(Duration.FromDays(11));
            var link = new ChargeLinkBuilder().WithStartDate(day1).WithEndDate(day2).Build();

            return new List<ChargeLink> { link, };
        }

        private List<ChargeLinkDto> GetChargeLinksWithoutOverlappingPeriods()
        {
            var day1 = SystemClock.Instance.GetCurrentInstant().Minus(Duration.FromDays(10));
            var day2 = SystemClock.Instance.GetCurrentInstant().Minus(Duration.FromDays(0));
            var link1 = new ChargeLinkDtoBuilder().WithStartDate(day1).WithEndDate(day2).Build();

            var day3 = SystemClock.Instance.GetCurrentInstant().Plus(Duration.FromDays(1));
            var link2 = new ChargeLinkDtoBuilder().WithStartDate(day3).Build();

            return new List<ChargeLinkDto> { link1, link2, };
        }

        private List<ChargeLinkDto> GetChargeLinksWithOverlappingPeriods()
        {
            var day1 = SystemClock.Instance.GetCurrentInstant().Minus(Duration.FromDays(30));
            var day2 = SystemClock.Instance.GetCurrentInstant().Minus(Duration.FromDays(0));
            var link1 = new ChargeLinkDtoBuilder().WithStartDate(day1).WithEndDate(day2).Build();

            var day3 = SystemClock.Instance.GetCurrentInstant().Plus(Duration.FromDays(1));
            var link2 = new ChargeLinkDtoBuilder().WithStartDate(day3).Build();

            return new List<ChargeLinkDto> { link1, link2, };
        }
    }
}
