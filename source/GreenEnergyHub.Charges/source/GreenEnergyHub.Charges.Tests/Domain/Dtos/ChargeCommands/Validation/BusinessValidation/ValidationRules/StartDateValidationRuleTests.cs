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
using GreenEnergyHub.Charges.Domain.Dtos.ChargeInformationCommands.Validation.InputValidation.ValidationRules;
using GreenEnergyHub.Charges.Domain.Dtos.Validation;
using GreenEnergyHub.Charges.TestCore;
using GreenEnergyHub.Charges.TestCore.Attributes;
using GreenEnergyHub.Charges.TestCore.TestHelpers;
using GreenEnergyHub.Charges.Tests.Builders.Command;
using GreenEnergyHub.TestHelpers;
using NodaTime;
using NodaTime.Testing;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Domain.Dtos.ChargeCommands.Validation.BusinessValidation.ValidationRules
{
    [UnitTest]
    public class StartDateValidationRuleTests
    {
        [Theory]
        [InlineAutoMoqData(-721, false)]
        [InlineAutoMoqData(-720, true)]
        [InlineAutoMoqData(0, true)]
        [InlineAutoMoqData(1095, true)]
        [InlineAutoMoqData(1096, false)]
        public void IsValid_WhenStartDateIsWithinInterval_IsTrue(
            int daysOffset,
            bool expected,
            [Frozen] ChargeInformationOperationDtoBuilder builder)
        {
            // Arrange
            var effectiveDate = InstantHelper.GetTodayPlusDaysAtMidnightUtc(daysOffset);
            var clock = new FakeClock(InstantHelper.GetTodayAtMidnightUtc());
            var zonedDateTimeService = ZonedDateTimeServiceHelper.GetZonedDateTimeService(InstantHelper.GetTodayAtMidnightUtc());
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
        public void ValidationRuleIdentifier_ShouldBe_EqualTo(ChargeInformationOperationDtoBuilder builder, IClock clock)
        {
            // Arrange
            var chargeOperationDto = builder.WithStartDateTime(InstantHelper.GetEndDefault()).Build();
            var zonedDateTimeService = ZonedDateTimeServiceHelper.GetZonedDateTimeService(InstantHelper.GetTodayAtMidnightUtc());

            // Act (implicit)
            var sut = new StartDateValidationRule(chargeOperationDto, zonedDateTimeService, clock);

            // Assert
            sut.ValidationRuleIdentifier.Should().Be(ValidationRuleIdentifier.StartDateValidation);
        }
    }
}
