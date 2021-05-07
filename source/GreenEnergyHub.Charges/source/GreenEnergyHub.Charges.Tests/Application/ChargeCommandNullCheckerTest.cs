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
using GreenEnergyHub.Charges.Application;
using GreenEnergyHub.Charges.Domain.ChangeOfCharges.Transaction;
using GreenEnergyHub.Charges.Tests.Builders;
using GreenEnergyHub.TestHelpers;
using Xunit;

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
        [InlineAutoDomainData("", "Valid", "Valid", "Valid", "Valid", "Valid", "Valid", "Valid", "Valid", "Valid")]
        [InlineAutoDomainData("valid", "", "Valid", "Valid", "Valid", "Valid", "Valid", "Valid", "Valid", "Valid")]
        [InlineAutoDomainData("valid", "Valid", "", "Valid", "Valid", "Valid", "Valid", "Valid", "Valid", "Valid")]
        [InlineAutoDomainData("valid", "Valid", "Valid", "", "Valid", "Valid", "Valid", "Valid", "Valid", "Valid")]
        [InlineAutoDomainData("valid", "Valid", "Valid", "Valid", "", "Valid", "Valid", "Valid", "Valid", "Valid")]
        [InlineAutoDomainData("valid", "Valid", "Valid", "Valid", "Valid", "", "Valid", "Valid", "Valid", "Valid")]
        [InlineAutoDomainData("valid", "Valid", "Valid", "Valid", "Valid", "Valid", "", "Valid", "Valid", "Valid")]
        [InlineAutoDomainData("valid", "Valid", "Valid", "Valid", "Valid", "Valid", "Valid", "", "Valid", "Valid")]
        [InlineAutoDomainData("valid", "Valid", "Valid", "Valid", "Valid", "Valid", "Valid", "Valid", "", "Valid")]
        public void ChargeCommandPropertiesAreNotNullOrWhitespace(
            string chargeId,
            string correlationId,
            string owner,
            string documentId,
            string senderId,
            string recipientId,
            string eventId,
            string chargeTypeLongDescription,
            string chargeTypeDescription)
        {
            // Arrange
            var c = Build();
            c.ChargeOperation.ChargeId = chargeId;
            c.ChargeOperation.ChargeOwner = owner;
            c.SetCorrelationId(correlationId);
            c.Document.Id = documentId;
            c.Document.Sender.MRid = senderId;
            c.Document.Recipient.MRid = recipientId;
            c.ChargeOperation.Id = eventId;
            c.ChargeOperation.ChargeDescription = chargeTypeLongDescription;
            c.ChargeOperation.ChargeName = chargeTypeDescription;

            // Act & Assert
            Assert.Throws<ArgumentException>(() => ChargeCommandNullChecker.ThrowExceptionIfRequiredPropertyIsNull(c));
        }

        [Fact]
        public void ChargeCommandDocumentIsNullThrowsException()
        {
            // Arrange
            var c = Build();
            c.Document = null!;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => ChargeCommandNullChecker.ThrowExceptionIfRequiredPropertyIsNull(c));
        }

        [Fact]
        public void ChargeCommandChargeOperationIsNullThrowsException()
        {
            // Arrange
            var testBuilder = new ChargeCommandTestBuilder();
            var c = testBuilder.Build();
            c.ChargeOperation = null!;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => ChargeCommandNullChecker.ThrowExceptionIfRequiredPropertyIsNull(c));
        }

        private static ChargeCommand Build()
        {
            var builder = new ChargeCommandTestBuilder();
            return builder.Build();
        }
    }
}
