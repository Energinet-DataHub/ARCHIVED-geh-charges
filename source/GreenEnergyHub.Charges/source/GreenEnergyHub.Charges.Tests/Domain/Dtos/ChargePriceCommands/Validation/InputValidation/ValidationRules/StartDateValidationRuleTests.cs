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

using AutoFixture.Xunit2;
using FluentAssertions;
using GreenEnergyHub.Charges.Core.DateTime;
using GreenEnergyHub.Charges.Domain.Dtos.ChargePriceCommands.Validation.InputValidation.ValidationRules;
using GreenEnergyHub.Charges.Domain.Dtos.Validation;
using GreenEnergyHub.Charges.TestCore;
using GreenEnergyHub.Charges.TestCore.Attributes;
using GreenEnergyHub.Charges.Tests.Builders.Command;
using GreenEnergyHub.Iso8601;
using GreenEnergyHub.TestHelpers;
using NodaTime;
using NodaTime.Testing;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Domain.Dtos.ChargePriceCommands.Validation.InputValidation.ValidationRules
{
    [UnitTest]
    public class StartDateValidationRuleTests
    {
        [Theory]
        [InlineAutoMoqData(-1000, false)]
        [InlineAutoMoqData(0, true)]
        [InlineAutoMoqData(2000, false)]
        public void IsValid_WhenStartDateIsWithinInterval_IsTrue(
            int daysOffset,
            bool expected,
            [Frozen] ChargePriceOperationDtoBuilder builder)
        {
            // Arrange
            var effectiveDate = InstantHelper.GetTodayPlusDaysAtMidnightUtc(daysOffset);
            var clock = new FakeClock(InstantHelper.GetTodayAtMidnightUtc());
            var zonedDateTimeService = new ZonedDateTimeService(clock, new Iso8601ConversionConfiguration("Europe/Copenhagen"));
            var chargeOperationDto = builder
                .WithStartDateTime(effectiveDate)
                .Build();

            // Act (implicit)
            var sut = new StartDateValidationRule(chargeOperationDto, zonedDateTimeService, clock);

            // Assert
            sut.IsValid.Should().Be(expected);
        }

        [Theory]
        [InlineAutoDomainData]
        public void ValidationRuleIdentifier_ShouldBe_EqualTo(ChargePriceOperationDtoBuilder builder, IClock clock)
        {
            // Arrange
            var chargeOperationDto = builder.WithStartDateTime(InstantHelper.GetEndDefault()).Build();
            var zonedDateTimeService = new ZonedDateTimeService(clock, new Iso8601ConversionConfiguration("Europe/Copenhagen"));

            // Act (implicit)
            var sut = new StartDateValidationRule(chargeOperationDto, zonedDateTimeService, clock);

            // Assert
            sut.ValidationRuleIdentifier.Should().Be(ValidationRuleIdentifier.StartDateValidation);
        }
    }
}
