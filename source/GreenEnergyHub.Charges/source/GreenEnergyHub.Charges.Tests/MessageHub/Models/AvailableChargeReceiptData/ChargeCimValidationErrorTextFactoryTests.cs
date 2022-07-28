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
using FluentAssertions;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeInformationCommands;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeInformationCommands.Validation.InputValidation.ValidationRules;
using GreenEnergyHub.Charges.Domain.Dtos.Validation;
using GreenEnergyHub.Charges.Infrastructure.Core.Cim.ValidationErrors;
using GreenEnergyHub.Charges.MessageHub.AvailableData.Models.AvailableChargeReceiptData;
using GreenEnergyHub.Charges.TestCore.Attributes;
using GreenEnergyHub.Charges.Tests.Builders.Command;
using GreenEnergyHub.Charges.Tests.TestCore;
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
            ChargeInformationCommandBuilder chargeInformationCommandBuilder,
            ChargeOperationDto chargeOperationDto,
            CimValidationErrorTextProvider cimValidationErrorTextProvider,
            ILoggerFactory loggerFactory)
        {
            // Arrange
            var chargeCommand = chargeInformationCommandBuilder.WithChargeOperation(chargeOperationDto).Build();
            var sut = new ChargeCimValidationErrorTextFactory(cimValidationErrorTextProvider, loggerFactory);
            var rule = new ResolutionTariffValidationRule(chargeOperationDto);
            var validationError = new ValidationError(rule.ValidationRuleIdentifier, null!, null!);

            var expected = CimValidationErrorTextTemplateMessages.ResolutionTariffValidationErrorText
                .Replace("{{ChargeResolution}}", chargeOperationDto.Resolution.ToString())
                .Replace("{{DocumentSenderProvidedChargeId}}", chargeOperationDto.ChargeId)
                .Replace("{{ChargeType}}", chargeOperationDto.Type.ToString())
                .Replace("{{ChargeOwner}}", chargeOperationDto.ChargeOwner);

            // Act
            var actual = sut.Create(validationError, chargeCommand, chargeOperationDto);

            // Assert
            actual.Should().Be(expected);
        }

        [Theory]
        [InlineAutoMoqData]
        public void Create_WithPointPosition_ReturnsExpectedDescription(
            ChargeInformationCommandBuilder chargeInformationCommandBuilder,
            ChargeOperationDto chargeOperationDto,
            CimValidationErrorTextProvider cimValidationErrorTextProvider,
            ILoggerFactory loggerFactory)
        {
            // Arrange
            var chargeCommand = chargeInformationCommandBuilder.WithChargeOperation(chargeOperationDto).Build();
            var sut = new ChargeCimValidationErrorTextFactory(cimValidationErrorTextProvider, loggerFactory);
            var rule = new MaximumPriceRule(chargeOperationDto);
            var triggeredBy = chargeOperationDto.Points[1].Position.ToString();
            var validationError = new ValidationError(rule.ValidationRuleIdentifier, chargeOperationDto.Id, triggeredBy);

            var expectedPoint = chargeOperationDto.Points[1];
            var expected = CimValidationErrorTextTemplateMessages.MaximumPriceErrorText
                .Replace("{{ChargePointPrice}}", expectedPoint.Price.ToString("N"))
                .Replace("{{ChargePointPosition}}", expectedPoint.Position.ToString())
                .Replace("{{DocumentSenderProvidedChargeId}}", chargeOperationDto.ChargeId)
                .Replace("{{ChargeType}}", chargeOperationDto.Type.ToString())
                .Replace("{{ChargeOwner}}", chargeOperationDto.ChargeOwner);

            // Act
            var actual = sut.Create(validationError, chargeCommand, chargeOperationDto);

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
            ChargeInformationCommandBuilder chargeInformationCommandBuilder,
            ChargeOperationDto chargeOperationDto,
            CimValidationErrorTextProvider cimValidationErrorTextProvider,
            ILoggerFactory loggerFactory)
        {
            // Arrange
            var chargeCommand = chargeInformationCommandBuilder.WithChargeOperation(chargeOperationDto).Build();
            var validationError = new ValidationError(validationRuleIdentifier, chargeOperationDto.Id, triggeredBy);
            var sut = new ChargeCimValidationErrorTextFactory(cimValidationErrorTextProvider, loggerFactory);

            // Act
            var actual = sut.Create(validationError, chargeCommand, chargeOperationDto);

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
            ChargeInformationCommandBuilder chargeInformationCommandBuilder,
            ChargeOperationDto chargeOperationDto,
            CimValidationErrorTextProvider cimValidationErrorTextProvider,
            ILoggerFactory loggerFactory)
        {
            // Arrange
            var chargeCommand = chargeInformationCommandBuilder.WithChargeOperation(chargeOperationDto).Build();
            var triggeredBy = seedTriggeredBy == "0" ?
                chargeOperationDto.Points[1].Position.ToString() :
                seedTriggeredBy;
            var validationError = new ValidationError(validationRuleIdentifier, chargeOperationDto.Id, triggeredBy);

            var sut = new ChargeCimValidationErrorTextFactory(cimValidationErrorTextProvider, loggerFactory);
            var expected = CimValidationErrorTextTemplateMessages.StartDateValidationErrorText
                .Replace("{{ChargeStartDateTime}}", chargeOperationDto.StartDateTime.ToString());

            // Act
            var actual = sut.Create(validationError, chargeCommand, chargeOperationDto);

            // Assert
            actual.Should().Contain(expected);
        }

        [Theory]
        [InlineAutoMoqData]
        public void Create_MergesAllMergeFields(
            ChargeInformationCommand chargeInformationCommand,
            CimValidationErrorTextProvider cimValidationErrorTextProvider,
            ILoggerFactory loggerFactory)
        {
            // Arrange
            var validationRuleIdentifiers = (ValidationRuleIdentifier[])Enum.GetValues(typeof(ValidationRuleIdentifier));
            var identifiersForRulesWithExtendedData =
                ValidationRuleForInterfaceLoader.GetValidationRuleIdentifierForTypes(
                    typeof(MaximumPriceRule).Assembly, typeof(IValidationRuleWithExtendedData));

            var sut = new ChargeCimValidationErrorTextFactory(cimValidationErrorTextProvider, loggerFactory);

            // Act
            // Assert
            foreach (var operation in chargeInformationCommand.Operations)
            {
                var chargeOperationDtos = new List<ChargeOperationDto> { operation };
                var commandWithOperation = new ChargeInformationCommand(chargeInformationCommand.Document, chargeOperationDtos);

                foreach (var identifier in validationRuleIdentifiers)
                {
                    var triggeredBy = GetTriggeredBy(operation, identifier);
                    var validationError = new ValidationError(identifier, operation.Id, triggeredBy);

                    var actual = sut.Create(validationError, commandWithOperation, operation);

                    actual.Should().NotBeNullOrWhiteSpace();
                    actual.Should().NotContain("{");
                    actual.Should().NotContain("  ");

                    if (identifiersForRulesWithExtendedData.Contains(identifier))
                        actual.Should().NotContain("unknown");
                }
            }
        }

        private static string? GetTriggeredBy(
            ChargeOperationDto chargeOperationDto, ValidationRuleIdentifier validationRuleIdentifier)
        {
            return validationRuleIdentifier switch
            {
                ValidationRuleIdentifier.ChargePriceMaximumDigitsAndDecimals =>
                    chargeOperationDto.Points[0].Position.ToString(),
                ValidationRuleIdentifier.MaximumPrice =>
                    chargeOperationDto.Points[1].Position.ToString(),
                ValidationRuleIdentifier.SubsequentBundleOperationsFail =>
                    chargeOperationDto.Id,
                _ => null,
            };
        }
    }
}
