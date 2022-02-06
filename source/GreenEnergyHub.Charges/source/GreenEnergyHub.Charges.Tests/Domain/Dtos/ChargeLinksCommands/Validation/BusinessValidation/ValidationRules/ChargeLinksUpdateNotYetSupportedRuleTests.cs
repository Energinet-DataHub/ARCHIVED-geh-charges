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
using NodaTime.Text;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Domain.Dtos.ChargeLinksCommands.Validation.BusinessValidation.ValidationRules
{
    [UnitTest]
    public class ChargeLinksUpdateNotYetSupportedRuleTests
    {
        [Theory]
        [InlineAutoMoqData("2022-01-11T23:00:00Z", "2022-01-20T23:00:00Z", "2022-01-21T00:00:00Z", "2022-01-25T23:00:00Z")] // Before existing
        [InlineAutoMoqData("2022-01-25T23:00:00Z", "2022-01-31T23:00:00Z", "2022-01-01T23:00:00Z", "2022-01-25T23:00:00Z")] // After existing
        [InlineAutoMoqData("2022-01-21T23:00:00Z", "9999-12-31T23:59:59Z", "2022-01-11T23:00:00Z", "2022-01-21T23:00:00Z")] // After with EndDate is DateTime.Max
        [InlineAutoMoqData("2022-01-21T23:00:00Z", null!, "2022-01-11T23:00:00Z", "2022-01-21T23:00:00Z")] // After with EndDate is null!
        public void IsValid_WhenCalledWithValidChargeLinks_ReturnsTrue(
            string newStartDate, string? newEndDate, string existingStartDate, string existingEndDate, string meteringPointId, DocumentDto document)
        {
            // Arrange
            var newChargeLinks = new List<ChargeLinkDto> { CreateChargeLinkDto(newStartDate, newEndDate) };
            var chargeLinkCommand = new ChargeLinksCommand(meteringPointId, document, newChargeLinks);

            var existingChargeLinks = GetExistingChargeLinks(existingStartDate, existingEndDate);

            var sut = new ChargeLinksUpdateNotYetSupportedRule(chargeLinkCommand, existingChargeLinks);

            // Act & Assert
            sut.IsValid.Should().BeTrue();
        }

        [Theory]
        [InlineAutoMoqData("2022-01-11T23:00:00Z", "2022-01-20T23:00:00Z", "2022-01-05T23:00:00Z", "2022-01-25T23:00:00Z")] // Encapsulated in existing
        [InlineAutoMoqData("2022-01-01T23:00:00Z", "2022-01-12T23:00:00Z", "2022-01-11T23:00:00Z", "2022-01-21T23:00:00Z")] // Intersects at beginning of existing
        [InlineAutoMoqData("2022-01-12T23:00:00Z", "2022-01-20T23:00:00Z", "2022-01-11T23:00:00Z", "2022-01-21T23:00:00Z")] // Intersects at end of existing
        [InlineAutoMoqData("2022-01-01T23:00:00Z", "2022-01-30T23:00:00Z", "2022-01-11T23:00:00Z", "2022-01-21T23:00:00Z")] // Encapsulates existing
        [InlineAutoMoqData("2022-01-11T23:00:00Z", "2022-01-21T23:00:00Z", "2022-01-11T23:00:00Z", "2022-01-21T23:00:00Z")] // Exact match
        [InlineAutoMoqData("2022-01-01T23:00:00Z", "9999-12-31T23:59:59Z", "2022-01-11T23:00:00Z", "9999-12-31T23:59:59Z")] // EndDate is DateTime.Max
        [InlineAutoMoqData("2022-01-10T23:00:00Z", null!, "2022-01-11T23:00:00Z", "9999-12-31T23:59:59Z")] // StartDate before existing, EndDate is null!
        [InlineAutoMoqData("2022-01-11T23:00:00Z", null!, "2022-01-11T23:00:00Z", "9999-12-31T23:59:59Z")] // StartDate equal to existing, EndDate is null!
        [InlineAutoMoqData("2022-01-12T23:00:00Z", null!, "2022-01-11T23:00:00Z", "9999-12-31T23:59:59Z")] // StartDate later than existing, EndDate is null!
        public void IsValid_WhenCalledWithOverlappingChargeLinks_ReturnsFalse(
            string newStartDate, string? newEndDate, string existingStartDate, string existingEndDate, string meteringPointId, DocumentDto document)
        {
            // Arrange
            var newChargeLinks = new List<ChargeLinkDto> { CreateChargeLinkDto(newStartDate, newEndDate) };
            var chargeLinkCommand = new ChargeLinksCommand(meteringPointId, document, newChargeLinks);
            var existingChargeLinks = GetExistingChargeLinks(existingStartDate, existingEndDate);

            var sut = new ChargeLinksUpdateNotYetSupportedRule(chargeLinkCommand, existingChargeLinks);

            // Act & Assert
            sut.IsValid.Should().BeFalse();
        }

        [Theory]
        [InlineAutoMoqData("2022-01-01T23:00:00Z", "2022-01-12T23:00:00Z")]
        public void IsValid_WhenExistingChargeLinksListIsEmpty_ReturnsTrue(
            string newStartDate, string? newEndDate, string meteringPointId, DocumentDto document)
        {
            // Arrange
            var newChargeLinks = new List<ChargeLinkDto> { CreateChargeLinkDto(newStartDate, newEndDate) };
            var chargeLinkCommand = new ChargeLinksCommand(meteringPointId, document, newChargeLinks);

            var sut = new ChargeLinksUpdateNotYetSupportedRule(chargeLinkCommand, new List<ChargeLink>());

            // Act & Assert
            sut.IsValid.Should().BeTrue();
        }

        private static ChargeLinkDto CreateChargeLinkDto(string newStartDate, string? newEndDate)
        {
            var startDate = InstantPattern.General.Parse(newStartDate).Value;
            Instant? endDateTime = newEndDate == null ? null : InstantPattern.General.Parse(newEndDate).Value;
            return new ChargeLinkDtoBuilder().WithStartDate(startDate).WithEndDate(endDateTime).Build();
        }

        private static IReadOnlyCollection<ChargeLink> GetExistingChargeLinks(
            string existingStartDate, string existingEndDate)
        {
            var startDate = InstantPattern.General.Parse(existingStartDate).Value;
            var endDate = InstantPattern.General.Parse(existingEndDate).Value;
            var link = new ChargeLinkBuilder().WithStartDate(startDate).WithEndDate(endDate).Build();

            return new List<ChargeLink> { link };
        }
    }
}
