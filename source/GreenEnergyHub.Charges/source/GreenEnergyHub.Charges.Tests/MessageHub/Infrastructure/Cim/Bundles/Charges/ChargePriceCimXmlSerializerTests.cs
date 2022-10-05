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
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture.Xunit2;
using GreenEnergyHub.Charges.Domain.Charges;
using GreenEnergyHub.Charges.Domain.Dtos.SharedDtos;
using GreenEnergyHub.Charges.Domain.MarketParticipants;
using GreenEnergyHub.Charges.MessageHub.Infrastructure.Cim;
using GreenEnergyHub.Charges.MessageHub.Infrastructure.Cim.Bundles.Charges;
using GreenEnergyHub.Charges.MessageHub.Models.AvailableChargeData;
using GreenEnergyHub.Charges.TestCore;
using GreenEnergyHub.Charges.Tests.TestHelpers;
using GreenEnergyHub.Iso8601;
using GreenEnergyHub.TestHelpers;
using Moq;
using NodaTime;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.MessageHub.Infrastructure.Cim.Bundles.Charges
{
    [UnitTest]
    public class ChargePriceCimXmlSerializerTests
    {
        private const int NoOfChargesInBundle = 10;
        private const string CimTestId = "00000000000000000000000000000000";
        private const string RecipientId = "Recipient";

        [Theory]
        [InlineAutoDomainData("TestFiles/ExpectedOutputChargeCimSerializerChargePrices.blob")]
        public async Task SerializeAsync_WhenCalled_StreamHasSerializedResult(
            string expectedFilePath,
            [Frozen] Mock<IMarketParticipantRepository> marketParticipantRepository,
            [Frozen] Mock<IClock> clock,
            [Frozen] Mock<IIso8601Durations> iso8601Durations,
            [Frozen] Mock<ICimIdProvider> cimIdProvider,
            ChargePriceCimXmlSerializer sut)
        {
            // Arrange
            SetupMocks(marketParticipantRepository, clock, iso8601Durations, cimIdProvider);
            await using var stream = new MemoryStream();

            var path = FilePathHelper.GetFullFilePath(expectedFilePath);
            var expected = ContentStreamHelper.GetFileAsString(path);

            var charges = GetCharges(clock.Object);

            // Act
            await sut.SerializeToStreamAsync(
                charges,
                stream,
                charges.First().BusinessReasonCode,
                "5790001330552",
                MarketParticipantRole.MeteringPointAdministrator,
                RecipientId,
                MarketParticipantRole.GridAccessProvider);

            // Assert
            var actual = stream.AsString();

            Assert.Equal(expected, actual, ignoreLineEndingDifferences: true);
        }

        [Theory(Skip = "Manually run test to save the generated file to disk")]
        [InlineAutoDomainData]
        public async Task SerializeAsync_WhenCalled_SaveSerializedStream(
            [Frozen] Mock<IMarketParticipantRepository> marketParticipantRepository,
            [Frozen] Mock<IClock> clock,
            [Frozen] Mock<IIso8601Durations> iso8601Durations,
            [Frozen] Mock<ICimIdProvider> cimIdProvider,
            ChargePriceCimXmlSerializer sut)
        {
            SetupMocks(marketParticipantRepository, clock, iso8601Durations, cimIdProvider);

            var charges = GetCharges(clock.Object);

            await using var stream = new MemoryStream();

            await sut.SerializeToStreamAsync(
                charges,
                stream,
                BusinessReasonCode.UpdateChargeInformation,
                "5790001330552",
                MarketParticipantRole.MeteringPointAdministrator,
                RecipientId,
                MarketParticipantRole.GridAccessProvider);

            await using var fileStream = File.Create("C:\\Temp\\TestChargeBundle" + Guid.NewGuid() + ".xml");

            await stream.CopyToAsync(fileStream);
        }

        private static void SetupMocks(
            Mock<IMarketParticipantRepository> marketParticipantRepository,
            Mock<IClock> clock,
            Mock<IIso8601Durations> iso8601Durations,
            Mock<ICimIdProvider> cimIdProvider)
        {
            marketParticipantRepository
                .Setup(r => r.GetMeteringPointAdministratorAsync())
                .ReturnsAsync(new MarketParticipant(
                    id: Guid.NewGuid(),
                    actorId: Guid.NewGuid(),
                    b2CActorId: Guid.NewGuid(),
                    "5790001330552",
                    MarketParticipantStatus.Active,
                    MarketParticipantRole.MeteringPointAdministrator));

            var currentTime = Instant.FromUtc(2021, 10, 22, 15, 30, 41).PlusNanoseconds(4);
            clock.Setup(c => c.GetCurrentInstant()).Returns(currentTime);

            iso8601Durations
                .Setup(i => i.GetTimeFixedToDuration(
                        It.IsAny<Instant>(),
                        It.IsAny<string>(),
                        It.IsAny<int>()))
                .Returns(Instant.FromUtc(2100, 3, 31, 22, 0, 0));

            cimIdProvider.Setup(c => c.GetUniqueId()).Returns(CimTestId);
        }

        private static List<AvailableChargePriceData> GetCharges(IClock clock)
        {
            var charges = new List<AvailableChargePriceData>();

            for (var i = 1; i <= NoOfChargesInBundle; i++)
            {
                charges.Add(GetChargePrices(i, clock));
            }

            return charges;
        }

        private static AvailableChargePriceData GetChargePrices(int no, IClock clock)
        {
            return new AvailableChargePriceData(
                "5790001330552",
                MarketParticipantRole.MeteringPointAdministrator,
                "Recipient",
                MarketParticipantRole.GridAccessProvider,
                businessReasonCode: BusinessReasonCode.UpdateChargePrices,
                clock.GetCurrentInstant(),
                Guid.NewGuid(),
                "ChargeId" + no,
                "Owner" + no,
                GetChargeType(no),
                Instant.FromUtc(2020, 12, 31, 23, 0, 0),
                GetResolution(no),
                DocumentType.NotifyPriceList,
                0,
                Guid.NewGuid(),
                GetPoints(GetNoOfPoints(no)));
        }

        private static ChargeType GetChargeType(int no)
        {
            return (no % 3) switch
            {
                0 => ChargeType.Subscription,
                1 => ChargeType.Fee,
                _ => ChargeType.Tariff,
            };
        }

        private static Resolution GetResolution(int no)
        {
            return (no % 3) switch
            {
                0 => Resolution.P1M,
                1 => Resolution.P1M,
                _ => Resolution.PT1H,
            };
        }

        private static int GetNoOfPoints(int no)
        {
            return (no % 3) switch
            {
                0 => 1,
                1 => 1,
                _ => 24,
            };
        }

        private static List<AvailableChargePriceDataPoint> GetPoints(int noOfPoints)
        {
            var points = new List<AvailableChargePriceDataPoint>();

            for (int i = 1; i <= noOfPoints; i++)
            {
                points.Add(new AvailableChargePriceDataPoint(i, i));
            }

            return points;
        }
    }
}
