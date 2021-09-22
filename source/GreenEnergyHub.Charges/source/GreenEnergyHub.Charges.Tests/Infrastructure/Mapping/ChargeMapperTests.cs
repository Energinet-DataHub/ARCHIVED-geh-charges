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
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using GreenEnergyHub.Charges.Domain.Charges;
using GreenEnergyHub.Charges.Domain.MarketDocument;
using GreenEnergyHub.Charges.Infrastructure.Context.Mapping;
using GreenEnergyHub.Charges.Infrastructure.Context.Model;
using GreenEnergyHub.Charges.TestCore;
using GreenEnergyHub.Charges.TestCore.Attributes;
using NodaTime;
using Xunit;
using Xunit.Categories;
using Charge = GreenEnergyHub.Charges.Infrastructure.Context.Model.Charge;
using ChargeOperation = GreenEnergyHub.Charges.Infrastructure.Context.Model.ChargeOperation;
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
                    },
                    new ChargePeriodDetails
                    {
                        StartDateTime = past,
                    },
                    new ChargePeriodDetails
                    {
                        StartDateTime = expected,
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
            MarketParticipant marketParticipant)
        {
            // Arrange
            var now = SystemClock.Instance.GetCurrentInstant();

            // Set all other times to a valid time and not just a random which can get the test to blink
            var charge = new Charges.Domain.Charges.Charge(
                Guid.NewGuid(),
                new Document
                {
                    RequestDate = now,
                },
                "ChargeOperationId",
                "SenderProvidedId",
                "Name",
                "description",
                "owner",
                "CorrelationId",
                now,
                null,
                ChargeType.Fee,
                VatClassification.Unknown,
                Resolution.P1D,
                true,
                false,
                new List<Point>());

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
            Charges.Domain.Charges.Charge? charge = null;

            // Act / Assert
            Assert.Throws<ArgumentNullException>(
                () => ChargeMapper.MapDomainChargeToCharge(charge!, marketParticipant));
        }

        [Theory]
        [InlineAutoMoqData]
        public void MapDomainChargeToCharge_IfMarketParticipantIsNull_ThrowsArgumentNullException(
            [NotNull] Charges.Domain.Charges.Charge charge)
        {
            // Arrange
            MarketParticipant? marketParticipant = null;

            // Act / Assert
            Assert.Throws<ArgumentNullException>(
                () => ChargeMapper.MapDomainChargeToCharge(charge, marketParticipant!));
        }
    }
}
