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
using GreenEnergyHub.Charges.Domain.Dtos.SharedDtos;
using GreenEnergyHub.Charges.Domain.Dtos.Validation;
using GreenEnergyHub.Charges.Infrastructure.Core.Cim.ValidationErrors;
using GreenEnergyHub.Charges.MessageHub.Models.AvailableChargeLinksReceiptData;
using GreenEnergyHub.Charges.TestCore.Attributes;
using GreenEnergyHub.Charges.Tests.TestCore;
using Microsoft.Extensions.Logging;
using Moq;
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
            ChargeLinkOperationDto chargeLinkOperationDto,
            CimValidationErrorTextProvider cimValidationErrorTextProvider,
            ILoggerFactory loggerFactory)
        {
            // Arrange
            var validationError = new ValidationError(
                ValidationRuleIdentifier.MeteringPointDoesNotExist, chargeLinkOperationDto.SenderProvidedChargeId, null!);
            var sut = new ChargeLinksCimValidationErrorTextFactory(cimValidationErrorTextProvider, loggerFactory);
            var expected = CimValidationErrorTextTemplateMessages.MeteringPointDoesNotExistValidationErrorText
                .Replace("{{MeteringPointId}}", chargeLinkOperationDto.MeteringPointId)
                .Replace("{{ChargeLinkStartDate}}", chargeLinkOperationDto.StartDateTime.ToString());

            // Act
            var actual = sut.Create(validationError, It.IsAny<DocumentDto>(), chargeLinkOperationDto);

            // Assert
            actual.Should().Be(expected);
        }

        [Theory]
        [InlineAutoMoqData]
        public void Create_MergesAllMergeFields(
            ChargeLinksCommand chargeLinksCommand,
            CimValidationErrorTextProvider cimValidationErrorTextProvider,
            ILoggerFactory loggerFactory)
        {
            // Arrange
            var chargeLinkDto = chargeLinksCommand.Operations.First();
            var validationRuleIdentifiers = (ValidationRuleIdentifier[])Enum.GetValues(typeof(ValidationRuleIdentifier));
            var identifiersForRulesWithExtendedData =
                ValidationRuleForInterfaceLoader.GetValidationRuleIdentifierForTypes(
                    AssemblyHelper.GetDomainAssembly(), typeof(IValidationRuleWithExtendedData));

            var sut = new ChargeLinksCimValidationErrorTextFactory(cimValidationErrorTextProvider, loggerFactory);

            // Act
            // Assert
            foreach (var validationRuleIdentifier in validationRuleIdentifiers)
            {
                var triggeredBy = validationRuleIdentifier == ValidationRuleIdentifier.SubsequentBundleOperationsFail ?
                    chargeLinksCommand.Operations.First().OperationId :
                    null!;

                var actual = sut.Create(
                    new ValidationError(validationRuleIdentifier, chargeLinkDto.OperationId, triggeredBy),
                    It.IsAny<DocumentDto>(),
                    chargeLinkDto);

                actual.Should().NotBeNullOrWhiteSpace();
                actual.Should().NotContain("{");
                actual.Should().NotContain("  ");

                if (identifiersForRulesWithExtendedData.Contains(validationRuleIdentifier) && triggeredBy != null)
                    actual.Should().NotContain("unknown");
            }
        }
    }
}
