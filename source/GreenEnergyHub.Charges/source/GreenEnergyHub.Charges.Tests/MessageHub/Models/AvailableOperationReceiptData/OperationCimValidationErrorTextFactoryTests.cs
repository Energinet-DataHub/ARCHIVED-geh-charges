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
// limitations under the License.using System;

using System;
using System.Linq;
using FluentAssertions;
using GreenEnergyHub.Charges.Domain.Charges;
using GreenEnergyHub.Charges.Domain.Dtos.ChargePriceCommands;
using GreenEnergyHub.Charges.Domain.Dtos.Validation;
using GreenEnergyHub.Charges.Infrastructure.Core.Cim.ValidationErrors;
using GreenEnergyHub.Charges.MessageHub.Models.AvailableOperationReceiptData;
using GreenEnergyHub.Charges.TestCore.Attributes;
using GreenEnergyHub.Charges.Tests.Builders.Command;
using GreenEnergyHub.Charges.Tests.TestCore;
using Microsoft.Extensions.Logging;
using Xunit;

namespace GreenEnergyHub.Charges.Tests.MessageHub.Models.AvailableOperationReceiptData
{
    public class OperationCimValidationErrorTextFactoryTests
    {
        [Theory]
        [InlineAutoMoqData]
        public void Create_WithMultipleMergeFields_ReturnsExpectedDescription(
            ChargePriceCommandBuilder chargePriceCommandBuilder,
            CimValidationErrorTextProvider cimValidationErrorTextProvider,
            ChargePriceOperationDto chargeOperationDto,
            ILoggerFactory loggerFactory)
        {
            // Arrange
            var chargeCommand = chargePriceCommandBuilder.WithChargeOperation(chargeOperationDto).Build();
            var sut = new ChargePriceCimValidationErrorTextFactory(cimValidationErrorTextProvider, loggerFactory);
            var validationError = new ValidationError(ValidationRuleIdentifier.DocumentTypeMustBeRequestChangeOfPriceList, null, null);

            var expected = CimValidationErrorTextTemplateMessages.DocumentTypeMustBeRequestChangeOfPriceListErrorText
                .Replace("{{DocumentType}}", chargeCommand.Document.Type.ToString())
                .Replace("{{DocumentBusinessReasonCode}}", chargeCommand.Document.BusinessReasonCode.ToString());

            // Act
            var actual = sut.Create(validationError, chargeCommand, chargeOperationDto);

            // Assert
            actual.Should().Be(expected);
        }

        [Theory]
        [InlineAutoMoqData]
        public void Create_WithPointPosition_ReturnsExpectedDescription(
            ChargePriceCommandBuilder chargePriceCommandBuilder,
            ChargePriceOperationDto chargePriceOperationDto,
            CimValidationErrorTextProvider cimValidationErrorTextProvider,
            ILoggerFactory loggerFactory)
        {
            // Arrange
            var chargeCommand = chargePriceCommandBuilder.WithChargeOperation(chargePriceOperationDto).Build();
            var sut = new ChargePriceCimValidationErrorTextFactory(cimValidationErrorTextProvider, loggerFactory);
            var triggeredBy = chargePriceOperationDto.Points.GetPositionOfPoint(chargePriceOperationDto.Points[1]).ToString();
            var validationError = new ValidationError(ValidationRuleIdentifier.MaximumPrice, chargePriceOperationDto.Id, triggeredBy);

            var expectedPoint = chargePriceOperationDto.Points[1];
            var expected = CimValidationErrorTextTemplateMessages.MaximumPriceErrorText
                .Replace("{{ChargePointPrice}}", expectedPoint.Price.ToString("N"))
                .Replace("{{ChargePointPosition}}", chargePriceOperationDto.Points.GetPositionOfPoint(expectedPoint).ToString())
                .Replace("{{DocumentSenderProvidedChargeId}}", chargePriceOperationDto.ChargeId)
                .Replace("{{ChargeType}}", chargePriceOperationDto.Type.ToString())
                .Replace("{{ChargeOwner}}", chargePriceOperationDto.ChargeOwner);

            // Act
            var actual = sut.Create(validationError, chargeCommand, chargePriceOperationDto);

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
            ChargePriceCommandBuilder chargePriceCommandBuilder,
            ChargePriceOperationDto chargePriceOperationDto,
            CimValidationErrorTextProvider cimValidationErrorTextProvider,
            ILoggerFactory loggerFactory)
        {
            // Arrange
            var chargeCommand = chargePriceCommandBuilder.WithChargeOperation(chargePriceOperationDto).Build();
            var validationError = new ValidationError(validationRuleIdentifier, chargePriceOperationDto.Id, triggeredBy);
            var sut = new ChargePriceCimValidationErrorTextFactory(cimValidationErrorTextProvider, loggerFactory);

            // Act
            var actual = sut.Create(validationError, chargeCommand, chargePriceOperationDto);

            // Assert
            actual.ToLower().Should().Contain($"price {CimValidationErrorTextTemplateMessages.Unknown}");
            if (validationRuleIdentifier == ValidationRuleIdentifier.MaximumPrice)
                actual.ToLower().Should().Contain($"position {CimValidationErrorTextTemplateMessages.Unknown}");
        }

        [Theory]
        [InlineAutoMoqData]
        public void Create_MergesAllMergeFields(
            ChargePriceCommand chargePriceCommand,
            CimValidationErrorTextProvider cimValidationErrorTextProvider,
            ILoggerFactory loggerFactory)
        {
            // Arrange
            var chargePriceOperationDto = chargePriceCommand.Operations.First();
            var validationRuleIdentifiers = (ValidationRuleIdentifier[])Enum.GetValues(typeof(ValidationRuleIdentifier));
            var identifiersForRulesWithExtendedData =
                ValidationRuleForInterfaceLoader.GetValidationRuleIdentifierForTypes(
                    DomainAssemblyHelper.GetDomainAssembly(), typeof(IValidationRuleWithExtendedData));

            var sut = new ChargePriceCimValidationErrorTextFactory(cimValidationErrorTextProvider, loggerFactory);

            // Act
            // Assert
            foreach (var validationRuleIdentifier in validationRuleIdentifiers)
            {
                var triggeredBy = GetTriggeredBy(chargePriceCommand, validationRuleIdentifier);
                var actual = sut.Create(
                    new ValidationError(validationRuleIdentifier, null, triggeredBy),
                    chargePriceCommand,
                    chargePriceOperationDto);

                actual.Should().NotBeNullOrWhiteSpace();
                actual.Should().NotContain("{");
                actual.Should().NotContain("  ");

                if (identifiersForRulesWithExtendedData.Contains(validationRuleIdentifier) && triggeredBy != null)
                    actual.Should().NotContain("unknown");
            }
        }

        private static string? GetTriggeredBy(
            ChargePriceCommand chargePriceCommand,
            ValidationRuleIdentifier validationRuleIdentifier)
        {
            switch (validationRuleIdentifier)
            {
                case ValidationRuleIdentifier.SubsequentBundleOperationsFail:
                    return chargePriceCommand.Operations.First().Id;
                default:
                    return null;
            }
        }
    }
}
