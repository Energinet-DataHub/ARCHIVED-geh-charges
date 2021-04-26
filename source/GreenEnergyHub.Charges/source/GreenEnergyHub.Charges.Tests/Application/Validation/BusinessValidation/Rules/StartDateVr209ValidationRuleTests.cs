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

using GreenEnergyHub.Charges.Application.Validation.BusinessValidation.Rules;
using GreenEnergyHub.Charges.Core;
using GreenEnergyHub.Charges.Core.DateTime;
using GreenEnergyHub.Charges.Domain.ChangeOfCharges.Transaction;
using GreenEnergyHub.Iso8601;
using Moq;
using NodaTime;
using NodaTime.Text;
using Xunit;

namespace GreenEnergyHub.Charges.Tests.Application.Validation.BusinessValidation.Rules
{
    public class StartDateVr209ValidationRuleTests
    {
        [Theory]
        // Test that start of interval is inclusive
        [InlineData("2020-05-06T21:59:59Z", "2020-05-10T13:00:00Z", "Europe/Copenhagen", 3, 1, false)]
        [InlineData("2020-05-06T22:00:00Z", "2020-05-10T13:00:00Z", "Europe/Copenhagen", 3, 1, true)]
        // Test that end of interval is inclusive
        [InlineData("2020-05-08T22:00:00Z", "2020-05-10T13:00:00Z", "Europe/Copenhagen", 3, 1, true)]
        [InlineData("2020-05-08T22:00:01Z", "2020-05-10T13:00:00Z", "Europe/Copenhagen", 3, 1, false)]
        public void IsValid_WhenStartDateIsWithinInterval_IsTrue(
            string effectuationDateIsoString,
            string nowIsoString,
            string timeZoneId,
            int startOfOccurrence,
            int endOfOccurrence,
            bool expected)
        {
            // Arrange
            var command = CreateCommand(effectuationDateIsoString);
            var configuration = CreateRuleConfiguration(startOfOccurrence, endOfOccurrence);
            var zonedDateTimeService = CreateLocalDateTimeService(nowIsoString, timeZoneId);

            // Act (implicit)
            var sut = new StartDateVr209ValidationRule(command, configuration, zonedDateTimeService);

            // Assert
            Assert.Equal(expected, sut.IsValid);
        }

        private static ZonedDateTimeService CreateLocalDateTimeService(string nowIsoString, string timeZoneId)
        {
            var clock = new Mock<IClock>();
            clock
                .Setup(c => c.GetCurrentInstant())
                .Returns(InstantPattern.General.Parse(nowIsoString).Value);
            return new ZonedDateTimeService(clock.Object, new Iso8601ConversionConfiguration(timeZoneId));
        }

        private static StartDateVr209ValidationRuleConfiguration CreateRuleConfiguration(
            int startOfOccurrence,
            int endOfOccurrence)
        {
            var configuration =
                new StartDateVr209ValidationRuleConfiguration(new Interval<int>(startOfOccurrence, endOfOccurrence));
            return configuration;
        }

        private static ChargeCommand CreateCommand(string effectuationDateIsoString)
        {
            return new ChargeCommand
            {
                MktActivityRecord = new MktActivityRecord
                {
                    ValidityStartDate = InstantPattern.General.Parse(effectuationDateIsoString).Value,
                },
            };
        }
    }
}
