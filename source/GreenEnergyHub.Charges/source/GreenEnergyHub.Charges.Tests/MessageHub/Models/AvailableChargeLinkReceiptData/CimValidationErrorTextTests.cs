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
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommands.Validation;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeLinksCommands;
using GreenEnergyHub.Charges.Domain.Dtos.Validation;
using GreenEnergyHub.Charges.Infrastructure.Core.Cim.ValidationErrors;
using GreenEnergyHub.Charges.MessageHub.Models.AvailableChargeLinksReceiptData;
using GreenEnergyHub.Charges.TestCore.Attributes;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.MessageHub.Models.AvailableChargeLinkReceiptData
{
    [UnitTest]
    public class CimValidationErrorTextFactoryTests
    {
        [Theory]
        [InlineAutoMoqData]
        public void Create_WhenThreeMergeFields_ReturnsExpectedDescription(
            ChargeLinksCommand chargeLinksCommand,
            CimValidationErrorTextProvider cimValidationErrorTextProvider)
        {
            // Arrange
            var sut = new ChargeLinksCimValidationErrorTextFactory(cimValidationErrorTextProvider);
            var expected = CimValidationErrorTextTemplateMessages.MeteringPointDoesNotExistValidationErrorText
                .Replace("{{MeteringPointId}}", chargeLinksCommand.MeteringPointId)
                .Replace("{{MeteringPointEffectiveDate}}", "TODO");

            // Act
            var actual = sut.Create(
                new ValidationError(
                    ValidationRuleIdentifier.MeteringPointDoesNotExist,
                    chargeLinksCommand.ChargeLinks.First().SenderProvidedChargeId),
                chargeLinksCommand);

            // Assert
            actual.Should().Be(expected);
        }

        [Theory]
        [InlineAutoMoqData]
        public void Create_MergesAllMergeFields(
            ChargeLinksCommand chargeLinksCommand,
            CimValidationErrorTextProvider cimValidationErrorTextProvider)
        {
            // Arrange
            var validationRuleIdentifiers = (ValidationRuleIdentifier[])Enum.GetValues(typeof(ValidationRuleIdentifier));
            var sut = new ChargeLinksCimValidationErrorTextFactory(cimValidationErrorTextProvider);

            // Act
            // Assert
            foreach (var validationRuleIdentifier in validationRuleIdentifiers)
            {
                var listElement = SetListElementWithValidationError(chargeLinksCommand, validationRuleIdentifier);
                var actual = sut.Create(new ValidationError(validationRuleIdentifier, listElement), chargeLinksCommand);
                actual.Should().NotBeNullOrWhiteSpace();
                actual.Should().NotContain("{");
            }
        }

        private static string? SetListElementWithValidationError(
            ChargeLinksCommand chargeLinksCommand, ValidationRuleIdentifier validationRuleIdentifier)
        {
            return validationRuleIdentifier switch
            {
                ValidationRuleIdentifier.ChargeDoesNotExist =>
                chargeLinksCommand.ChargeLinks.First().SenderProvidedChargeId,
                _ => null,
            };
        }
    }
}
