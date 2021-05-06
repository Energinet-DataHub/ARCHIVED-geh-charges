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
        [InlineAutoDomainData(null, "Valid", "Valid", "Valid", "Valid", "Valid", "Valid", "Valid", "Valid")]
        [InlineAutoDomainData("valid", null, "Valid", "Valid", "Valid", "Valid", "Valid", "Valid", "Valid", "Valid")]
        [InlineAutoDomainData("valid", "Valid", null, "Valid", "Valid", "Valid", "Valid", "Valid", "Valid", "Valid")]
        [InlineAutoDomainData("valid", "Valid", "Valid", null, "Valid", "Valid", "Valid", "Valid", "Valid", "Valid")]
        [InlineAutoDomainData("valid", "Valid", "Valid", "Valid", null, "Valid", "Valid", "Valid", "Valid", "Valid")]
        [InlineAutoDomainData("valid", "Valid", "Valid", "Valid", "Valid", null, "Valid", "Valid", "Valid", "Valid")]
        [InlineAutoDomainData("valid", "Valid", "Valid", "Valid", "Valid", "Valid", null, "Valid", "Valid", "Valid")]
        [InlineAutoDomainData("valid", "Valid", "Valid", "Valid", "Valid", "Valid", "Valid", null, "Valid", "Valid")]
        [InlineAutoDomainData("valid", "Valid", "Valid", "Valid", "Valid", "Valid", "Valid", "Valid", null, "Valid")]
        [InlineAutoDomainData("valid", "Valid", "Valid", "Valid", "Valid", "Valid", "Valid", "Valid", "Valid", null)]
        [InlineAutoDomainData("", "Valid", "Valid", "Valid", "Valid", "Valid", "Valid", "Valid", "Valid", "Valid")]
        [InlineAutoDomainData("valid", "", "Valid", "Valid", "Valid", "Valid", "Valid", "Valid", "Valid", "Valid")]
        [InlineAutoDomainData("valid", "Valid", "", "Valid", "Valid", "Valid", "Valid", "Valid", "Valid", "Valid")]
        [InlineAutoDomainData("valid", "Valid", "Valid", "", "Valid", "Valid", "Valid", "Valid", "Valid", "Valid")]
        [InlineAutoDomainData("valid", "Valid", "Valid", "Valid", "", "Valid", "Valid", "Valid", "Valid", "Valid")]
        [InlineAutoDomainData("valid", "Valid", "Valid", "Valid", "Valid", "", "Valid", "Valid", "Valid", "Valid")]
        [InlineAutoDomainData("valid", "Valid", "Valid", "Valid", "Valid", "Valid", "", "Valid", "Valid", "Valid")]
        [InlineAutoDomainData("valid", "Valid", "Valid", "Valid", "Valid", "Valid", "Valid", "", "Valid", "Valid")]
        [InlineAutoDomainData("valid", "Valid", "Valid", "Valid", "Valid", "Valid", "Valid", "Valid", "", "Valid")]
        [InlineAutoDomainData("valid", "Valid", "Valid", "Valid", "Valid", "Valid", "Valid", "Valid", "Valid", "")]
        public void ChargeCommandPropertiesAreNotNullOrWhitespace(
            string chargeId,
            string correlationId,
            string lastUpdatedBy,
            string owner,
            string documentId,
            string senderId,
            string recipientId,
            string eventId,
            string chargeTypeLongDescription,
            string chargeTypeDescription)
        {
            // Arrange
            var c = GetValidCharge();
            c.ChargeDto.Id = chargeId;
            c.ChargeDto.Owner = owner;
            c.SetCorrelationId(correlationId);
            c.ChargeOperation.LastUpdatedBy = lastUpdatedBy;
            c.Document.Id = documentId;
            c.Document.Sender.MRid = senderId;
            c.Document.Recipient.MRid = recipientId;
            c.ChargeOperation.Id = eventId;
            c.ChargeDto.Description = chargeTypeLongDescription;
            c.ChargeDto.Name = chargeTypeDescription;

            // Act & Assert
            Assert.Throws<ArgumentException>(() => ChargeCommandNullChecker.ThrowExceptionIfRequiredPropertyIsNull(c));
        }

        [Fact]
        public void ChargeCommandChargeIsNullThrowsException()
        {
            // Arrange
            var c = GetValidCharge();
            c.ChargeDto = null!;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => ChargeCommandNullChecker.ThrowExceptionIfRequiredPropertyIsNull(c));
        }

        [Fact]
        public void ChargeCommandDocumentIsNullThrowsException()
        {
            // Arrange
            var c = GetValidCharge();
            c.Document = null!;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => ChargeCommandNullChecker.ThrowExceptionIfRequiredPropertyIsNull(c));
        }

        [Fact]
        public void ChargeCommandChargeEventIsNullThrowsException()
        {
            // Arrange
            var c = GetValidCharge();
            c.ChargeOperation = null!;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => ChargeCommandNullChecker.ThrowExceptionIfRequiredPropertyIsNull(c));
        }

        private static ChargeCommand GetValidCharge()
        {
            return new ("some-correlation-id")
            {
                ChargeDto = new ChargeDto
                {
                    Name = "description",
                    Id = "id",
                    StartDateTime = SystemClock.Instance.GetCurrentInstant(),
                    Owner = "owner",
                    Points = new List<Point>
                    {
                        new Point { Position = 0, Time = SystemClock.Instance.GetCurrentInstant(), Price = 200m },
                    },
                    Resolution = Resolution.P1D,
                    Type = ChargeType.Fee,
                    Vat = Vat.NoVat,
                    Description = "LongDescription",
                },
                Document = new Document
                {
                    Id = "id",
                    CorrelationId = "CorrelationId",
                    RequestDate = SystemClock.Instance.GetCurrentInstant(),
                    Recipient = new MarketParticipant
                    {
                        Id = 0,
                        Name = "Name",
                        Role = MarketParticipantRole.EnergySupplier,
                        MRid = "MRid",
                    },
                    Sender = new MarketParticipant
                    {
                        Id = 1,
                        Name = "Name",
                        Role = MarketParticipantRole.EnergySupplier,
                        MRid = "MRid",
                    },
                    Type = "type",
                    IndustryClassification = IndustryClassification.Electricity,
                    CreatedDateTime = SystemClock.Instance.GetCurrentInstant(),
                },
                ChargeOperation = new ChargeOperation
                {
                  Id = "id",
                  Status = ChargeEventFunction.Change,
                  BusinessReasonCode = BusinessReasonCode.UpdateChargeInformation,
                  LastUpdatedBy = "LastUpdatedBy",
                },
            };
        }
    }
}
