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
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommands;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommands.Validation.InputValidation.ValidationRules;
using GreenEnergyHub.Charges.Domain.Dtos.Validation;
using GreenEnergyHub.Charges.Infrastructure.Core.Cim.ValidationErrors;
using GreenEnergyHub.Charges.MessageHub.Models.AvailableChargeReceiptData;
using GreenEnergyHub.Charges.TestCore.Attributes;
using GreenEnergyHub.Charges.Tests.Builders.Command;
using GreenEnergyHub.Charges.Tests.TestCore;
using Microsoft.Extensions.Logging;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.MessageHub.Models.AvailableChargeReceiptData
{
    [UnitTest]
    public class ChargeInformationCimValidationErrorTextFactoryTests
    {
        [Theory]
        [InlineAutoMoqData]
        public void Create_WithMultipleMergeFields_ReturnsExpectedDescription(
            ChargeCommandBuilder chargeCommandBuilder,
            ChargeInformationDto chargeInformationDto,
            CimValidationErrorTextProvider cimValidationErrorTextProvider,
            ILoggerFactory loggerFactory)
        {
            // Arrange
            var chargeCommand = chargeCommandBuilder.WithChargeOperation(chargeInformationDto).Build();
            var sut = new ChargeInformationCimValidationErrorTextFactory(cimValidationErrorTextProvider, loggerFactory);
            var rule = new ResolutionTariffValidationRule(chargeInformationDto);
            var validationError = new ValidationError(rule.ValidationRuleIdentifier, null!, null!);

            var expected = CimValidationErrorTextTemplateMessages.ResolutionTariffValidationErrorText
                .Replace("{{ChargeResolution}}", chargeInformationDto.Resolution.ToString())
                .Replace("{{DocumentSenderProvidedChargeId}}", chargeInformationDto.ChargeId)
                .Replace("{{ChargeType}}", chargeInformationDto.Type.ToString());

            // Act
            var actual = sut.Create(validationError, chargeCommand, chargeInformationDto);

            // Assert
            actual.Should().Be(expected);
        }

        [Theory]
        [InlineAutoMoqData]
        public void Create_WithPointPosition_ReturnsExpectedDescription(
            ChargeCommandBuilder chargeCommandBuilder,
            ChargeInformationDto chargeInformationDto,
            CimValidationErrorTextProvider cimValidationErrorTextProvider,
            ILoggerFactory loggerFactory)
        {
            // Arrange
            var chargeCommand = chargeCommandBuilder.WithChargeOperation(chargeInformationDto).Build();
            var sut = new ChargeInformationCimValidationErrorTextFactory(cimValidationErrorTextProvider, loggerFactory);
#pragma warning disable CS0618
            var rule = new MaximumPriceRule(chargeInformationDto);
#pragma warning restore CS0618
            var triggeredBy = chargeInformationDto.Points[1].Position.ToString();
            var validationError = new ValidationError(rule.ValidationRuleIdentifier, chargeInformationDto.Id, triggeredBy);

            var expectedPoint = chargeInformationDto.Points[1];
            var expected = CimValidationErrorTextTemplateMessages.MaximumPriceErrorText
                .Replace("{{ChargePointPrice}}", expectedPoint.Price.ToString("N"))
                .Replace("{{ChargePointPosition}}", expectedPoint.Position.ToString());

            // Act
            var actual = sut.Create(validationError, chargeCommand, chargeInformationDto);

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
            ChargeCommandBuilder chargeCommandBuilder,
            ChargeInformationDto chargeInformationDto,
            CimValidationErrorTextProvider cimValidationErrorTextProvider,
            ILoggerFactory loggerFactory)
        {
            // Arrange
            var chargeCommand = chargeCommandBuilder.WithChargeOperation(chargeInformationDto).Build();
            var validationError = new ValidationError(validationRuleIdentifier, chargeInformationDto.Id, triggeredBy);
            var sut = new ChargeInformationCimValidationErrorTextFactory(cimValidationErrorTextProvider, loggerFactory);

            // Act
            var actual = sut.Create(validationError, chargeCommand, chargeInformationDto);

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
            ChargeCommandBuilder chargeCommandBuilder,
            ChargeInformationDto chargeInformationDto,
            CimValidationErrorTextProvider cimValidationErrorTextProvider,
            ILoggerFactory loggerFactory)
        {
            // Arrange
            var chargeCommand = chargeCommandBuilder.WithChargeOperation(chargeInformationDto).Build();
            var triggeredBy = seedTriggeredBy == "0" ?
                chargeInformationDto.Points[1].Position.ToString() :
                seedTriggeredBy;
            var validationError = new ValidationError(validationRuleIdentifier, chargeInformationDto.Id, triggeredBy);

            var sut = new ChargeInformationCimValidationErrorTextFactory(cimValidationErrorTextProvider, loggerFactory);
            var expected = CimValidationErrorTextTemplateMessages.StartDateValidationErrorText
                .Replace("{{ChargeStartDateTime}}", chargeInformationDto.StartDateTime.ToString());

            // Act
            var actual = sut.Create(validationError, chargeCommand, chargeInformationDto);

            // Assert
            actual.Should().Contain(expected);
        }

        [Theory]
        [InlineAutoMoqData]
        public void Create_MergesAllMergeFields(
            ChargeCommandBuilder chargeCommandBuilder,
            ChargeInformationDtoBuilder chargeInformationDtoBuilder,
            CimValidationErrorTextProvider cimValidationErrorTextProvider,
            ILoggerFactory loggerFactory)
        {
            // Arrange
            var chargeInformationDto = chargeInformationDtoBuilder.WithPoint(0, 1.11m).WithPoint(1, 2.22m).Build();
            var chargeCommand = chargeCommandBuilder.WithChargeOperation(chargeInformationDto).Build();
            var validationRuleIdentifiers = (ValidationRuleIdentifier[])Enum.GetValues(typeof(ValidationRuleIdentifier));
            var identifiersForRulesWithExtendedData =
                ValidationRuleForInterfaceLoader.GetValidationRuleIdentifierForTypes(
                    typeof(MaximumPriceRule).Assembly, typeof(IValidationRuleWithExtendedData));

            var sut = new ChargeInformationCimValidationErrorTextFactory(cimValidationErrorTextProvider, loggerFactory);

            // Act
            // Assert
            foreach (var operation in chargeCommand.ChargeOperations)
            {
                var chargeOperations = new List<IChargeOperation> { operation };
                var commandWithOperation = new ChargeCommand(chargeCommand.Document, chargeOperations);

                foreach (var identifier in validationRuleIdentifiers)
                {
                    var triggeredBy = GetTriggeredBy(chargeInformationDto, identifier);
                    var validationError = new ValidationError(identifier, operation.Id, triggeredBy);

                    var actual = sut.Create(validationError, commandWithOperation, (ChargeInformationDto)operation);

                    actual.Should().NotBeNullOrWhiteSpace();
                    actual.Should().NotContain("{");
                    actual.Should().NotContain("  ");

                    if (identifiersForRulesWithExtendedData.Contains(identifier))
                        actual.Should().NotContain("unknown");
                }
            }
        }

        private static string? GetTriggeredBy(
            ChargeInformationDto chargeInformationDto, ValidationRuleIdentifier validationRuleIdentifier)
        {
            return validationRuleIdentifier switch
            {
                ValidationRuleIdentifier.ChargePriceMaximumDigitsAndDecimals =>
                    chargeInformationDto.Points[0].Position.ToString(),
                ValidationRuleIdentifier.MaximumPrice =>
                    chargeInformationDto.Points[1].Position.ToString(),
                ValidationRuleIdentifier.SubsequentBundleOperationsFail =>
                    chargeInformationDto.Id,
                _ => null,
            };
        }
    }
}
