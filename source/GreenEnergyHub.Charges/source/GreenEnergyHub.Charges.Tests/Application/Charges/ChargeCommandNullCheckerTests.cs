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
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommands;
using GreenEnergyHub.Charges.Domain.Dtos.SharedDtos;
using GreenEnergyHub.Charges.Tests.Builders;
using GreenEnergyHub.TestHelpers;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Application.Charges
{
    [UnitTest]
    public class ChargeCommandNullCheckerTests
    {
        [Theory]
        [InlineAutoDomainData(null, "Valid", "Valid")]
        [InlineAutoDomainData("Valid", null, "Valid")]
        [InlineAutoDomainData("Valid", "Valid", null)]
        [InlineAutoDomainData("", "Valid", "Valid")]
        [InlineAutoDomainData("Valid", "", "Valid")]
        [InlineAutoDomainData("Valid", "Valid", "")]
        public void ChargeCommandPropertiesAreNotNullOrWhitespace(
            string description,
            string chargeName,
            string documentId)
        {
            // Arrange
            var c = new ChargeCommandTestBuilder()
                .WithDescription(description)
                .WithChargeName(chargeName)
                .WithDocumentId(documentId)
                .Build();

            // Act & Assert
            Assert.Throws<ArgumentException>(() => ChargeCommandNullChecker.ThrowExceptionIfRequiredPropertyIsNull(c));
        }

        [Theory]
        [InlineAutoDomainData]
        public void ThrowExceptionIfRequiredPropertyIsNull_WhenValid_DoesNotThrow(ChargeCommandTestBuilder chargeCommandTestBuilder)
        {
            // Arrange
            var command = chargeCommandTestBuilder.Build();

            // Act
            var ex = Record.Exception(() => ChargeCommandNullChecker.ThrowExceptionIfRequiredPropertyIsNull(command));

            // Assert
            Assert.Null(ex);
        }

        [Fact]
        public void ThrowExceptionIfRequiredPropertyIsNull_WhenCommandIsNull_ThrowsArgumentNullException()
        {
            // Arrange
            ChargeCommand? command = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => ChargeCommandNullChecker.ThrowExceptionIfRequiredPropertyIsNull(command!));
        }

        [Theory]
        [InlineAutoDomainData]
        public void ThrowExceptionIfRequiredPropertyIsNull_WhenParticipantIsNull_ThrowsArgumentNullException(ChargeCommandTestBuilder builder)
        {
            // Arrange
            MarketParticipantDto? marketParticipant = null;
            var command = builder.WithSender(marketParticipant!).Build();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => ChargeCommandNullChecker.ThrowExceptionIfRequiredPropertyIsNull(command));
        }

        [Theory]
        [InlineAutoDomainData]
        public void ChargeCommandDocumentIsNullThrowsException(ChargeCommandTestBuilder builder)
        {
            // Arrange
            var c = builder.Build();
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
    }
}
