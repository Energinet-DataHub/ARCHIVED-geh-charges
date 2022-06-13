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
using System.Linq;
using Energinet.DataHub.Core.TestCommon.AutoFixture.Attributes;
using FluentAssertions;
using GreenEnergyHub.Charges.Domain.Charges;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommands.Validation.BusinessValidation.ValidationRules;
using GreenEnergyHub.Charges.Tests.Builders.Command;
using NodaTime;
using NodaTime.Text;
using Xunit;

namespace GreenEnergyHub.Charges.Tests.Domain.Dtos.ChargeCommands.Validation.BusinessValidation.ValidationRules
{
    public class UpdateChargeMustHaveEffectiveDateBeforeOrOnStopDateRuleTests
    {
        [Theory]
        [InlineAutoMoqData("2020-10-10T22:00:00Z", "2020-10-09T22:00:00Z", "it is valid as update is earlier than stop", true)]
        [InlineAutoMoqData("2020-10-10T22:00:00Z", "2020-10-10T22:00:00Z", "it is valid as update is the same as existing stop date", true)]
        [InlineAutoMoqData("9999-12-31T23:59:59Z", "9999-12-31T23:59:59Z", "it is valid as update and existing stop date is infinite", true)]
        [InlineAutoMoqData("2020-05-05T22:00:00Z", "2020-05-06T22:00:00Z", "it is invalid as the update is after existing stop date", false)]
        public void IsValid_WhenExpectedDateIsWithinInterval_IsTrue(
            string existingChargeEndDateIsoString,
            string incomingCommandStartDateIsoString,
            string reason,
            bool expectedIsValid)
        {
            // Arrange
            var existingStopDate = InstantPattern.General.Parse(existingChargeEndDateIsoString).Value;
            var incomingCommandStartDate = InstantPattern.General.Parse(incomingCommandStartDateIsoString).Value;

            var existingCharge = new ChargeBuilder()
                .AddPeriods(CreateExistingChargePeriods(existingStopDate))
                .WithStopDate(existingStopDate)
                .Build();

            var sut = new UpdateChargeMustHaveEffectiveDateBeforeOrOnStopDateRule(
                existingCharge.Periods.OrderBy(p => p.StartDateTime).Last().EndDateTime,
                incomingCommandStartDate);

            // Act
            var isValid = sut.IsValid;

            // Assert
            isValid.Should().Be(expectedIsValid, reason);
        }

        private List<ChargePeriod> CreateExistingChargePeriods(Instant existingStopDate)
        {
            var existingPeriod1 = new ChargePeriodBuilder()
                .WithStartDateTime(existingStopDate
                    .Minus(Duration.FromDays(20)))
                .Build();
            var existingPeriod2 = new ChargePeriodBuilder()
                .WithStartDateTime(existingStopDate
                    .Minus(Duration.FromDays(10)))
                .Build();

            return new List<ChargePeriod> { existingPeriod1, existingPeriod2 };
        }
    }
}
