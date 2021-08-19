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
using GreenEnergyHub.Charges.Domain.MarketDocument;
using GreenEnergyHub.Charges.Infrastructure.Context.Mapping;
using GreenEnergyHub.Charges.Infrastructure.Context.Model;
using GreenEnergyHub.Charges.Infrastructure.Mapping;
using GreenEnergyHub.Charges.TestCore;
using NodaTime;
using Xunit;
using Xunit.Categories;
using MarketParticipant = GreenEnergyHub.Charges.Infrastructure.Context.Model.MarketParticipant;

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
            var sut = ChargeMapper.MapChargeToChargeDomainModel(new Charge
            {
                ChargePeriodDetails =
                {
                    new ChargePeriodDetails
                    {
                        StartDateTime = future,
                        RowId = 1,
                    },
                    new ChargePeriodDetails
                    {
                        StartDateTime = past,
                        RowId = 2,
                    },
                    new ChargePeriodDetails
                    {
                        StartDateTime = expected,
                        RowId = 3,
                    },
                },
                MarketParticipant = new MarketParticipant
                {
                  MarketParticipantId = "id",
                },
                ChargeOperation = new ChargeOperation
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
            MarketParticipant marketParticipant)
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
            var result = ChargeMapper.MapDomainChargeToCharge(charge, marketParticipant);

            // Assert
            Assert.Null(result.ChargePeriodDetails.First().EndDateTime);
        }

        [Fact]
        public void MapChargeToChargeDomainModel_IfChargeIsNull_ThrowsArgumentNullException()
        {
            // Arrange
            Charge? charge = null;

            // Act / Assert
            Assert.Throws<ArgumentNullException>(
                    () => ChargeMapper.MapChargeToChargeDomainModel(charge!));
        }

        [Theory]
        [InlineAutoMoqData]
        public void MapDomainChargeToCharge_IfChargeIsNull_ThrowsArgumentNullException(
            [NotNull] MarketParticipant marketParticipant)
        {
            // Arrange
            GreenEnergyHub.Charges.Domain.Charge? charge = null;

            // Act / Assert
            Assert.Throws<ArgumentNullException>(
                () => ChargeMapper.MapDomainChargeToCharge(charge!, marketParticipant));
        }

        [Theory]
        [InlineAutoMoqData]
        public void MapDomainChargeToCharge_IfMarketParticipantIsNull_ThrowsArgumentNullException(
            [NotNull] GreenEnergyHub.Charges.Domain.Charge charge)
        {
            // Arrange
            MarketParticipant? marketParticipant = null;

            // Act / Assert
            Assert.Throws<ArgumentNullException>(
                () => ChargeMapper.MapDomainChargeToCharge(charge, marketParticipant!));
        }
    }
}
