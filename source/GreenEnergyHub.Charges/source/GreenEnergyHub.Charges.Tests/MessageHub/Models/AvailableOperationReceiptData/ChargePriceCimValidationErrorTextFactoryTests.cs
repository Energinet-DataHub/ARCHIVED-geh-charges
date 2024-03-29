﻿// Copyright 2020 Energinet DataHub A/S
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

using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using GreenEnergyHub.Charges.Domain.Charges;
using GreenEnergyHub.Charges.Domain.Dtos.ChargePriceCommands;
using GreenEnergyHub.Charges.Domain.Dtos.SharedDtos;
using GreenEnergyHub.Charges.Domain.Dtos.Validation;
using GreenEnergyHub.Charges.Infrastructure.Core.Cim.ValidationErrors;
using GreenEnergyHub.Charges.MessageHub.Models.AvailableOperationReceiptData;
using GreenEnergyHub.Charges.TestCore.Attributes;
using GreenEnergyHub.Charges.TestCore.Builders.Command;
using GreenEnergyHub.Charges.Tests.TestCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace GreenEnergyHub.Charges.Tests.MessageHub.Models.AvailableOperationReceiptData
{
    public class ChargePriceCimValidationErrorTextFactoryTests
    {
        [Theory]
        [InlineAutoMoqData]
        public void Create_WithMultipleMergeFields_ReturnsExpectedDescription(
            DocumentDtoBuilder documentDtoBuilder,
            CimValidationErrorTextProvider cimValidationErrorTextProvider,
            ChargePriceOperationDto chargeOperationDto,
            ILoggerFactory loggerFactory)
        {
            // Arrange
            var document = documentDtoBuilder
                .WithDocumentType(DocumentType.RejectRequestChangeOfPriceList)
                .WithBusinessReasonCode(BusinessReasonCode.UpdateChargePrices)
                .Build();
            var sut = new ChargePriceCimValidationErrorTextFactory(cimValidationErrorTextProvider, loggerFactory);
            var validationError = new ValidationError(ValidationRuleIdentifier.DocumentTypeMustBeRequestChangeOfPriceList, null, null);

            var expected = CimValidationErrorTextTemplateMessages.DocumentTypeMustBeRequestChangeOfPriceListErrorText
                .Replace("{{DocumentType}}", document.Type.ToString())
                .Replace("{{DocumentBusinessReasonCode}}", document.BusinessReasonCode.ToString());

            // Act
            var actual = sut.Create(validationError, document, chargeOperationDto);

            // Assert
            actual.Should().Be(expected);
        }

        [Theory]
        [InlineAutoMoqData]
        public void Create_WithPointPosition_ReturnsExpectedDescription(
            DocumentDto documentDto,
            ChargePriceOperationDto chargePriceOperationDto,
            CimValidationErrorTextProvider cimValidationErrorTextProvider,
            ILoggerFactory loggerFactory)
        {
            // Arrange
            var sut = new ChargePriceCimValidationErrorTextFactory(cimValidationErrorTextProvider, loggerFactory);
            const string triggeredBy = "3";
            var validationError = new ValidationError(ValidationRuleIdentifier.MaximumPrice, chargePriceOperationDto.OperationId, triggeredBy);

            var expectedPoint = chargePriceOperationDto.Points.OrderBy(x => x.Time).ToList().Last();
            var expected = CimValidationErrorTextTemplateMessages.MaximumPriceErrorText
                .Replace("{{ChargePointPrice}}", expectedPoint.Price.ToString("N"))
                .Replace("{{ChargePointPosition}}", triggeredBy)
                .Replace("{{DocumentSenderProvidedChargeId}}", chargePriceOperationDto.SenderProvidedChargeId)
                .Replace("{{ChargeType}}", chargePriceOperationDto.ChargeType.ToString())
                .Replace("{{ChargeOwner}}", chargePriceOperationDto.ChargeOwner);

            // Act
            var actual = sut.Create(validationError, documentDto, chargePriceOperationDto);

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
            DocumentDto document,
            ChargePriceOperationDto chargePriceOperationDto,
            CimValidationErrorTextProvider cimValidationErrorTextProvider,
            ILoggerFactory loggerFactory)
        {
            // Arrange
            var validationError = new ValidationError(validationRuleIdentifier, chargePriceOperationDto.OperationId, triggeredBy);
            var sut = new ChargePriceCimValidationErrorTextFactory(cimValidationErrorTextProvider, loggerFactory);

            // Act
            var actual = sut.Create(validationError, document, chargePriceOperationDto);

            // Assert
            actual.ToLower().Should().Contain($"price {CimValidationErrorTextTemplateMessages.Unknown}");
            if (validationRuleIdentifier == ValidationRuleIdentifier.MaximumPrice)
                actual.ToLower().Should().Contain($"position {CimValidationErrorTextTemplateMessages.Unknown}");
        }

        [Theory]
        [InlineAutoMoqData]
        public void Create_MergesAllMergeFields(
            DocumentDto documentDto,
            CimValidationErrorTextProvider cimValidationErrorTextProvider,
            ChargePriceOperationDtoBuilder chargePriceOperationDtoBuilder,
            ILoggerFactory loggerFactory)
        {
            // Arrange
            var chargePriceOperationDto = chargePriceOperationDtoBuilder.Build();
            var validationRuleIdentifiers = GetExpectedRulesForChargePriceOperation();
            var identifiersForRulesWithExtendedData =
                ValidationRuleForInterfaceLoader.GetValidationRuleIdentifierForTypes(
                    AssemblyHelper.GetDomainAssembly(), typeof(IValidationRuleWithExtendedData));
            var sut = new ChargePriceCimValidationErrorTextFactory(cimValidationErrorTextProvider, loggerFactory);

            // Act
            // Assert
            foreach (var identifier in validationRuleIdentifiers)
            {
                var validationError = new ValidationError(identifier, chargePriceOperationDto.OperationId, "1");
                var actual = sut.Create(validationError, documentDto, chargePriceOperationDto);

                actual.Should().NotBeNullOrWhiteSpace();
                actual.Should().NotContain("{");
                actual.Should().NotContain("  ");
                if (identifiersForRulesWithExtendedData.Contains(identifier))
                    actual.Should().NotContain("unknown");
            }
        }

        [Theory]
        [InlineAutoMoqData(ValidationRuleIdentifier.StartDateValidation, null!)]
        [InlineAutoMoqData(ValidationRuleIdentifier.StartDateValidation, -1)]
        [InlineAutoMoqData(ValidationRuleIdentifier.StartDateValidation, 0)]
        public void Create_PointPositionAreIgnored_WhenNotApplicable(
            ValidationRuleIdentifier validationRuleIdentifier,
            string? seedTriggeredBy,
            ChargePriceOperationDto chargePriceOperationDto,
            CimValidationErrorTextProvider cimValidationErrorTextProvider,
            ILoggerFactory loggerFactory)
        {
            // Arrange
            var triggeredBy = seedTriggeredBy == "0" ?
                chargePriceOperationDto.Points.GetPositionOfPoint(chargePriceOperationDto.Points[1]).ToString() :
                seedTriggeredBy;
            var validationError = new ValidationError(validationRuleIdentifier, chargePriceOperationDto.OperationId, triggeredBy);

            var sut = new ChargePriceCimValidationErrorTextFactory(cimValidationErrorTextProvider, loggerFactory);
            var expected = CimValidationErrorTextTemplateMessages.StartDateValidationErrorText
                .Replace("{{ChargeStartDateTime}}", chargePriceOperationDto.StartDateTime.ToString());

            // Act
            var actual = sut.Create(validationError, It.IsAny<DocumentDto>(), chargePriceOperationDto);

            // Assert
            actual.Should().Contain(expected);
        }

        private static List<ValidationRuleIdentifier> GetExpectedRulesForChargePriceOperation()
        {
            var expectedRules = new List<ValidationRuleIdentifier>
            {
                ValidationRuleIdentifier.ChargeIdLengthValidation,
                ValidationRuleIdentifier.ChargeIdRequiredValidation,
                ValidationRuleIdentifier.ChargeOperationIdRequired,
                ValidationRuleIdentifier.ChargeOperationIdLengthValidation,
                ValidationRuleIdentifier.ChargeOwnerIsRequiredValidation,
                ValidationRuleIdentifier.ChargeOwnerMustMatchSender,
                ValidationRuleIdentifier.ChargeTypeIsKnownValidation,
                ValidationRuleIdentifier.ChargeTypeTariffPriceCount,
                ValidationRuleIdentifier.ChargePriceMaximumDigitsAndDecimals,
                ValidationRuleIdentifier.MaximumPrice,
                ValidationRuleIdentifier.NumberOfPointsMatchTimeIntervalAndResolution,
                ValidationRuleIdentifier.PriceListMustStartAndStopAtMidnightValidationRule,
                ValidationRuleIdentifier.StartDateTimeRequiredValidation,
                ValidationRuleIdentifier.StartDateValidation,
                ValidationRuleIdentifier.ResolutionSubscriptionValidation,
                ValidationRuleIdentifier.ResolutionTariffValidation,
                ValidationRuleIdentifier.ResolutionFeeValidation,
                ValidationRuleIdentifier.ResolutionIsRequired,
            };
            return expectedRules;
        }
    }
}
