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
using GreenEnergyHub.Charges.Domain.MeteringPoints;
using GreenEnergyHub.Charges.Infrastructure.Context.Mapping;
using GreenEnergyHub.Charges.Infrastructure.Context.Model;
using GreenEnergyHub.TestHelpers.FluentAssertionsExtensions;
using NodaTime;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Infrastructure.Mapping
{
    [UnitTest]
    public class DefaultChargeLinkMapperTests
    {
        [Fact]
        public void MapDefaultChargeLinkSettingToDefaultChargeLink_WhenCalled_MapsAllProperties()
        {
            // Arrange
            var meteringPointCreatedDateTime = SystemClock.Instance.GetCurrentInstant();
            var settingStartDateTime = meteringPointCreatedDateTime.Plus(Duration.FromMinutes(3)).ToDateTimeUtc();
            var settingEndDateTime = meteringPointCreatedDateTime.Plus(Duration.FromHours(24)).ToDateTimeUtc();

            // Act
            var actual = DefaultChargeLinkMapper.Map(
                meteringPointCreatedDateTime,
                new DefaultChargeLinkSetting
                {
                    MeteringPointType = (int)MeteringPointType.Consumption,
                    StartDateTime = settingStartDateTime,
                    EndDateTime = settingEndDateTime,
                });

            // Assert
            actual.Should().NotContainNullsOrEmptyEnumerables();
        }
    }
}
