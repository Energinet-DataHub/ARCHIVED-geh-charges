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
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture.Xunit2;
using FluentAssertions;
using GreenEnergyHub.Charges.Application.Validation.BusinessValidation;
using GreenEnergyHub.Charges.Application.Validation.BusinessValidation.Factories;
using GreenEnergyHub.Charges.Application.Validation.BusinessValidation.ValidationRules;
using GreenEnergyHub.Charges.Core;
using GreenEnergyHub.Charges.Domain.ChangeOfCharges.Transaction;
using GreenEnergyHub.Charges.TestCore;
using Moq;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Application.Validation.BusinessValidation.Factories
{
    [UnitTest]
    public class BusinessUpdateValidationRulesFactoryTests
    {
        [Theory]

        // Subscription update
        [InlineAutoMoqData(typeof(StartDateValidationRule), ChargeType.Subscription)]
        [InlineAutoMoqData(typeof(CommandSenderMustBeAnExistingPartyRule), ChargeType.Subscription)]

        // Fee update
        [InlineAutoMoqData(typeof(StartDateValidationRule), ChargeType.Fee)]
        [InlineAutoMoqData(typeof(CommandSenderMustBeAnExistingPartyRule), ChargeType.Fee)]

        // Tariff update
        [InlineAutoMoqData(typeof(ChangingTariffTaxValueNotAllowedRule), ChargeType.Tariff)]
        [InlineAutoMoqData(typeof(StartDateValidationRule), ChargeType.Tariff)]
        [InlineAutoMoqData(typeof(CommandSenderMustBeAnExistingPartyRule), ChargeType.Tariff)]
        public async Task CreateRulesForUpdateCommandAsync_ReturnsRulesContainingExpectedRule(
            Type expectedRuleType,
            ChargeType chargeType,
            [NotNull] [Frozen] Mock<IRulesConfigurationRepository> updateRulesConfigurationRepository,
            [NotNull] BusinessUpdateValidationRulesFactory sut,
            [NotNull] TestableChargeCommand chargeCommand)
        {
            // Arrange
            ConfigureRepositoryMock(updateRulesConfigurationRepository);
            var command = TurnCommandIntoSpecifiedUpdateType(chargeCommand, chargeType);

            // Act
            var actual = await sut.CreateRulesForUpdateCommandAsync(command).ConfigureAwait(false);

            // Assert
            var actualRules = actual.GetRules().Select(r => r.GetType());
            actualRules.Should().Contain(expectedRuleType);
        }

        /// <summary>
        /// Workaround because we haven't yet found a way to have AutoFixture create objects
        /// without parameterless constructors.
        /// </summary>
        private static RulesConfiguration CreateConfiguration()
        {
            return new RulesConfiguration(
                new StartDateValidationRuleConfiguration(new Interval<int>(31, 1095)));
        }

        private static ChargeCommand TurnCommandIntoSpecifiedUpdateType(ChargeCommand chargeCommand, ChargeType chargeType)
        {
            chargeCommand.ChargeOperation.Type = chargeType;
            chargeCommand.ChargeOperation.OperationType = OperationType.Update;
            return chargeCommand;
        }

        private static void ConfigureRepositoryMock(
            Mock<IRulesConfigurationRepository> updateRulesConfigurationRepository)
        {
            var configuration = CreateConfiguration();
            updateRulesConfigurationRepository
                .Setup(r => r.GetConfigurationAsync())
                .Returns(Task.FromResult(configuration));
        }
    }
}
