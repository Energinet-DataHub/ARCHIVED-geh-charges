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
using GreenEnergyHub.Charges.Domain.Dtos.SharedDtos;
using GreenEnergyHub.Charges.Domain.MarketParticipants;
using GreenEnergyHub.Charges.Infrastructure.Core.Cim;
using GreenEnergyHub.Charges.Infrastructure.Core.Cim.MarketDocument;
using GreenEnergyHub.Charges.MessageHub.Infrastructure.Cim;
using GreenEnergyHub.Charges.MessageHub.Infrastructure.Cim.Bundles.ChargeLinkReceipt;
using GreenEnergyHub.Charges.MessageHub.Models.AvailableChargeLinksReceiptData;
using GreenEnergyHub.Charges.MessageHub.Models.AvailableData;
using GreenEnergyHub.Charges.TestCore;
using GreenEnergyHub.Charges.Tests.TestHelpers;
using GreenEnergyHub.TestHelpers;
using Moq;
using NodaTime;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.MessageHub.Infrastructure.Cim.Bundles.ChargeLinksReceipt
{
    [UnitTest]
    public class ChargeLinkReceiptCimSerializerTests
    {
        private const int NoOfReceiptsInBundle = 10;
        private const string CimTestId = "00000000000000000000000000000000";
        private const string RecipientId = "TestRecipient1111";

        [Theory]
        [InlineAutoDomainData(ReceiptStatus.Confirmed, "TestFiles/ExpectedOutputChargeLinkReceiptCimSerializerConfirmation.blob")]
        [InlineAutoDomainData(ReceiptStatus.Rejected, "TestFiles/ExpectedOutputChargeLinkReceiptCimSerializerRejection.blob")]
        public async Task SerializeAsync_WhenCalled_StreamHasSerializedResult(
            ReceiptStatus receiptStatus,
            string expectedFilePath,
            [Frozen] Mock<IMarketParticipantRepository> marketParticipantRepository,
            [Frozen] Mock<IClock> clock,
            [Frozen] Mock<ICimIdProvider> cimIdProvider,
            ChargeLinksReceiptCimSerializer sut)
        {
            // Arrange
            SetupMocks(marketParticipantRepository, clock, cimIdProvider);
            await using var stream = new MemoryStream();

            var path = FilePathHelper.GetFullFilePath(expectedFilePath);
            var expected = ContentStreamHelper.GetFileAsString(path);

            var receipts = GetReceipts(receiptStatus, clock.Object);

            // Act
            await sut.SerializeToStreamAsync(
                receipts,
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
            ChargeLinksReceiptCimSerializer sut)
        {
            SetupMocks(marketParticipantRepository, clock, cimIdProvider);

            var receipts = GetReceipts(ReceiptStatus.Rejected, clock.Object);

            await using var stream = new MemoryStream();

            await sut.SerializeToStreamAsync(
                receipts,
                stream,
                BusinessReasonCode.UpdateMasterDataSettlement,
                "senderId",
                MarketParticipantRole.MeteringPointAdministrator,
                RecipientId,
                MarketParticipantRole.GridAccessProvider);

            await using var fileStream = File.Create("C:\\Temp\\TestChargeLinkReceiptBundle" + Guid.NewGuid() + ".xml");

            await stream.CopyToAsync(fileStream);
        }

        private void SetupMocks(
            Mock<IMarketParticipantRepository> marketParticipantRepository,
            Mock<IClock> clock,
            Mock<ICimIdProvider> cimIdProvider)
        {
            marketParticipantRepository
                .Setup(r => r.GetMeteringPointAdministratorAsync())
                .ReturnsAsync(new MarketParticipant(
                    id: Guid.NewGuid(),
                    actorId: Guid.NewGuid(),
                    b2CActorId: Guid.NewGuid(),
                    "5790001330552",
                    true,
                    MarketParticipantRole.MeteringPointAdministrator));

            var currentTime = Instant.FromUtc(2021, 10, 12, 13, 37, 43).PlusNanoseconds(4);
            clock.Setup(c => c.GetCurrentInstant()).Returns(currentTime);

            cimIdProvider.Setup(c => c.GetUniqueId()).Returns(CimTestId);
        }

        private static List<AvailableChargeLinksReceiptData> GetReceipts(ReceiptStatus receiptStatus, IClock clock)
        {
            var chargeLinks = new List<AvailableChargeLinksReceiptData>();

            for (var i = 1; i <= NoOfReceiptsInBundle; i++)
            {
                chargeLinks.Add(GetReceipt(i, receiptStatus, clock));
            }

            return chargeLinks;
        }

        private static AvailableChargeLinksReceiptData GetReceipt(int no, ReceiptStatus receiptStatus, IClock clock)
        {
            return new AvailableChargeLinksReceiptData(
                "senderId",
                MarketParticipantRole.MeteringPointAdministrator,
                RecipientId,
                MarketParticipantRole.GridAccessProvider,
                BusinessReasonCode.UpdateMasterDataSettlement,
                clock.GetCurrentInstant(),
                Guid.NewGuid(),
                receiptStatus,
                "OriginalOperationId" + no,
                "MeteringPoint" + no,
                GetDocumentType(receiptStatus),
                0,
                Guid.NewGuid(),
                GetReasonCodes(no));
        }

        private static List<AvailableReceiptValidationError> GetReasonCodes(int no)
        {
            var reasonCodes = new List<AvailableReceiptValidationError>();
            var noOfReasons = (no % 3) + 1;

            for (var i = 1; i <= noOfReasons; i++)
            {
                var text = i % 2 == 0 ? $"Text{no}_{i}" : string.Empty;

                reasonCodes.Add(new AvailableReceiptValidationError(
                    ReasonCode.D14, // Matches that of the test file
                    text));
            }

            return reasonCodes;
        }

        private static DocumentType GetDocumentType(ReceiptStatus receiptStatus)
        {
            return receiptStatus == ReceiptStatus.Confirmed
                ? DocumentType.ConfirmRequestChangeBillingMasterData
                : DocumentType.RejectRequestChangeBillingMasterData;
        }
    }
}
