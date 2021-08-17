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

using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using GreenEnergyHub.Charges.Infrastructure.Context.Mapping;
using GreenEnergyHub.Charges.Infrastructure.Context.Model;
using GreenEnergyHub.Charges.TestCore;
using NodaTime;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Infrastructure.Mapping
{
    [UnitTest]
    public class ChargeMapperTests
    {
        [Fact]
        public void MapChargeToChargeDomainModel_SetsCorrectStartDateTimeFromDetails()
        {
            // Arrange
            var expected = DateTime.UtcNow;
            var past = expected - TimeSpan.FromSeconds(3);
            var future = expected + TimeSpan.FromDays(3);

            // Act
            var sut = ChargeMapper.MapChargeContextModelToDomainModel(new DBCharge
            {
                ChargePeriodDetails =
                {
                    new DBChargePeriodDetails
                    {
                        StartDateTime = future,
                        RowId = 1,
                    },
                    new DBChargePeriodDetails
                    {
                        StartDateTime = past,
                        RowId = 2,
                    },
                    new DBChargePeriodDetails
                    {
                        StartDateTime = expected,
                        RowId = 3,
                    },
                },
                DBMarketParticipant = new DBMarketParticipant
                {
                  MarketParticipantId = "id",
                },
                DBChargeOperation = new DBChargeOperation
                {
                    ChargeOperationId = "ChargeOperationId",
                },
            });

            // Assert
            Assert.Equal(expected, sut.StartDateTime.ToDateTimeUtc());
        }

        [Theory]
        [InlineAutoMoqData]
        public void MapDomainChargeToCharge_WhenNoEndTimeIsUsed_MapsEndTimeToNull(
            [NotNull] Domain.Charge charge,
            DBMarketParticipant dbMarketParticipant)
        {
            // Arrange
            charge.EndDateTime = null;

            // Set all other times to a valid time and not just a random which can get the test to blink
            var now = SystemClock.Instance.GetCurrentInstant();
            charge.StartDateTime = now;
            charge.Document.RequestDate = now;
            foreach (var point in charge.Points)
            {
                point.Time = now;
            }

            // Act
            var result = ChargeMapper.MapDomainChargeToContextChargeModel(charge, dbMarketParticipant);

            // Assert
            Assert.Null(result.ChargePeriodDetails.First().EndDateTime);
        }

        [Fact]
        public void MapChargeToChargeDomainModel_IfChargeIsNull_ThrowsArgumentNullException()
        {
            // Arrange
            DBCharge? charge = null;

            // Act / Assert
            Assert.Throws<ArgumentNullException>(
                    () => ChargeMapper.MapChargeContextModelToDomainModel(charge!));
        }

        [Theory]
        [InlineAutoMoqData]
        public void MapDomainChargeToCharge_IfChargeIsNull_ThrowsArgumentNullException(
            [NotNull] DBMarketParticipant dbMarketParticipant)
        {
            // Arrange
            Domain.Charge? charge = null;

            // Act / Assert
            Assert.Throws<ArgumentNullException>(
                () => ChargeMapper.MapDomainChargeToContextChargeModel(charge!, dbMarketParticipant));
        }

        [Theory]
        [InlineAutoMoqData]
        public void MapDomainChargeToCharge_IfMarketParticipantIsNull_ThrowsArgumentNullException(
            [NotNull] Domain.Charge charge)
        {
            // Arrange
            DBMarketParticipant? marketParticipant = null;

            // Act / Assert
            Assert.Throws<ArgumentNullException>(
                () => ChargeMapper.MapDomainChargeToContextChargeModel(charge, marketParticipant!));
        }
    }
}
