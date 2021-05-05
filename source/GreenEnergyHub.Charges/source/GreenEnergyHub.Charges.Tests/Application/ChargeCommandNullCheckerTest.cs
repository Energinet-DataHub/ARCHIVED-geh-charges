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
            c.ChargeNew.Id = chargeId;
            c.ChargeNew.Owner = owner;
            c.ChargeEvent.CorrelationId = correlationId;
            c.ChargeEvent.LastUpdatedBy = lastUpdatedBy;
            c.Document.Id = documentId;
            c.Document.Sender.MRid = senderId;
            c.Document.Recipient.MRid = recipientId;
            c.ChargeEvent.Id = eventId;
            c.ChargeNew.Description = chargeTypeLongDescription;
            c.ChargeNew.Name = chargeTypeDescription;

            // Act & Assert
            Assert.Throws<ArgumentException>(() => ChargeCommandNullChecker.ThrowExceptionIfRequiredPropertyIsNull(c));
        }

        [Fact]
        public void ChargeCommandChargeIsNullThrowsException()
        {
            // Arrange
            var c = GetValidCharge();
            c.ChargeNew = null!;

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
            c.ChargeEvent = null!;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => ChargeCommandNullChecker.ThrowExceptionIfRequiredPropertyIsNull(c));
        }

        private static ChargeCommand GetValidCharge()
        {
            return new ()
            {
                ChargeNew = new ChargeNew
                {
                    Name = "description",
                    Id = "id",
                    Owner = "owner",
                    Points = new List<Point>
                    {
                        new Point { Position = 0, Time = SystemClock.Instance.GetCurrentInstant(), Price = 200m },
                    },
                    Resolution = Resolution.P1D,
                    Type = ChargeType.Fee,
                    Vat = Vat.D01,
                    Description = "LongDescription",
                },
                Document = new Document
                {
                    Id = "id",
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
                    BusinessReasonCode = BusinessReasonCode.D18,
                    CreatedDateTime = SystemClock.Instance.GetCurrentInstant(),
                },
                ChargeEvent = new ChargeEvent
                {
                  Id = "id",
                  Status = ChargeEventFunction.Change,
                  CorrelationId = "CorrelationId",
                  LastUpdatedBy = "LastUpdatedBy",
                  StartDateTime = SystemClock.Instance.GetCurrentInstant(),
                  RequestDate = SystemClock.Instance.GetCurrentInstant(),
                },
            };
        }
    }
}
