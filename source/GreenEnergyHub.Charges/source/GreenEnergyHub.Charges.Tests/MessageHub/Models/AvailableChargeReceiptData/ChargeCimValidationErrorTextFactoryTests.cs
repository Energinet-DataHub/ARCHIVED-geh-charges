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
using GreenEnergyHub.Charges.Domain.Dtos.ChargeInformationCommands;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeInformationCommands.Validation.InputValidation.ValidationRules;
using GreenEnergyHub.Charges.Domain.Dtos.SharedDtos;
using GreenEnergyHub.Charges.Domain.Dtos.Validation;
using GreenEnergyHub.Charges.Infrastructure.Core.Cim.ValidationErrors;
using GreenEnergyHub.Charges.MessageHub.Models.AvailableChargeReceiptData;
using GreenEnergyHub.Charges.TestCore.Attributes;
using Microsoft.Extensions.Logging;
using Moq;
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
            ChargeInformationOperationDto chargeInformationOperationDto,
            CimValidationErrorTextProvider cimValidationErrorTextProvider,
            ILoggerFactory loggerFactory)
        {
            // Arrange
            var sut = new ChargeCimValidationErrorTextFactory(cimValidationErrorTextProvider, loggerFactory);
            var rule = new ResolutionTariffValidationRule(chargeInformationOperationDto);
            var validationError = new ValidationError(rule.ValidationRuleIdentifier, null!, null!);

            var expected = CimValidationErrorTextTemplateMessages.ResolutionTariffValidationErrorText
                .Replace("{{ChargeResolution}}", chargeInformationOperationDto.Resolution.ToString())
                .Replace("{{DocumentSenderProvidedChargeId}}", chargeInformationOperationDto.SenderProvidedChargeId)
                .Replace("{{ChargeType}}", chargeInformationOperationDto.ChargeType.ToString())
                .Replace("{{ChargeOwner}}", chargeInformationOperationDto.ChargeOwner);

            // Act
            var actual = sut.Create(validationError, It.IsAny<DocumentDto>(), chargeInformationOperationDto);

            // Assert
            actual.Should().Be(expected);
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
            var sut = new ChargeCimValidationErrorTextFactory(cimValidationErrorTextProvider, loggerFactory);

            // Act
            // Assert
            foreach (var operation in chargeInformationCommand.Operations)
            {
                foreach (var identifier in validationRuleIdentifiers)
                {
                    var triggeredBy = GetTriggeredBy(operation, identifier);
                    var validationError = new ValidationError(identifier, operation.OperationId, triggeredBy);

                    var actual = sut.Create(validationError, chargeInformationCommand.Document, operation);

                    actual.Should().NotBeNullOrWhiteSpace();
                    actual.Should().NotContain("{");
                    actual.Should().NotContain("  ");
                    if (identifier == ValidationRuleIdentifier.SubsequentBundleOperationsFail)
                        actual.Should().NotContain("unknown");
                }
            }
        }

        private static string? GetTriggeredBy(
            ChargeInformationOperationDto chargeInformationOperationDto, ValidationRuleIdentifier validationRuleIdentifier)
        {
            return validationRuleIdentifier switch
            {
                ValidationRuleIdentifier.SubsequentBundleOperationsFail =>
                    chargeInformationOperationDto.OperationId,
                _ => null,
            };
        }
    }
}
