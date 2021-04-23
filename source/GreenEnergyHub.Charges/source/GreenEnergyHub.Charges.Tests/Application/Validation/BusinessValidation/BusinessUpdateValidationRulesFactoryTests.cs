﻿// Copyright 2020 Energinet DataHub A/S
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
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture.Xunit2;
using FluentAssertions;
using GreenEnergyHub.Charges.Application.Validation.BusinessValidation;
using GreenEnergyHub.Charges.Application.Validation.BusinessValidation.Rules;
using GreenEnergyHub.Charges.Domain;
using GreenEnergyHub.Charges.Domain.ChangeOfCharges.Transaction;
using GreenEnergyHub.TestHelpers;
using Moq;
using Xunit;

namespace GreenEnergyHub.Charges.Tests.Application.Validation.BusinessValidation
{
    public class BusinessUpdateValidationRulesFactoryTests
    {
        [Theory]
        [InlineAutoDomainData]
        public async Task CreateRulesForUpdateCommandAsync_ReturnsRulesForTariffUpdateCommand(
            [NotNull][Frozen] Mock<IUpdateRulesConfigurationRepository> updateRulesConfigurationRepository,
            [NotNull] BusinessUpdateValidationRulesFactory sut,
            [NotNull] ChargeCommand chargeCommand)
        {
            // Arrange
            var configuration = CreateConfiguration();
            updateRulesConfigurationRepository
                .Setup(r => r.GetConfigurationAsync())
                .Returns(Task.FromResult(configuration));

            var expectedRules = new HashSet<Type>
            {
                typeof(VatPayerMustNotChangeInUpdateRule),
                typeof(TaxIndicatorMustNotChangeInUpdateRule),
                typeof(StartDateVr209ValidationRule),
            };

            var tariffUpdateCommand = TurnCommandIntoSpecifiedUpdateType(chargeCommand, ChargeCommandType.Tariff);

            // Act
            var actual = await sut.CreateRulesForUpdateCommandAsync(tariffUpdateCommand).ConfigureAwait(false);

            // Assert
            var actualRules = actual.GetRules().Select(r => r.GetType());
            actualRules.Should().BeEquivalentTo(expectedRules);
        }

        [Theory]
        [InlineAutoDomainData]
        public async Task CreateRulesForUpdateCommandAsync_ReturnsRulesForFeeUpdateCommand(
            [NotNull][Frozen] Mock<IUpdateRulesConfigurationRepository> updateRulesConfigurationRepository,
            [NotNull] BusinessUpdateValidationRulesFactory sut,
            [NotNull] ChargeCommand chargeCommand)
        {
            // Arrange
            var configuration = CreateConfiguration();
            updateRulesConfigurationRepository
                .Setup(r => r.GetConfigurationAsync())
                .Returns(Task.FromResult(configuration));

            var expectedRules = new HashSet<Type>
            {
                typeof(StartDateVr209ValidationRule),
            };

            var feeUpdateCommand = TurnCommandIntoSpecifiedUpdateType(chargeCommand, ChargeCommandType.Fee);

            // Act
            var actual = await sut.CreateRulesForUpdateCommandAsync(feeUpdateCommand).ConfigureAwait(false);

            // Assert
            var actualRules = actual.GetRules().Select(r => r.GetType());
            actualRules.Should().BeEquivalentTo(expectedRules);
        }

        [Theory]
        [InlineAutoDomainData]
        public async Task CreateRulesForUpdateCommandAsync_ReturnsRulesForSubscriptionUpdateCommand(
            [NotNull][Frozen] Mock<IUpdateRulesConfigurationRepository> updateRulesConfigurationRepository,
            [NotNull] BusinessUpdateValidationRulesFactory sut,
            [NotNull] ChargeCommand chargeCommand)
        {
            // Arrange
            var configuration = CreateConfiguration();
            updateRulesConfigurationRepository
                .Setup(r => r.GetConfigurationAsync())
                .Returns(Task.FromResult(configuration));

            var expectedRules = new HashSet<Type>
            {
                typeof(StartDateVr209ValidationRule),
            };

            var subscriptionUpdateCommand = TurnCommandIntoSpecifiedUpdateType(chargeCommand, ChargeCommandType.Subscription);

            // Act
            var actual = await sut.CreateRulesForUpdateCommandAsync(subscriptionUpdateCommand).ConfigureAwait(false);

            // Assert
            var actualRules = actual.GetRules().Select(r => r.GetType());
            actualRules.Should().BeEquivalentTo(expectedRules);
        }

        /// <summary>
        /// Workaround because we haven't yet found a way to have AutoFixture create objects
        /// without parameterless constructors.
        /// </summary>
        private static UpdateRulesConfiguration CreateConfiguration()
        {
            return new UpdateRulesConfiguration(new StartDateVr209ValidationRuleConfiguration(new Interval<int>(1, 2)));
        }

        private static ChargeCommand TurnCommandIntoSpecifiedUpdateType(ChargeCommand chargeCommand, string commandType)
        {
            chargeCommand.Type = commandType;
            chargeCommand!.MktActivityRecord!.Status = MktActivityRecordStatus.Change;
            return chargeCommand;
        }
    }
}
