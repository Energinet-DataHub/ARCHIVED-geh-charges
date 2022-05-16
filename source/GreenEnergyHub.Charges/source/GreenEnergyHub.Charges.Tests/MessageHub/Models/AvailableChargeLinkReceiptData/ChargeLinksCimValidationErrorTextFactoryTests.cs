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
using GreenEnergyHub.Charges.Domain.Dtos.ChargeLinksCommands;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeLinksCommands.Validation.BusinessValidation.ValidationRules;
using GreenEnergyHub.Charges.Domain.Dtos.Validation;
using GreenEnergyHub.Charges.Infrastructure.Core.Cim.ValidationErrors;
using GreenEnergyHub.Charges.MessageHub.Models.AvailableChargeLinksReceiptData;
using GreenEnergyHub.Charges.TestCore.Attributes;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.MessageHub.Models.AvailableChargeLinkReceiptData
{
    [UnitTest]
    public class ChargeLinksCimValidationErrorTextFactoryTests
    {
        /*[Theory]
        [InlineAutoMoqData]
        public void Create_WhenTwoMergeFields_ReturnsExpectedDescription(
            ChargeLinksCommand chargeLinksCommand,
            ChargeLinkDto chargeLinkDto,
            CimValidationErrorTextProvider cimValidationErrorTextProvider)
        {
            // Arrange
            var meteringPoint = new MeteringPointBuilder().WithId(chargeLinkDto.MeteringPointId);
            var rule = new MeteringPointMustExistRule(meteringPoint);
            var validationRuleWithOperation =
                new ValidationRuleContainer(rule, chargeLinkDto.SenderProvidedChargeId);
            var sut = new ChargeLinksCimValidationErrorTextFactory(cimValidationErrorTextProvider);
            var expected = CimValidationErrorTextTemplateMessages.MeteringPointDoesNotExistValidationErrorText
                .Replace("{{MeteringPointId}}", chargeLinkDto.MeteringPointId)
                .Replace("{{ChargeLinkStartDate}}", chargeLinkDto.StartDateTime.ToString());

            // Act
            var actual = sut.Create(validationRuleWithOperation, chargeLinksCommand, chargeLinkDto);

            // Assert
            actual.Should().Be(expected);
        }*/

        /*[Theory]
        [InlineAutoMoqData]
        public void Create_MergesAllMergeFields(
            ChargeLinksCommand chargeLinksCommand,
            CimValidationErrorTextProvider cimValidationErrorTextProvider)
        {
            // Arrange
            var chargeLinkDto = chargeLinksCommand.ChargeLinksOperations.First();
            var validationRuleIdentifiers =
                (ValidationRuleIdentifier[])Enum.GetValues(typeof(ValidationRuleIdentifier));
            var sut = new ChargeLinksCimValidationErrorTextFactory(cimValidationErrorTextProvider);

            // Act
            // Assert
            foreach (var validationRuleIdentifier in validationRuleIdentifiers)
            {
                var triggeredBy = SetTriggeredByWithValidationError(chargeLinksCommand, validationRuleIdentifier);
                var actual = sut.Create(
                    new ValidationRuleContainer(validationRuleIdentifier, triggeredBy),
                    chargeLinksCommand,
                    chargeLinkDto);

                actual.Should().NotBeNullOrWhiteSpace();
                actual.Should().NotContain("{");
                actual.Should().NotContain("  ");
            }
        }*/

        private static string? SetTriggeredByWithValidationError(
            ChargeLinksCommand chargeLinksCommand, ValidationRuleIdentifier validationRuleIdentifier)
        {
            switch (validationRuleIdentifier)
            {
                case ValidationRuleIdentifier.ChargeDoesNotExist:
                case ValidationRuleIdentifier.MeteringPointDoesNotExist:
                case ValidationRuleIdentifier.ChargeLinkUpdateNotYetSupported:
                    return chargeLinksCommand.ChargeLinksOperations.First().SenderProvidedChargeId;
                default:
                    return null;
            }
        }
    }
}
