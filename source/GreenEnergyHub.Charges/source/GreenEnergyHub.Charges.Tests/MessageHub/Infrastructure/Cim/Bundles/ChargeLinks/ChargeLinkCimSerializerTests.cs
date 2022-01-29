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
using System.Reflection;
using System.Threading.Tasks;
using AutoFixture.Xunit2;
using GreenEnergyHub.Charges.Domain.Charges;
using GreenEnergyHub.Charges.Domain.MarketParticipants;
using GreenEnergyHub.Charges.MessageHub.Infrastructure.Cim;
using GreenEnergyHub.Charges.MessageHub.Infrastructure.Cim.Bundles.ChargeLinks;
using GreenEnergyHub.Charges.MessageHub.Models.AvailableChargeLinksData;
using GreenEnergyHub.Charges.TestCore;
using GreenEnergyHub.TestHelpers;
using Moq;
using NodaTime;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.MessageHub.Infrastructure.Cim.Bundles.ChargeLinks
{
    [UnitTest]
    public class ChargeLinkCimSerializerTests
    {
        private const int NoOfLinksInBundle = 10;
        private const string CimTestId = "00000000000000000000000000000000";
        private const string RecipientId = "TestRecipient1111";

        [Theory]
        [InlineAutoDomainData]
        public async Task SerializeAsync_WhenCalled_StreamHasSerializedResult(
            [Frozen] Mock<IMarketParticipantRepository> marketParticipantRepository,
            [Frozen] Mock<IClock> clock,
            [Frozen] Mock<ICimIdProvider> cimIdProvider,
            ChargeLinkCimSerializer sut)
        {
            // Arrange
            SetupMocks(marketParticipantRepository, clock, cimIdProvider);
            await using var stream = new MemoryStream();

            var expected = EmbeddedStreamHelper.GetEmbeddedStreamAsString(
                Assembly.GetExecutingAssembly(),
                "GreenEnergyHub.Charges.Tests.TestFiles.ExpectedOutputChargeLinkCimSerializer.blob");

            var chargeLinks = GetChargeLinks(clock.Object);

            // Act
            await sut.SerializeToStreamAsync(
                chargeLinks,
                stream,
                BusinessReasonCode.UpdateMasterDataSettlement,
                "5790001330552",
                MarketParticipantRole.MeteringPointAdministrator,
                RecipientId,
                MarketParticipantRole.GridAccessProvider);

            // Assert
            var actual = stream.AsString();

            Assert.Equal(expected, actual);
        }

        [Theory(Skip = "Manually run test to save the generated file to disk")]
        [InlineAutoDomainData]
        public async Task SerializeAsync_WhenCalled_SaveSerializedStream(
            [Frozen] Mock<IMarketParticipantRepository> marketParticipantRepository,
            [Frozen] Mock<IClock> clock,
            [Frozen] Mock<ICimIdProvider> cimIdProvider,
            ChargeLinkCimSerializer sut)
        {
            SetupMocks(marketParticipantRepository, clock, cimIdProvider);

            var chargeLinks = GetChargeLinks(clock.Object);

            await using var stream = new MemoryStream();

            await sut.SerializeToStreamAsync(
                chargeLinks,
                stream,
                BusinessReasonCode.UpdateMasterDataSettlement,
                "5790001330552",
                MarketParticipantRole.MeteringPointAdministrator,
                RecipientId,
                MarketParticipantRole.GridAccessProvider);

            await using var fileStream = File.Create("C:\\Temp\\TestChargeLinkBundle" + Guid.NewGuid() + ".xml");

            await stream.CopyToAsync(fileStream);
        }

        private void SetupMocks(
            [Frozen] Mock<IMarketParticipantRepository> marketParticipantRepository,
            Mock<IClock> clock,
            Mock<ICimIdProvider> cimIdProvider)
        {
            marketParticipantRepository
                .Setup(r => r.GetMeteringPointAdministratorAsync())
                .ReturnsAsync(new MarketParticipant(Guid.NewGuid(), "5790001330552", true, MarketParticipantRole.MeteringPointAdministrator));

            var currentTime = Instant.FromUtc(2021, 10, 12, 13, 37, 43).PlusNanoseconds(4);
            clock.Setup(c => c.GetCurrentInstant()).Returns(currentTime);

            cimIdProvider.Setup(c => c.GetUniqueId()).Returns(CimTestId);
        }

        private List<AvailableChargeLinksData> GetChargeLinks(IClock clock)
        {
            var chargeLinks = new List<AvailableChargeLinksData>();

            for (var i = 1; i <= NoOfLinksInBundle; i++)
            {
                chargeLinks.Add(GetChargeLink(i, clock));
            }

            return chargeLinks;
        }

        private AvailableChargeLinksData GetChargeLink(int no, IClock clock)
        {
            var validTo = no % 2 == 0 ?
                Instant.FromUtc(9999, 12, 31, 23, 59, 59) :
                Instant.FromUtc(2021, 4, 30, 22, 0, 0);

            return new AvailableChargeLinksData(
                "5790001330552",
                MarketParticipantRole.MeteringPointAdministrator,
                RecipientId,
                MarketParticipantRole.GridAccessProvider,
                BusinessReasonCode.UpdateMasterDataSettlement,
                clock.GetCurrentInstant(),
                Guid.NewGuid(),
                "Charge" + no,
                "Owner" + no,
                ChargeType.Tariff,
                "MeteringPoint" + no,
                no,
                Instant.FromUtc(2020, 12, 31, 23, 0, 0),
                validTo);
        }
    }
}
