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
using FluentAssertions;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommands;
using GreenEnergyHub.Charges.Domain.Dtos.Validation;
using GreenEnergyHub.Charges.Infrastructure.Core.Cim.ValidationErrors;
using GreenEnergyHub.Charges.MessageHub.Models.AvailableChargeReceiptData;
using GreenEnergyHub.Charges.TestCore.Attributes;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.MessageHub.Models.AvailableChargeReceiptData
{
    [UnitTest]
    public class CimValidationErrorTextFactoryTests
    {
        [Theory]
        [InlineAutoMoqData]
        public void Create_WhenThreeMergeFields_ReturnsExpectedDescription(
            ChargeCommand chargeCommand,
            CimValidationErrorTextProvider cimValidationErrorTextProvider)
        {
            // Arrange
            var sut = new ChargeCimValidationErrorTextFactory(cimValidationErrorTextProvider);

            var expected = CimValidationErrorTextTemplateMessages.ResolutionTariffValidationErrorText
                .Replace("{{ChargeResolution}}", chargeCommand.ChargeOperation.Resolution.ToString())
                .Replace("{{DocumentSenderProvidedChargeId}}", chargeCommand.ChargeOperation.ChargeId)
                .Replace("{{ChargeType}}", chargeCommand.ChargeOperation.Type.ToString());

            // Act
            var actual = sut.Create(
                new ValidationError(ValidationRuleIdentifier.ResolutionTariffValidation, null),
                chargeCommand);

            // Assert
            actual.Should().Be(expected);
        }

        [Theory]
        [InlineAutoMoqData]
        public void Create_WithPointPosition_ReturnsExpectedDescription(
            ChargeCommand chargeCommand,
            CimValidationErrorTextProvider cimValidationErrorTextProvider)
        {
            // Arrange
            var sut = new CimValidationErrorTextFactory(cimValidationErrorTextProvider);

            var expectedPoint = chargeCommand.ChargeOperation.Points[1];
            var expected = CimValidationErrorTextTemplateMessages.MaximumPriceErrorText
                .Replace("{{ChargePointPrice}}", expectedPoint.Price.ToString("N"))
                .Replace("{{ChargePointPosition}}", expectedPoint.Position.ToString());

            // Act
            var actual = sut.Create(
                new ValidationError(
                    ValidationRuleIdentifier.MaximumPrice,
                    chargeCommand.ChargeOperation.Points[1].Position.ToString()),
                chargeCommand);

            // Assert
            actual.Should().Be(expected);
        }

        [Theory]
        [InlineAutoMoqData]
        public void Create_MergesAllMergeFields(
            ChargeCommand chargeCommand,
            CimValidationErrorTextProvider cimValidationErrorTextProvider)
        {
            // Arrange
            var validationRuleIdentifiers = (ValidationRuleIdentifier[])Enum.GetValues(typeof(ValidationRuleIdentifier));
            var sut = new ChargeCimValidationErrorTextFactory(cimValidationErrorTextProvider);

            // Act
            // Assert
            foreach (var validationRuleIdentifier in validationRuleIdentifiers)
            {
                var listElement = SetListElementWithValidationError(chargeCommand, validationRuleIdentifier);
                var actual = sut.Create(new ValidationError(validationRuleIdentifier, listElement), chargeCommand);
                actual.Should().NotBeNullOrWhiteSpace();
                actual.Should().NotContain("{");
            }
        }

        private static string? SetListElementWithValidationError(
            ChargeCommand chargeCommand, ValidationRuleIdentifier validationRuleIdentifier)
        {
            return validationRuleIdentifier switch
            {
                ValidationRuleIdentifier.ChargePriceMaximumDigitsAndDecimals =>
                    chargeCommand.ChargeOperation.Points[0].Position.ToString(),
                ValidationRuleIdentifier.MaximumPrice =>
                    chargeCommand.ChargeOperation.Points[1].Position.ToString(),
                _ => null,
            };
        }
    }
}
