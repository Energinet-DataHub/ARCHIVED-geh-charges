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

using GreenEnergyHub.Charges.Domain.Dtos.ChargeInformationCommands.Validation.InputValidation.ValidationRules;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeInformationCommands;
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
    public class MaximumPriceRuleTests
    {
        private const decimal LargestValidPrice = 999999;
        private const decimal SmallestInvalidPrice = 1000000;

        [Theory]
        [InlineAutoMoqData(999999, true)]
        [InlineAutoMoqData(999999.999999, true)]
        [InlineAutoMoqData(1000000, false)]
        [InlineAutoMoqData(1000000.000001, false)]
        public void MaximumPriceRule_WhenCalledPriceIsTooHigh_IsFalse(
            decimal price,
            bool expected,
            ChargeOperationDtoBuilder chargeOperationDtoBuilder)
        {
            var chargeOperationDto = CreateChargeOperationDto(chargeOperationDtoBuilder, price);
            var sut = new MaximumPriceRule(chargeOperationDto);
            sut.IsValid.Should().Be(expected);
        }

        [Theory]
        [InlineAutoDomainData]
        public void ValidationRuleIdentifier_ShouldBe_EqualTo(ChargeOperationDtoBuilder chargeOperationDtoBuilder)
        {
            var chargeOperationDto = CreateChargeOperationDto(chargeOperationDtoBuilder, SmallestInvalidPrice);
            var sut = new MaximumPriceRule(chargeOperationDto);
            sut.ValidationRuleIdentifier.Should().Be(ValidationRuleIdentifier.MaximumPrice);
        }

        [Theory]
        [InlineAutoDomainData(ValidationRuleIdentifier.MaximumPrice)]
        public void TriggeredBy_ShouldCauseCompleteErrorMessages_ToMarketParticipant(
            ValidationRuleIdentifier validationRuleIdentifier,
            ILoggerFactory loggerFactory,
            CimValidationErrorTextProvider cimValidationErrorTextProvider)
        {
            // Arrange
            var chargeOperationDto = new ChargeOperationDtoBuilder()
                .WithPoint(1, LargestValidPrice)
                .WithPoint(2, SmallestInvalidPrice)
                .Build();
            var invalidCommand = new ChargeCommandBuilder().WithChargeOperation(chargeOperationDto).Build();
            var expectedPoint = chargeOperationDto.Points[1];
            var triggeredBy = expectedPoint.Position.ToString();

            // Act & arrange
            var sutRule = new MaximumPriceRule(chargeOperationDto);
            var sutFactory = new ChargeCimValidationErrorTextFactory(cimValidationErrorTextProvider, loggerFactory);
            var actual = sutFactory.Create(
                new ValidationError(validationRuleIdentifier, chargeOperationDto.Id, triggeredBy),
                invalidCommand,
                chargeOperationDto);

            // Assert
            sutRule.IsValid.Should().BeFalse();
            sutRule.TriggeredBy.Should().Be(triggeredBy);

            var expected = CimValidationErrorTextTemplateMessages.MaximumPriceErrorText
                .Replace("{{ChargePointPrice}}", expectedPoint.Price.ToString("N"))
                .Replace("{{ChargePointPosition}}", triggeredBy);
            actual.Should().Be(expected);
        }

        private static ChargeOperationDto CreateChargeOperationDto(ChargeOperationDtoBuilder builder, decimal price)
        {
            return builder.WithPoint(1, price).Build();
        }
    }
}
