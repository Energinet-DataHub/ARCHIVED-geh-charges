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
using GreenEnergyHub.Charges.Domain.Charges.Commands;
using GreenEnergyHub.Charges.Domain.MarketDocument;
using GreenEnergyHub.Charges.Infrastructure.Context.Model;
using GreenEnergyHub.Charges.Tests.Builders;
using GreenEnergyHub.TestHelpers;
using Xunit;
using Xunit.Categories;
using Xunit.Sdk;
using MarketParticipant = GreenEnergyHub.Charges.Domain.MarketDocument.MarketParticipant;

namespace GreenEnergyHub.Charges.Tests.Application
{
    [UnitTest]
    public class ChargeCommandNullCheckerTests
    {
        [Theory]
        [InlineAutoDomainData(null, "Valid", "Valid", "Valid")]
        [InlineAutoDomainData("valid", null, "Valid", "Valid")]
        [InlineAutoDomainData("valid", "Valid", null, "Valid")]
        [InlineAutoDomainData("valid", "Valid", "Valid", null)]
        [InlineAutoDomainData("", "Valid", "Valid", "Valid")]
        [InlineAutoDomainData("valid", "", "Valid", "Valid")]
        [InlineAutoDomainData("valid", "Valid", "", "Valid")]
        [InlineAutoDomainData("valid", "Valid", "Valid", "")]
        public void ChargeCommandPropertiesAreNotNullOrWhitespace(
            string correlationId,
            string description,
            string chargeName,
            string documentId)
        {
            // Arrange
            var c = Build();
            c.SetCorrelationId(correlationId);
            c.ChargeOperation.ChargeDescription = description;
            c.ChargeOperation.ChargeName = chargeName;
            c.Document.Id = documentId;

            // Act & Assert
            Assert.Throws<ArgumentException>(() => ChargeCommandNullChecker.ThrowExceptionIfRequiredPropertyIsNull(c));
        }

        [Fact]
        public void ThrowExceptionIfRequiredPropertyIsNull_WhenValid_DoesNotThrow()
        {
            // Arrange
            var command = Build();

            // Act / Assert
            ChargeCommandNullChecker.ThrowExceptionIfRequiredPropertyIsNull(command);
        }

        [Fact]
        public void ThrowExceptionIfRequiredPropertyIsNull_WhenCommandIsNull_ThrowsArgumentNullException()
        {
            // Arrange
            ChargeCommand? command = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => ChargeCommandNullChecker.ThrowExceptionIfRequiredPropertyIsNull(command!));
        }

        [Fact]
        public void ThrowExceptionIfRequiredPropertyIsNull_WhenParticipantIsNull_ThrowsArgumentNullException()
        {
            // Arrange
            MarketParticipant? marketParticipant = null;
            var command = Build();
            command.Document.Sender = marketParticipant!;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => ChargeCommandNullChecker.ThrowExceptionIfRequiredPropertyIsNull(command!));
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
