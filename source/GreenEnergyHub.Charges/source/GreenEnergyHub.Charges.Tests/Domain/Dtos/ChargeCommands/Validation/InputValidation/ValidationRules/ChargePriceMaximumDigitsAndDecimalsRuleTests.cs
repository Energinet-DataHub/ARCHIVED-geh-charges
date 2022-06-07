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
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommands.Validation.InputValidation.ValidationRules;
using GreenEnergyHub.Charges.Domain.Dtos.Validation;
using GreenEnergyHub.Charges.Infrastructure.Core.Cim.ValidationErrors;
using GreenEnergyHub.Charges.MessageHub.Models.AvailableChargeReceiptData;
using GreenEnergyHub.Charges.TestCore.Attributes;
using GreenEnergyHub.Charges.Tests.Builders.Command;
using GreenEnergyHub.TestHelpers;
using Microsoft.Extensions.Logging;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Domain.Dtos.ChargeCommands.Validation.InputValidation.ValidationRules
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
            ChargeOperationDtoBuilder builder)
        {
            // Arrange
            var chargeOperationDto = builder.WithPoint(1, price).Build();

            // Act
            var sut = new ChargePriceMaximumDigitsAndDecimalsRule(chargeOperationDto);

            // Assert
            sut.IsValid.Should().Be(expected);
        }

        [Theory]
        [InlineAutoDomainData]
        public void ValidationRuleIdentifier_ShouldBe_EqualTo(ChargeOperationDtoBuilder builder)
        {
            var invalidChargeOperationDto = builder.WithPoint(1, 123456789m).Build();
            var sut = new ChargePriceMaximumDigitsAndDecimalsRule(invalidChargeOperationDto);
            sut.ValidationRuleIdentifier.Should().Be(ValidationRuleIdentifier.ChargePriceMaximumDigitsAndDecimals);
        }

        [Theory]
        [InlineAutoDomainData]
        public void TriggeredBy_ShouldCauseCompleteErrorMessages_ToMarketParticipant(
            ILoggerFactory loggerFactory,
            CimValidationErrorTextProvider cimValidationErrorTextProvider)
        {
            // Arrange
            var chargeOperationDto = new ChargeOperationDtoBuilder()
                .WithPoint(1, 123456789m)
                .Build();
            var invalidCommand = new ChargeCommandBuilder()
                .WithChargeOperation(chargeOperationDto)
                .Build();
            var expectedPoint = chargeOperationDto.Points[0];
            var triggeredBy = expectedPoint.Position.ToString();

            var sutRule = new ChargePriceMaximumDigitsAndDecimalsRule(chargeOperationDto);
            var validationError =
                new ValidationError(sutRule.ValidationRuleIdentifier, chargeOperationDto.Id, triggeredBy);
            var sutFactory = new ChargeCimValidationErrorTextFactory(cimValidationErrorTextProvider, loggerFactory);

            // Act
            var actual = sutFactory.Create(validationError, invalidCommand, chargeOperationDto);

            // Assert
            sutRule.IsValid.Should().BeFalse();
            sutRule.TriggeredBy.Should().Be(triggeredBy);

            var expected = CimValidationErrorTextTemplateMessages.ChargePriceMaximumDigitsAndDecimalsErrorText
                            .Replace("{{ChargePointPrice}}", expectedPoint.Price.ToString("N"))
                            .Replace("{{DocumentSenderProvidedChargeId}}", chargeOperationDto.ChargeId);
            actual.Should().Be(expected);
        }
    }
}
