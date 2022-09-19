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

using FluentAssertions;
using GreenEnergyHub.Charges.Domain.Charges;
using GreenEnergyHub.Charges.Domain.Dtos.ChargePriceCommands.Validation.InputValidation.ValidationRules;
using GreenEnergyHub.Charges.Domain.Dtos.Validation;
using GreenEnergyHub.Charges.Infrastructure.Core.Cim.ValidationErrors;
using GreenEnergyHub.Charges.MessageHub.Models.AvailableOperationReceiptData;
using GreenEnergyHub.Charges.TestCore.Attributes;
using GreenEnergyHub.Charges.TestCore.Builders.Command;
using GreenEnergyHub.TestHelpers;
using Microsoft.Extensions.Logging;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Domain.Dtos.ChargePriceCommands.Validation.InputValidation.ValidationRules
{
    [UnitTest]
    public class ChargePriceMaximumDigitsAndDecimalsRuleTests
    {
        [Theory]
        [InlineAutoMoqData(0.000001, true)]
        [InlineAutoMoqData(99999999.000001, true)]
        [InlineAutoMoqData(99999999.0000001, false)]
        [InlineAutoMoqData(99999999, true)]
        [InlineAutoMoqData(100000000.000001, false)]
        public void IsValid_WhenLessThan8DigitsAnd6Decimals_IsValid(
            decimal price,
            bool expected,
            ChargePriceOperationDtoBuilder builder)
        {
            // Arrange
            var chargePriceOperationDto = builder.WithPoint(price).Build();

            // Act
            var sut = new ChargePriceMaximumDigitsAndDecimalsRule(chargePriceOperationDto);

            // Assert
            sut.IsValid.Should().Be(expected);
        }

        [Theory]
        [InlineAutoDomainData]
        public void ValidationRuleIdentifier_ShouldBe_EqualTo(ChargePriceOperationDtoBuilder builder)
        {
            var invalidChargePriceOperationDto = builder.WithPoint(123456789m).Build();
            var sut = new ChargePriceMaximumDigitsAndDecimalsRule(invalidChargePriceOperationDto);
            sut.ValidationRuleIdentifier.Should().Be(ValidationRuleIdentifier.ChargePriceMaximumDigitsAndDecimals);
        }

        [Theory]
        [InlineAutoDomainData]
        public void TriggeredBy_ShouldCauseCompleteErrorMessages_ToMarketParticipant(
            ILoggerFactory loggerFactory,
            CimValidationErrorTextProvider cimValidationErrorTextProvider)
        {
            // Arrange
            var chargePriceOperationDto = new ChargePriceOperationDtoBuilder()
                .WithPoint(123456789m)
                .Build();
            var invalidCommand = new ChargePriceCommandBuilder()
                .WithChargeOperation(chargePriceOperationDto)
                .Build();
            var expectedPoint = chargePriceOperationDto.Points[0];
            var triggeredBy = chargePriceOperationDto.Points.GetPositionOfPoint(expectedPoint).ToString();

            var sutRule = new ChargePriceMaximumDigitsAndDecimalsRule(chargePriceOperationDto);
            var validationError =
                new ValidationError(sutRule.ValidationRuleIdentifier, chargePriceOperationDto.OperationId, triggeredBy);
            var sutFactory = new ChargePriceCimValidationErrorTextFactory(cimValidationErrorTextProvider, loggerFactory);

            // Act
            var actual = sutFactory.Create(validationError, invalidCommand.Document, chargePriceOperationDto);

            // Assert
            sutRule.IsValid.Should().BeFalse();
            sutRule.TriggeredBy.Should().Be(triggeredBy);

            var expected = CimValidationErrorTextTemplateMessages.ChargePriceMaximumDigitsAndDecimalsErrorText
                            .Replace("{{ChargePointPrice}}", expectedPoint.Price.ToString("N"))
                            .Replace("{{DocumentSenderProvidedChargeId}}", chargePriceOperationDto.SenderProvidedChargeId)
                            .Replace("{{ChargeType}}", chargePriceOperationDto.ChargeType.ToString())
                            .Replace("{{ChargeOwner}}", chargePriceOperationDto.ChargeOwner);
            actual.Should().Be(expected);
        }
    }
}
