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
using GreenEnergyHub.Charges.Domain.Dtos.ChargePriceCommands.Validation.BusinessValidation.ValidationRules;
using GreenEnergyHub.Charges.Domain.Dtos.SharedDtos;
using GreenEnergyHub.Charges.Domain.Dtos.Validation;
using GreenEnergyHub.Charges.TestCore.Attributes;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Domain.Dtos.ChargePriceCommands.Validation.BusinessRules.ValidationRules
{
    [UnitTest]
    public class UpdateTaxTariffOnlyAllowedBySystemOperatorRuleTests
    {
        [Theory]
        [InlineAutoMoqData(ChargeType.Fee)]
        [InlineAutoMoqData(ChargeType.Subscription)]
        [InlineAutoMoqData(ChargeType.Unknown)]
        public void IsValid_WhenChargeTypeNotTariff_IsTrue(
            ChargeType chargeType)
        {
            // Arrange
            // Act
            var sut = new UpdateTaxTariffOnlyAllowedBySystemOperatorRule(chargeType, MarketParticipantRole.GridAccessProvider, false);

            // Assert
            sut.IsValid.Should().Be(true);
        }

        [Fact]
        public void IsValid_WhenNotSystemOperator_IsTrue()
        {
            foreach (var senderRole in Enum.GetValues<MarketParticipantRole>())
            {
                // Arrange
                var expectedResult = senderRole == MarketParticipantRole.SystemOperator;

                // Act
                var sut = new UpdateTaxTariffOnlyAllowedBySystemOperatorRule(ChargeType.Tariff, senderRole, true);

                // Assert
                sut.IsValid.Should().Be(expectedResult);
            }
        }

        [Fact]
        public void IsValid_WhenTaxIndicatorIsFalse_IsTrue()
        {
            // Arrange
            // Act
            var sut = new UpdateTaxTariffOnlyAllowedBySystemOperatorRule(
                ChargeType.Tariff,
                MarketParticipantRole.GridAccessProvider,
                false);

            // Assert
            sut.IsValid.Should().Be(true);
        }

        [Fact]
        public void ValidationRuleIdentifier_ShouldBeUpdateTaxTariffOnlyAllowedBySystemOperator()
        {
            // Arrange
            // Act
            var sut = new UpdateTaxTariffOnlyAllowedBySystemOperatorRule(ChargeType.Tariff, MarketParticipantRole.SystemOperator, true);

            // Assert
            sut.ValidationRuleIdentifier.Should().Be(ValidationRuleIdentifier.UpdateTaxTariffOnlyAllowedBySystemOperator);
        }
    }
}
