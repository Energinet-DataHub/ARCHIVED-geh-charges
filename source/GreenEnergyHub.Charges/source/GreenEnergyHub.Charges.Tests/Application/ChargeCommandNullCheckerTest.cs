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
using GreenEnergyHub.Charges.Application;
using GreenEnergyHub.Charges.Domain.ChangeOfCharges.Transaction;
using GreenEnergyHub.Charges.Domain.Common;
using GreenEnergyHub.TestHelpers;
using NodaTime;
using Xunit;
using MarketParticipant = GreenEnergyHub.Charges.Domain.Common.MarketParticipant;
using MarketParticipantRole = GreenEnergyHub.Charges.Domain.Common.MarketParticipantRole;

namespace GreenEnergyHub.Charges.Tests.Application
{
    public class ChargeCommandNullCheckerTest
    {
        [Theory]
        [InlineAutoDomainData(null, "Valid", "Valid", "Valid", "Valid", "Valid", "Valid", "Valid", "Valid", "Valid", "Valid", "Valid", "Valid")]
        [InlineAutoDomainData("valid", null, "Valid", "Valid", "Valid", "Valid", "Valid", "Valid", "Valid", "Valid", "Valid", "Valid", "Valid")]
        [InlineAutoDomainData("valid", "Valid", null, "Valid", "Valid", "Valid", "Valid", "Valid", "Valid", "Valid", "Valid", "Valid", "Valid")]
        [InlineAutoDomainData("valid", "Valid", "Valid", null, "Valid", "Valid", "Valid", "Valid", "Valid", "Valid", "Valid", "Valid", "Valid")]
        [InlineAutoDomainData("valid", "Valid", "Valid", "Valid", null, "Valid", "Valid", "Valid", "Valid", "Valid", "Valid", "Valid", "Valid")]
        [InlineAutoDomainData("valid", "Valid", "Valid", "Valid", "Valid", null, "Valid", "Valid", "Valid", "Valid", "Valid", "Valid", "Valid")]
        [InlineAutoDomainData("valid", "Valid", "Valid", "Valid", "Valid", "Valid", null, "Valid", "Valid", "Valid", "Valid", "Valid", "Valid")]
        [InlineAutoDomainData("valid", "Valid", "Valid", "Valid", "Valid", "Valid", "Valid", null, "Valid", "Valid", "Valid", "Valid", "Valid")]
        [InlineAutoDomainData("valid", "Valid", "Valid", "Valid", "Valid", "Valid", "Valid", "Valid", null, "Valid", "Valid", "Valid", "Valid")]
        [InlineAutoDomainData("valid", "Valid", "Valid", "Valid", "Valid", "Valid", "Valid", "Valid", "Valid", null, "Valid", "Valid", "Valid")]
        [InlineAutoDomainData("valid", "Valid", "Valid", "Valid", "Valid", "Valid", "Valid", "Valid", "Valid", "Valid", null, "Valid", "Valid")]
        [InlineAutoDomainData("valid", "Valid", "Valid", "Valid", "Valid", "Valid", "Valid", "Valid", "Valid", "Valid", "Valid", null, "Valid")]
        [InlineAutoDomainData("valid", "Valid", "Valid", "Valid", "Valid", "Valid", "Valid", "Valid", "Valid", "Valid", "Valid", "Valid", null)]
        [InlineAutoDomainData("", "Valid", "Valid", "Valid", "Valid", "Valid", "Valid", "Valid", "Valid", "Valid", "Valid", "Valid", "Valid")]
        [InlineAutoDomainData("valid", "", "Valid", "Valid", "Valid", "Valid", "Valid", "Valid", "Valid", "Valid", "Valid", "Valid", "Valid")]
        [InlineAutoDomainData("valid", "Valid", "", "Valid", "Valid", "Valid", "Valid", "Valid", "Valid", "Valid", "Valid", "Valid", "Valid")]
        [InlineAutoDomainData("valid", "Valid", "Valid", "", "Valid", "Valid", "Valid", "Valid", "Valid", "Valid", "Valid", "Valid", "Valid")]
        [InlineAutoDomainData("valid", "Valid", "Valid", "Valid", "", "Valid", "Valid", "Valid", "Valid", "Valid", "Valid", "Valid", "Valid")]
        [InlineAutoDomainData("valid", "Valid", "Valid", "Valid", "Valid", "", "Valid", "Valid", "Valid", "Valid", "Valid", "Valid", "Valid")]
        [InlineAutoDomainData("valid", "Valid", "Valid", "Valid", "Valid", "Valid", "", "Valid", "Valid", "Valid", "Valid", "Valid", "Valid")]
        [InlineAutoDomainData("valid", "Valid", "Valid", "Valid", "Valid", "Valid", "Valid", "", "Valid", "Valid", "Valid", "Valid", "Valid")]
        [InlineAutoDomainData("valid", "Valid", "Valid", "Valid", "Valid", "Valid", "Valid", "Valid", "", "Valid", "Valid", "Valid", "Valid")]
        [InlineAutoDomainData("valid", "Valid", "Valid", "Valid", "Valid", "Valid", "Valid", "Valid", "Valid", "", "Valid", "Valid", "Valid")]
        [InlineAutoDomainData("valid", "Valid", "Valid", "Valid", "Valid", "Valid", "Valid", "Valid", "Valid", "Valid", "", "Valid", "Valid")]
        [InlineAutoDomainData("valid", "Valid", "Valid", "Valid", "Valid", "Valid", "Valid", "Valid", "Valid", "Valid", "Valid", "", "Valid")]
        [InlineAutoDomainData("valid", "Valid", "Valid", "Valid", "Valid", "Valid", "Valid", "Valid", "Valid", "Valid", "Valid", "Valid", "")]
        public void ChargeCommandPropertiesAreNotNullOrWhitespace(
            string chargeTypeMRid,
            string correlationId,
            string lastUpdatedBy,
            string chargeTypeOwnerId,
            string type,
            string resolution,
            string marketDocumentMRid,
            string senderMarketDocumentMRid,
            string receiverMarketDocumentMRid,
            string mktActivityRecordMRid,
            string chargeTypeDescription,
            string chargeTypeVatPayer,
            string chargeTypeName)
        {
            // Arrange
            var c = GetValidCharge();
            c.ChargeTypeMRid = chargeTypeMRid;
            c.ChargeTypeOwnerMRid = chargeTypeOwnerId;
            c.CorrelationId = correlationId;
            c.LastUpdatedBy = lastUpdatedBy;
            c.Type = type;
            c.Period.Resolution = resolution;
            c.MarketDocument.MRid = marketDocumentMRid;
            c.MarketDocument.SenderMarketParticipant.MRid = senderMarketDocumentMRid;
            c.MarketDocument.ReceiverMarketParticipant.MRid = receiverMarketDocumentMRid;
            c.MktActivityRecord.MRid = mktActivityRecordMRid;
            c.MktActivityRecord.ChargeType.Description = chargeTypeDescription;
            c.MktActivityRecord.ChargeType.VatPayer = chargeTypeVatPayer;
            c.MktActivityRecord.ChargeType.Name = chargeTypeName;

            // Act & Assert
            Assert.Throws<ArgumentException>(() => ChargeCommandNullChecker.ThrowExceptionIfRequiredPropertyIsNull(c));
        }

        [Fact]
        public void ChargeCommandPeriodIsNullThrowsException()
        {
            // Arrange
            var c = GetValidCharge();
            c.Period = null!;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => ChargeCommandNullChecker.ThrowExceptionIfRequiredPropertyIsNull(c));
        }

        [Fact]
        public void ChargeCommandMarketDocumentIsNullThrowsException()
        {
            // Arrange
            var c = GetValidCharge();
            c.MarketDocument = null!;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => ChargeCommandNullChecker.ThrowExceptionIfRequiredPropertyIsNull(c));
        }

        [Fact]
        public void ChargeCommandMktActivityRecordIsNullThrowsException()
        {
            // Arrange
            var c = GetValidCharge();
            c.MktActivityRecord = null!;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => ChargeCommandNullChecker.ThrowExceptionIfRequiredPropertyIsNull(c));
        }

        private static ChargeCommand GetValidCharge()
        {
            return new ()
            {
                Type = "D01",
                CorrelationId = "CorrelationId",
                RequestDate = SystemClock.Instance.GetCurrentInstant(),
                LastUpdatedBy = "LastUpdatedBy",
                ChargeTypeMRid = "ChargeTypeMrid",
                ChargeTypeOwnerMRid = "ChargeTypeOwnerMRid",
                Period = new ChargeTypePeriod
                {
                    Points = new List<Point>
                    {
                        new Point { Position = 0, Time = SystemClock.Instance.GetCurrentInstant(), PriceAmount = 200m },
                    },
                    Resolution = "Resolution",
                },
                MarketDocument = new MarketDocument
                {
                    MRid = "MRid",
                    ProcessType = ProcessType.UpdateChargeInformation,
                    CreatedDateTime = SystemClock.Instance.GetCurrentInstant(),
                    ReceiverMarketParticipant = new MarketParticipant
                    {
                        Id = 0,
                        Name = "Name",
                        Role = MarketParticipantRole.EnergySupplier,
                        MRid = "MRid",
                    },
                    SenderMarketParticipant = new MarketParticipant
                    {
                        Id = 1,
                        Name = "Name",
                        Role = MarketParticipantRole.EnergySupplier,
                        MRid = "MRid",
                    },
                    MarketServiceCategoryKind = ServiceCategoryKind.Electricity,
                },
                MktActivityRecord = new MktActivityRecord
                {
                    Status = MktActivityRecordStatus.Addition,
                    MRid = "MRid",
                    ValidityStartDate = SystemClock.Instance.GetCurrentInstant(),
                    ChargeType = new ChargeType
                    {
                     Description = "Description",
                     Name = "Name",
                     VatPayer = "VatPayer",
                    },
                },
            };
        }
    }
}
