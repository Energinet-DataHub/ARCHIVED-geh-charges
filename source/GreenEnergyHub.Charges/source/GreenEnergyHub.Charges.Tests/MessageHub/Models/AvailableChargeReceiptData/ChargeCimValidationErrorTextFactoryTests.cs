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
using System.Linq;
using FluentAssertions;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommands;
using GreenEnergyHub.Charges.Domain.Dtos.Validation;
using GreenEnergyHub.Charges.Infrastructure.Core.Cim.ValidationErrors;
using GreenEnergyHub.Charges.MessageHub.Models.AvailableChargeReceiptData;
using GreenEnergyHub.Charges.TestCore.Attributes;
using Microsoft.Extensions.Logging;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.MessageHub.Models.AvailableChargeReceiptData
{
    [UnitTest]
    public class ChargeCimValidationErrorTextFactoryTests
    {
        [Theory]
        [InlineAutoMoqData]
        public void Create_WithMultipleMergeFields_ReturnsExpectedDescription(
            ChargeCommand chargeCommand,
            CimValidationErrorTextProvider cimValidationErrorTextProvider,
            ILoggerFactory loggerFactory)
        {
            // Arrange
            var sut = new ChargeCimValidationErrorTextFactory(cimValidationErrorTextProvider, loggerFactory);

            var chargeOperationDto = chargeCommand.Charges.First();
            var expected = CimValidationErrorTextTemplateMessages.ResolutionTariffValidationErrorText
                .Replace("{{ChargeResolution}}", chargeOperationDto.Resolution.ToString())
                .Replace("{{DocumentSenderProvidedChargeId}}", chargeOperationDto.ChargeId)
                .Replace("{{ChargeType}}", chargeOperationDto.Type.ToString());

            // Act
            var actual = sut.Create(
                new ValidationError(ValidationRuleIdentifier.ResolutionTariffValidation, chargeOperationDto.Id, null),
                chargeCommand,
                chargeOperationDto);

            // Assert
            actual.Should().Be(expected);
        }

        [Theory]
        [InlineAutoMoqData]
        public void Create_WithPointPosition_ReturnsExpectedDescription(
            ChargeCommand chargeCommand,
            CimValidationErrorTextProvider cimValidationErrorTextProvider,
            ILoggerFactory loggerFactory)
        {
            // Arrange
            var sut = new ChargeCimValidationErrorTextFactory(cimValidationErrorTextProvider, loggerFactory);

            var chargeOperationDto = chargeCommand.Charges.First();
            var expectedPoint = chargeOperationDto.Points[1];
            var expected = CimValidationErrorTextTemplateMessages.MaximumPriceErrorText
                .Replace("{{ChargePointPrice}}", expectedPoint.Price.ToString("N"))
                .Replace("{{ChargePointPosition}}", expectedPoint.Position.ToString());

            // Act
            var actual = sut.Create(
                new ValidationError(
                    ValidationRuleIdentifier.MaximumPrice,
                    chargeOperationDto.Id,
                    chargeOperationDto.Points[1].Position.ToString()),
                chargeCommand,
                chargeOperationDto);

            // Assert
            actual.Should().Be(expected);
        }

        [Theory]
        [InlineAutoMoqData(ValidationRuleIdentifier.MaximumPrice, null!)]
        [InlineAutoMoqData(ValidationRuleIdentifier.MaximumPrice, -1)]
        [InlineAutoMoqData(ValidationRuleIdentifier.ChargePriceMaximumDigitsAndDecimals, null!)]
        [InlineAutoMoqData(ValidationRuleIdentifier.ChargePriceMaximumDigitsAndDecimals, -1)]
        public void Create_WithInvalidPointPosition_ReturnsErrorMessage(
            ValidationRuleIdentifier validationRuleIdentifier,
            string? triggeredBy,
            ChargeCommand chargeCommand,
            CimValidationErrorTextProvider cimValidationErrorTextProvider,
            ILoggerFactory loggerFactory)
        {
            // Arrange
            var chargeOperationDto = chargeCommand.Charges.First();
            var sut = new ChargeCimValidationErrorTextFactory(cimValidationErrorTextProvider, loggerFactory);

            // Act
            var actual = sut.Create(
                new ValidationError(validationRuleIdentifier, chargeOperationDto.Id, triggeredBy),
                chargeCommand,
                chargeOperationDto);

            // Assert
            actual.ToLower().Should().Contain($"price {CimValidationErrorTextTemplateMessages.Unknown}");
            if (validationRuleIdentifier == ValidationRuleIdentifier.MaximumPrice)
                actual.ToLower().Should().Contain($"position {CimValidationErrorTextTemplateMessages.Unknown}");
        }

        [Theory]
        [InlineAutoMoqData(ValidationRuleIdentifier.StartDateValidation, null!)]
        [InlineAutoMoqData(ValidationRuleIdentifier.StartDateValidation, -1)]
        [InlineAutoMoqData(ValidationRuleIdentifier.StartDateValidation, 0)]
        public void Create_PointPositionAreIgnored_WhenNotApplicable(
            ValidationRuleIdentifier validationRuleIdentifier,
            string? seedTriggeredBy,
            ChargeCommand chargeCommand,
            CimValidationErrorTextProvider cimValidationErrorTextProvider,
            ILoggerFactory loggerFactory)
        {
            // Arrange
            var chargeOperationDto = chargeCommand.Charges.First();
            var triggeredBy = seedTriggeredBy == "0" ?
                chargeOperationDto.Points[1].Position.ToString() :
                seedTriggeredBy;

            var sut = new ChargeCimValidationErrorTextFactory(cimValidationErrorTextProvider, loggerFactory);
            var expected = CimValidationErrorTextTemplateMessages.StartDateValidationErrorText
                .Replace("{{ChargeStartDateTime}}", chargeOperationDto.StartDateTime.ToString());

            // Act
            var actual = sut.Create(
                new ValidationError(validationRuleIdentifier, chargeOperationDto.Id, triggeredBy),
                chargeCommand,
                chargeOperationDto);

            // Assert
            actual.Should().Contain(expected);
        }

        [Theory]
        [InlineAutoMoqData]
        public void Create_MergesAllMergeFields(
            ChargeCommand chargeCommand,
            CimValidationErrorTextProvider cimValidationErrorTextProvider,
            ILoggerFactory loggerFactory)
        {
            // Arrange
            var chargeOperationDto = chargeCommand.Charges.First();
            var validationRuleIdentifiers = (ValidationRuleIdentifier[])Enum.GetValues(typeof(ValidationRuleIdentifier));
            var sut = new ChargeCimValidationErrorTextFactory(cimValidationErrorTextProvider, loggerFactory);

            // Act
            // Assert
            foreach (var validationRuleIdentifier in validationRuleIdentifiers)
            {
                var triggeredBy = SetTriggeredByWithValidationError(chargeOperationDto, validationRuleIdentifier);
                var actual = sut.Create(
                    new ValidationError(validationRuleIdentifier, chargeOperationDto.Id, triggeredBy),
                    chargeCommand,
                    chargeOperationDto);

                actual.Should().NotBeNullOrWhiteSpace();
                actual.Should().NotContain("{");
                actual.Should().NotContain("  ");
            }
        }

        private static string? SetTriggeredByWithValidationError(
            ChargeOperationDto chargeOperationDto, ValidationRuleIdentifier validationRuleIdentifier)
        {
            return validationRuleIdentifier switch
            {
                ValidationRuleIdentifier.ChargePriceMaximumDigitsAndDecimals =>
                    chargeOperationDto.Points[0].Position.ToString(),
                ValidationRuleIdentifier.MaximumPrice =>
                    chargeOperationDto.Points[1].Position.ToString(),
                _ => null,
            };
        }
    }
}
