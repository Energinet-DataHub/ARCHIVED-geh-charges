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
        private readonly DateTimeZone _copenhagen = DateTimeZoneProviders.Tzdb["Europe/Copenhagen"];

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
        [InlineAutoMoqData(1, 12)]
        [InlineAutoMoqData(12, 20)]
        [InlineAutoMoqData(20, null!)]
        [InlineAutoMoqData(1, 30)]
        public void IsValid_WhenCalledWithOverlappingChargeLinks_ReturnsFalse(int startDateDayOfMonth, int? endDateDayOfMonth, string meteringPointId, DocumentDto document)
        {
            // Arrange
            var newChargeLinks = GetChargeLinksWithPeriod(2022, 1, startDateDayOfMonth, endDateDayOfMonth);
            var chargeLinkCommand = new ChargeLinksCommand(meteringPointId, document, newChargeLinks);

            var existingChargeLinks = GetExistingChargeLinks();

            var sut = new ChargeLinksUpdateNotYetSupportedRule(chargeLinkCommand, existingChargeLinks);

            // Act & Assert
            sut.IsValid.Should().BeFalse();
        }

        [Theory]
        [InlineAutoMoqData(1, 12)]
        public void IsValid_WhenExistingChargeLinksListIsEmpty_ReturnsTrue(int startDateDayOfMonth, int? endDateDayOfMonth, string meteringPointId, DocumentDto document)
        {
            // Arrange
            var newChargeLinks = GetChargeLinksWithPeriod(2022, 1, startDateDayOfMonth, endDateDayOfMonth);
            var chargeLinkCommand = new ChargeLinksCommand(meteringPointId, document, newChargeLinks);

            var existingChargeLinks = new List<ChargeLink>();

            var sut = new ChargeLinksUpdateNotYetSupportedRule(chargeLinkCommand, existingChargeLinks);

            // Act & Assert
            sut.IsValid.Should().BeTrue();
        }

        private IReadOnlyCollection<ChargeLink> GetExistingChargeLinks()
        {
            var day1LocalDateTime = new LocalDateTime(2022, 1, 11, 0, 0);
            var day1Utc = _copenhagen.AtStrictly(day1LocalDateTime).ToInstant();

            var day2LocalDateTime = new LocalDateTime(2022, 1, 21, 0, 0);
            var day2Utc = _copenhagen.AtStrictly(day2LocalDateTime).ToInstant();

            var link = new ChargeLinkBuilder().WithStartDate(day1Utc).WithEndDate(day2Utc).Build();

            return new List<ChargeLink> { link };
        }

        private List<ChargeLinkDto> GetChargeLinksWithoutOverlappingPeriods()
        {
            var day1LocalDateTime = new LocalDateTime(2022, 1, 3, 0, 0);
            var day1Utc = _copenhagen.AtStrictly(day1LocalDateTime).ToInstant();

            var day2LocalDateTime = new LocalDateTime(2022, 1, 11, 0, 0);
            var day2Utc = _copenhagen.AtStrictly(day2LocalDateTime).ToInstant();

            var link1 = new ChargeLinkDtoBuilder().WithStartDate(day1Utc).WithEndDate(day2Utc).Build();

            var day3LocalDateTime = new LocalDateTime(2022, 1, 21, 0, 0);
            var day3Utc = _copenhagen.AtStrictly(day3LocalDateTime).ToInstant();
            var link2 = new ChargeLinkDtoBuilder().WithStartDate(day3Utc).Build();

            return new List<ChargeLinkDto> { link1, link2 };
        }

        private List<ChargeLinkDto> GetChargeLinksWithPeriod(int year, int month, int startDateDayOfMonth, int? endDateDayOfMonth)
        {
            var day1LocalDateTime = new LocalDateTime(year, month, startDateDayOfMonth, 0, 0);
            var day1Utc = _copenhagen.AtStrictly(day1LocalDateTime).ToInstant();

            var day2LocalDateTime = new LocalDateTime(year, month, endDateDayOfMonth.GetValueOrDefault(1), 0, 0);
            var day2Utc = _copenhagen.AtStrictly(day2LocalDateTime).ToInstant();

            var link = endDateDayOfMonth is not null
                ? new ChargeLinkDtoBuilder().WithStartDate(day1Utc).WithEndDate(day2Utc).Build()
                : new ChargeLinkDtoBuilder().WithStartDate(day1Utc).Build();

            return new List<ChargeLinkDto> { link };
        }
    }
}
