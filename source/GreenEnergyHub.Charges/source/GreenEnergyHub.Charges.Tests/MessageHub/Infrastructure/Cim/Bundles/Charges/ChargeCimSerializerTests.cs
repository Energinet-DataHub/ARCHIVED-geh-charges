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
using System.Reflection;
using System.Threading.Tasks;
using AutoFixture.Xunit2;
using FluentAssertions;
using GreenEnergyHub.Charges.Domain.Charges;
using GreenEnergyHub.Charges.Domain.Dtos.SharedDtos;
using GreenEnergyHub.Charges.Domain.MarketParticipants;
using GreenEnergyHub.Charges.MessageHub.Infrastructure.Cim;
using GreenEnergyHub.Charges.MessageHub.Infrastructure.Cim.Bundles.Charges;
using GreenEnergyHub.Charges.MessageHub.Models.AvailableChargeData;
using GreenEnergyHub.Charges.TestCore;
using GreenEnergyHub.Iso8601;
using GreenEnergyHub.TestHelpers;
using Moq;
using NodaTime;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.MessageHub.Infrastructure.Cim.Bundles.Charges
{
    [UnitTest]
    public class ChargeCimSerializerTests
    {
        private const int NoOfChargesInBundle = 10;
        private const string CimTestId = "00000000000000000000000000000000";
        private const string RecipientId = "Recipient";

        [Theory]
        [InlineAutoDomainData("GreenEnergyHub.Charges.Tests.TestFiles.ExpectedOutputChargeCimSerializerMasterDataAndPrices.blob", true, true)] // TODO remove once D18 flow cant have prices anymore
        [InlineAutoDomainData("GreenEnergyHub.Charges.Tests.TestFiles.ExpectedOutputChargeCimSerializerChargeInformation.blob", true, false)]
        [InlineAutoDomainData("GreenEnergyHub.Charges.Tests.TestFiles.ExpectedOutputChargeCimSerializerChargePrices.blob", false, true)]
        public async Task SerializeAsync_WhenCalled_StreamHasSerializedResult(
            string embeddedResource,
            bool isChargeInformation,
            bool isChargePrices,
            [Frozen] Mock<IMarketParticipantRepository> marketParticipantRepository,
            [Frozen] Mock<IClock> clock,
            [Frozen] Mock<IIso8601Durations> iso8601Durations,
            [Frozen] Mock<ICimIdProvider> cimIdProvider,
            ChargeCimSerializer sut)
        {
            // Arrange
            SetupMocks(marketParticipantRepository, clock, iso8601Durations, cimIdProvider);
            await using var stream = new MemoryStream();

            var expected = EmbeddedStreamHelper.GetEmbeddedStreamAsString(
                Assembly.GetExecutingAssembly(),
                embeddedResource);

            var charges = GetCharges(clock.Object, isChargeInformation, isChargePrices);

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

            actual.Should().Be(expected);
        }

        [Theory(Skip = "Manually run test to save the generated file to disk")]
        [InlineAutoDomainData]
        public async Task SerializeAsync_WhenCalled_SaveSerializedStream(
            [Frozen] Mock<IMarketParticipantRepository> marketParticipantRepository,
            [Frozen] Mock<IClock> clock,
            [Frozen] Mock<IIso8601Durations> iso8601Durations,
            [Frozen] Mock<ICimIdProvider> cimIdProvider,
            ChargeCimSerializer sut)
        {
            SetupMocks(marketParticipantRepository, clock, iso8601Durations, cimIdProvider);

            var charges = GetCharges(clock.Object, false, true);

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

        private void SetupMocks(
            Mock<IMarketParticipantRepository> marketParticipantRepository,
            Mock<IClock> clock,
            Mock<IIso8601Durations> iso8601Durations,
            Mock<ICimIdProvider> cimIdProvider)
        {
            marketParticipantRepository
                .Setup(r => r.GetMeteringPointAdministratorAsync())
                .ReturnsAsync(new MarketParticipant(
                    id: Guid.NewGuid(),
                    b2CActorId: Guid.NewGuid(),
                    "5790001330552",
                    true,
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

        private static List<AvailableChargeData> GetCharges(IClock clock, bool isChargeInformation, bool isChargePrices)
        {
            var charges = new List<AvailableChargeData>();

            for (var i = 1; i <= NoOfChargesInBundle; i++)
            {
                var order = i - 1;
                if (isChargeInformation)
                {
                    charges.Add(GetChargeInformation(i, clock, isChargePrices, order));
                }
                else
                {
                    charges.Add(GetChargePrices(i, clock, isChargePrices));
                }
            }

            return charges;
        }

        private static AvailableChargeData GetChargeInformation(int no, IClock clock, bool includePrices, int order)
        {
            var validTo = no % 2 == 0 ?
                Instant.FromUtc(9999, 12, 31, 23, 59, 59) :
                Instant.FromUtc(2021, 4, 30, 22, 0, 0);

            return new AvailableChargeData(
                "5790001330552",
                MarketParticipantRole.MeteringPointAdministrator,
                RecipientId,
                MarketParticipantRole.GridAccessProvider,
                BusinessReasonCode.UpdateChargeInformation,
                clock.GetCurrentInstant(),
                Guid.NewGuid(),
                "ChargeId" + no,
                "Owner" + no,
                GetChargeType(no),
                "Name" + no,
                "Description" + no,
                Instant.FromUtc(2020, 12, 31, 23, 0, 0),
                validTo,
                VatClassification.NoVat,
                true,
                false,
                GetResolution(no),
                DocumentType.NotifyPriceList,
                order,
                Guid.NewGuid(),
                GetPoints(GetNoOfPoints(no, includePrices)));
        }

        private static AvailableChargeData GetChargePrices(int no, IClock clock, bool includePrices)
        {
            return new AvailableChargeData(
                "5790001330552",
                MarketParticipantRole.MeteringPointAdministrator,
                "Recipient",
                MarketParticipantRole.GridAccessProvider,
                BusinessReasonCode.UpdateChargePrices,
                clock.GetCurrentInstant(),
                Guid.NewGuid(),
                "ChargeId" + no,
                "Owner" + no,
                GetChargeType(no),
                string.Empty,
                string.Empty,
                Instant.FromUtc(2020, 12, 31, 23, 0, 0),
                Instant.FromUtc(9999, 12, 31, 23, 59, 59),
                VatClassification.Unknown,
                false,
                false,
                GetResolution(no),
                DocumentType.NotifyPriceList,
                0,
                Guid.NewGuid(),
                GetPoints(GetNoOfPoints(no, includePrices)));
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

        private static int GetNoOfPoints(int no, bool includePrices)
        {
            if (!includePrices)
            {
                return 0;
            }

            return (no % 3) switch
            {
                0 => 1,
                1 => 1,
                _ => 24,
            };
        }

        private static List<AvailableChargeDataPoint> GetPoints(int noOfPoints)
        {
            var points = new List<AvailableChargeDataPoint>();

            for (int i = 1; i <= noOfPoints; i++)
            {
                points.Add(new AvailableChargeDataPoint(i, i));
            }

            return points;
        }
    }
}
