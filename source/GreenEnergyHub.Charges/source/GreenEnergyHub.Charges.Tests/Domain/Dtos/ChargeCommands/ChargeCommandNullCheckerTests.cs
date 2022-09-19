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
using GreenEnergyHub.Charges.Domain.Dtos.ChargeInformationCommands;
using GreenEnergyHub.Charges.Domain.Dtos.SharedDtos;
using GreenEnergyHub.Charges.TestCore.Builders.Command;
using GreenEnergyHub.TestHelpers;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Domain.Dtos.ChargeCommands
{
    [UnitTest]
    public class ChargeCommandNullCheckerTests
    {
        [Theory]
        [InlineAutoDomainData(BusinessReasonCode.UpdateChargeInformation, null)]
        [InlineAutoDomainData(BusinessReasonCode.UpdateChargeInformation, "")]
        [InlineAutoDomainData(BusinessReasonCode.UpdateChargePrices, null)]
        [InlineAutoDomainData(BusinessReasonCode.UpdateChargePrices, "")]
        public void ChargeCommandPropertiesAreNotNullOrWhitespace(
            BusinessReasonCode businessReasonCode,
            string documentId)
        {
            // Arrange
            var chargeOperationDto = new ChargeInformationOperationDtoBuilder()
                .Build();
            var documentDto = new DocumentDtoBuilder()
                .WithDocumentId(documentId)
                .WithBusinessReasonCode(businessReasonCode)
                .Build();
            var chargeCommand = new ChargeInformationCommandBuilder()
                .WithDocumentDto(documentDto)
                .WithChargeOperation(chargeOperationDto)
                .Build();
            var chargeCommands = new List<ChargeInformationCommand> { chargeCommand };
            var chargeBundle = new ChargeInformationCommandBundle(documentDto, chargeCommands);

            // Act & Assert
            Assert.Throws<ArgumentException>(() =>
                ChargeCommandNullChecker.ThrowExceptionIfRequiredPropertyIsNull(chargeBundle));
        }

        [Theory]
        [InlineAutoDomainData]
        public void ThrowExceptionIfRequiredPropertyIsNull_WhenValid_DoesNotThrow(
            DocumentDtoBuilder documentDtoBuilder,
            ChargeInformationCommandBuilder chargeInformationCommandBuilder)
        {
            // Arrange
            var documentDto = documentDtoBuilder.Build();
            var chargeCommand = chargeInformationCommandBuilder.Build();
            var chargeCommands = new List<ChargeInformationCommand> { chargeCommand };
            var chargeBundle = new ChargeInformationCommandBundle(documentDto, chargeCommands);

            // Act
            var ex = Record.Exception(() =>
                ChargeCommandNullChecker.ThrowExceptionIfRequiredPropertyIsNull(chargeBundle));

            // Assert
            Assert.Null(ex);
        }

        [Theory]
        [InlineAutoDomainData]
        public void ThrowExceptionIfRequiredPropertyIsNull_WhenCommandIsNull_ThrowsArgumentNullException(
            DocumentDtoBuilder documentDtoBuilder)
        {
            // Arrange
            ChargeInformationCommand? chargeCommand = null;
            var documentDto = documentDtoBuilder.Build();
            var chargeCommands = new List<ChargeInformationCommand> { chargeCommand! };
            var chargeBundle = new ChargeInformationCommandBundle(documentDto, chargeCommands);

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                ChargeCommandNullChecker.ThrowExceptionIfRequiredPropertyIsNull(chargeBundle));
        }

        [Theory]
        [InlineAutoDomainData]
        public void ThrowExceptionIfRequiredPropertyIsNull_WhenParticipantIsNull_ThrowsArgumentNullException(
            ChargeInformationCommandBuilder builder)
        {
            // Arrange
            MarketParticipantDto? marketParticipant = null;
            var documentDto = new DocumentDtoBuilder()
                .WithSender(marketParticipant!)
                .Build();
            var chargeCommand = builder.WithDocumentDto(documentDto).Build();
            var chargeCommands = new List<ChargeInformationCommand> { chargeCommand };
            var chargeBundle = new ChargeInformationCommandBundle(documentDto, chargeCommands);

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                ChargeCommandNullChecker.ThrowExceptionIfRequiredPropertyIsNull(chargeBundle));
        }

        [Theory]
        [InlineAutoDomainData]
        public void ChargeCommandDocumentIsNullThrowsException(ChargeInformationCommandBuilder builder)
        {
            // Arrange
            var chargeCommand = builder.WithDocumentDto(null!).Build();
            var chargeCommands = new List<ChargeInformationCommand> { chargeCommand };
            var chargeBundle = new ChargeInformationCommandBundle(null!, chargeCommands);

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                ChargeCommandNullChecker.ThrowExceptionIfRequiredPropertyIsNull(chargeBundle));
        }

        [Theory]
        [InlineAutoDomainData]
        public void ChargeCommandChargeOperationIsNullThrowsException(
            DocumentDtoBuilder documentDtoBuilder)
        {
            // Arrange
            var documentDto = documentDtoBuilder.Build();
            var chargeCommand = new ChargeInformationCommandBuilder().WithChargeOperation(null!).Build();
            var chargeCommands = new List<ChargeInformationCommand> { chargeCommand };
            var chargeBundle = new ChargeInformationCommandBundle(documentDto, chargeCommands);

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                ChargeCommandNullChecker.ThrowExceptionIfRequiredPropertyIsNull(chargeBundle));
        }
    }
}
