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
using GreenEnergyHub.Charges.Domain.Charges;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeInformationCommands.Validation.InputValidation.ValidationRules;
using GreenEnergyHub.Charges.Domain.Dtos.SharedDtos;
using GreenEnergyHub.Charges.Domain.Dtos.Validation;
using GreenEnergyHub.Charges.TestCore.Attributes;
using GreenEnergyHub.Charges.Tests.Builders.Command;
using GreenEnergyHub.TestHelpers;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Domain.Dtos.ChargeCommands.Validation.InputValidation.ValidationRules
{
    [UnitTest]
    public class ChargeTypeTariffTaxIndicatorValidationRuleTests
    {
        [Theory]
        [InlineAutoMoqData(ChargeType.Fee)]
        [InlineAutoMoqData(ChargeType.Subscription)]
        [InlineAutoMoqData(ChargeType.Unknown)]
        public void ChargeTypeTariffTaxIndicatorValidationRule_WhenChargeTypeNotTariff_ReturnsTrue(
            ChargeType chargeType,
            ChargeInformationOperationDtoBuilder chargeInformationOperationDtoBuilder,
            MarketParticipantDtoBuilder marketParticipantDtoBuilder)
        {
            // Arrange
            var senderMarketParticipant = marketParticipantDtoBuilder
                .WithMarketParticipantRole(MarketParticipantRole.GridAccessProvider)
                .Build();
            var chargeOperation = chargeInformationOperationDtoBuilder
                .WithChargeType(chargeType)
                .Build();

            // Act
            var sut = new ChargeTypeTariffTaxIndicatorValidationRule(chargeOperation, senderMarketParticipant);

            // Assert
            sut.IsValid.Should().Be(true);
        }

        [Theory]
        [AutoDomainData]
        public void ChargeTypeTariffTaxIndicatorValidationRule_WhenNotSystemOperator_ReturnsTrue(
            ChargeInformationOperationDtoBuilder chargeInformationOperationDtoBuilder,
            MarketParticipantDtoBuilder marketParticipantDtoBuilder)
        {
            foreach (var senderRole in Enum.GetValues<MarketParticipantRole>())
            {
                // Arrange
                var expectedResult = senderRole == MarketParticipantRole.SystemOperator;
                var senderMarketParticipant = marketParticipantDtoBuilder
                    .WithMarketParticipantRole(senderRole)
                    .Build();
                var chargeOperation = chargeInformationOperationDtoBuilder
                    .WithChargeType(ChargeType.Tariff)
                    .WithTaxIndicator(TaxIndicator.Tax)
                    .Build();

                // Act
                var sut = new ChargeTypeTariffTaxIndicatorValidationRule(chargeOperation, senderMarketParticipant);

                // Assert
                sut.IsValid.Should().Be(expectedResult);
            }
        }

        [Theory]
        [AutoDomainData]
        public void IsValid_WhenTaxIndicatorIsFalse_ReturnsTrue(
            ChargeInformationOperationDtoBuilder chargeInformationOperationDtoBuilder,
            MarketParticipantDtoBuilder marketParticipantDtoBuilder)
        {
            // Arrange
            var senderMarketParticipant = marketParticipantDtoBuilder
                .WithMarketParticipantRole(MarketParticipantRole.GridAccessProvider)
                .Build();
            var chargeOperation = chargeInformationOperationDtoBuilder
                .WithChargeType(ChargeType.Tariff)
                .WithTaxIndicator(TaxIndicator.NoTax)
                .Build();

            // Act
            var sut = new ChargeTypeTariffTaxIndicatorValidationRule(chargeOperation, senderMarketParticipant);

            // Assert
            sut.IsValid.Should().Be(true);
        }

        [Theory]
        [InlineAutoDomainData]
        public void ValidationRuleIdentifier_ShouldBeChargeTypeTariffTaxIndicatorOnlyAllowedBySystemOperator(
            ChargeInformationOperationDtoBuilder chargeInformationOperationDtoBuilder,
            MarketParticipantDtoBuilder marketParticipantDtoBuilder)
        {
            // Arrange
            var senderMarketParticipant = marketParticipantDtoBuilder
                .WithMarketParticipantRole(MarketParticipantRole.SystemOperator)
                .Build();
            var chargeOperation = chargeInformationOperationDtoBuilder
                .WithChargeType(ChargeType.Tariff)
                .Build();

            // Act
            var sut = new ChargeTypeTariffTaxIndicatorValidationRule(chargeOperation, senderMarketParticipant);

            // Assert
            sut.ValidationRuleIdentifier.Should().Be(ValidationRuleIdentifier.ChargeTypeTariffTaxIndicatorOnlyAllowedBySystemOperator);
        }
    }
}
