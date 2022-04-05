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
using GreenEnergyHub.Charges.Domain.Dtos.Validation;
using GreenEnergyHub.Charges.Infrastructure.Core.Cim.ValidationErrors;
using GreenEnergyHub.Charges.MessageHub.Models.AvailableChargeLinksReceiptData;
using GreenEnergyHub.Charges.TestCore.Attributes;
using Microsoft.Extensions.Logging;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.MessageHub.Models.AvailableChargeLinkReceiptData
{
    [UnitTest]
    public class ChargeLinksCimValidationErrorTextFactoryTests
    {
        [Theory]
        [InlineAutoMoqData]
        public void Create_WhenTwoMergeFields_ReturnsExpectedDescription(
            ChargeLinksCommand chargeLinksCommand,
            CimValidationErrorTextProvider cimValidationErrorTextProvider,
            ILoggerFactory loggerFactor)
        {
            // Arrange
            var sut = new ChargeLinksCimValidationErrorTextFactory(cimValidationErrorTextProvider, loggerFactor);
            var chargeLinkDto = chargeLinksCommand.ChargeLinks.First();
            var expected = CimValidationErrorTextTemplateMessages.MeteringPointDoesNotExistValidationErrorText
                .Replace("{{MeteringPointId}}", chargeLinksCommand.MeteringPointId)
                .Replace("{{ChargeLinkStartDate}}", chargeLinkDto.StartDateTime.ToString());

            // Act
            var actual = sut.Create(
                new ValidationError(
                    ValidationRuleIdentifier.MeteringPointDoesNotExist,
                    chargeLinkDto.OperationId,
                    chargeLinkDto.SenderProvidedChargeId),
                chargeLinksCommand);

            // Assert
            actual.Should().Be(expected);
        }

        [Theory]
        [InlineAutoMoqData]
        public void Create_MergesAllMergeFields(
            ChargeLinksCommand chargeLinksCommand,
            CimValidationErrorTextProvider cimValidationErrorTextProvider,
            ILoggerFactory loggerFactor)
        {
            // Arrange
            var chargeLinkDto = chargeLinksCommand.ChargeLinks.First();
            var validationRuleIdentifiers =
                (ValidationRuleIdentifier[])Enum.GetValues(typeof(ValidationRuleIdentifier));
            var sut = new ChargeLinksCimValidationErrorTextFactory(cimValidationErrorTextProvider, loggerFactor);

            // Act
            // Assert
            foreach (var validationRuleIdentifier in validationRuleIdentifiers)
            {
                var triggeredBy = SetTriggeredByWithValidationError(chargeLinksCommand, validationRuleIdentifier);
                var actual = sut.Create(
                    new ValidationError(validationRuleIdentifier, chargeLinkDto.OperationId, triggeredBy),
                    chargeLinksCommand);

                actual.Should().NotBeNullOrWhiteSpace();
                actual.Should().NotContain("{");
                actual.Should().NotContain("  ");
            }
        }

        private static string? SetTriggeredByWithValidationError(
            ChargeLinksCommand chargeLinksCommand, ValidationRuleIdentifier validationRuleIdentifier)
        {
            switch (validationRuleIdentifier)
            {
                case ValidationRuleIdentifier.ChargeDoesNotExist:
                case ValidationRuleIdentifier.MeteringPointDoesNotExist:
                case ValidationRuleIdentifier.ChargeLinkUpdateNotYetSupported:
                    return chargeLinksCommand.ChargeLinks.First().SenderProvidedChargeId;
                default:
                    return null;
            }
        }
    }
}
