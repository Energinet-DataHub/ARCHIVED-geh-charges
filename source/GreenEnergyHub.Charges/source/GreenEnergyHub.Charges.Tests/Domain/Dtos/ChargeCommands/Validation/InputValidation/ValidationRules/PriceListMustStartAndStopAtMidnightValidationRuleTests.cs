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
using GreenEnergyHub.Charges.Core.DateTime;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeInformationCommands.Validation.InputValidation.ValidationRules;
using GreenEnergyHub.Charges.Domain.Dtos.Validation;
using GreenEnergyHub.Charges.TestCore;
using GreenEnergyHub.Charges.Tests.Builders.Command;
using GreenEnergyHub.Iso8601;
using GreenEnergyHub.TestHelpers;
using NodaTime.Testing;
using NodaTime.Text;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Domain.Dtos.ChargeCommands.Validation.InputValidation.ValidationRules
{
    [UnitTest]
    public class PriceListMustStartAndStopAtMidnightValidationRuleTests
    {
        [Theory]
        [InlineAutoMoqData("2022-07-01T22:00:00Z", "2022-07-01T22:00:00Z", true, "both start and end are DST")]
        [InlineAutoMoqData("2022-12-31T23:00:00Z", "2023-01-01T23:00:00Z", true, "both start and end are normal time")]
        [InlineAutoMoqData("2022-12-31T23:00:00Z", "2022-12-31T23:00:00Z", true, "both start and end are normal time, same date is allowed")]
        [InlineAutoMoqData("2022-03-26T23:00:00Z", "2022-03-27T22:00:00Z", true, "start is normal time, end date is DST")]
        [InlineAutoMoqData("2022-10-29T22:00:00Z", "2022-10-30T23:00:00Z", true, "start is DST time, end date is normal time")]
        [InlineAutoMoqData("2022-10-29T00:00:00Z", "2022-11-02T23:00:00Z", false, "start date is not midnight in DST")]
        [InlineAutoMoqData("2022-10-30T23:00:00Z", "2022-11-01T00:00:00Z", false, "end date is not midnight in normal time")]
        public void IsValid_WhenCalledStartAndEndDate_ShouldReturnExpectedResult(
            string startDateISOString,
            string endDateISOString,
            bool expected,
            string reasonText,
            ChargeInformationOperationDtoBuilder chargeInformationOperationDtoBuilder)
        {
            // Arrange
            var startDate = InstantPattern.General.Parse(startDateISOString).Value;
            var endDate = InstantPattern.General.Parse(endDateISOString).Value;
            var dto = chargeInformationOperationDtoBuilder.WithPointsInterval(startDate, endDate).Build();
            var zonedDateTimeService = GetZonedDateTimeService();
            var sut = new PriceListMustStartAndStopAtMidnightValidationRule(zonedDateTimeService, dto);

            // Act & Assert
            sut.IsValid.Should().Be(expected, reasonText);
        }

        [Theory]
        [InlineAutoDomainData]
        public void ValidationRuleIdentifier_ShouldBe_EqualTo(ChargeInformationOperationDtoBuilder chargeInformationOperationDtoBuilder)
        {
            var chargeOperationDto = chargeInformationOperationDtoBuilder.Build();
            var zonedDateTimeService = GetZonedDateTimeService();
            var sut = new PriceListMustStartAndStopAtMidnightValidationRule(zonedDateTimeService, chargeOperationDto);
            sut.ValidationRuleIdentifier.Should().Be(ValidationRuleIdentifier.PriceListMustStartAndStopAtMidnightValidationRule);
        }

        private static ZonedDateTimeService GetZonedDateTimeService()
        {
            var clock = new FakeClock(InstantHelper.GetTodayAtMidnightUtc());
            return new ZonedDateTimeService(clock, new Iso8601ConversionConfiguration("Europe/Copenhagen"));
        }
    }
}
