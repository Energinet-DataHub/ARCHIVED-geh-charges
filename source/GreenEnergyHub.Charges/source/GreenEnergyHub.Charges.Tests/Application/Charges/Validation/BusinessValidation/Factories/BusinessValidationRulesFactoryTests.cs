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
using System.Threading.Tasks;
using AutoFixture.Xunit2;
using GreenEnergyHub.Charges.Core;
using GreenEnergyHub.Charges.Domain.Charges;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommands;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommands.Validation.BusinessValidation;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommands.Validation.BusinessValidation.Factories;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommands.Validation.BusinessValidation.ValidationRules;
using GreenEnergyHub.Charges.TestCore.Attributes;
using GreenEnergyHub.Charges.Tests.Builders;
using Moq;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Application.Charges.Validation.BusinessValidation.Factories
{
    [UnitTest]
    public class BusinessValidationRulesFactoryTests
    {
        [Theory]
        [InlineAutoMoqData(typeof(StartDateValidationRule))]
        [InlineAutoMoqData(typeof(CommandSenderMustBeAnExistingMarketParticipantRule))]
        [InlineAutoMoqData(typeof(ChargeUpdateNotYetSupportedRule))]
        public async Task CreateRulesForChargeCommandAsync_WhenCalledWithNewCharge_ReturnsExpectedMandatoryRules(
            Type expectedRule,
            [Frozen] Mock<IChargeRepository> repository,
            [Frozen] Mock<IRulesConfigurationRepository> rulesConfigurationRepository,
            BusinessValidationRulesFactory sut,
            ChargeCommandTestBuilder builder)
        {
            // Arrange
            var chargeCommand = builder.Build();
            ConfigureRepositoryMock(rulesConfigurationRepository);

            Charge? charge = null;
            repository.Setup(r => r.GetOrNullAsync(It.IsAny<ChargeIdentifier>()))
                .ReturnsAsync(charge);

            // Act
            var actual = await sut.CreateRulesForChargeCommandAsync(chargeCommand).ConfigureAwait(false);
            var actualRules = actual.GetRules().Select(r => r.GetType());

            // Assert
            Assert.Equal(3, actual.GetRules().Count); // This assert is added to ensure that when the rule set is expanded, the test gets attention as well.
            Assert.Contains(expectedRule, actualRules);
        }

        [Theory]
        [InlineAutoMoqData(typeof(StartDateValidationRule))]
        [InlineAutoMoqData(typeof(CommandSenderMustBeAnExistingMarketParticipantRule))]
        [InlineAutoMoqData(typeof(ChargeUpdateNotYetSupportedRule))]
        public async Task CreateRulesForChargeCommandAsync_WhenCalledWithExistingChargeNotTariff_ReturnsExpectedRules(
            Type expectedRule,
            [Frozen] Mock<IChargeRepository> repository,
            [Frozen] Mock<IRulesConfigurationRepository> rulesConfigurationRepository,
            BusinessValidationRulesFactory sut,
            ChargeCommandTestBuilder builder,
            Charge charge)
        {
            // Arrange
            var chargeCommand = builder.WithChargeType(ChargeType.Fee).Build();
            ConfigureRepositoryMock(rulesConfigurationRepository);

            repository
                .Setup(r => r.GetOrNullAsync(It.IsAny<ChargeIdentifier>()))
                .ReturnsAsync(charge);

            // Act
            var actual = await sut.CreateRulesForChargeCommandAsync(chargeCommand).ConfigureAwait(false);
            var actualRules = actual.GetRules().Select(r => r.GetType());

            // Assert
            Assert.Equal(3, actual.GetRules().Count); // This assert is added to ensure that when the rule set is expanded, the test gets attention as well.
            Assert.Contains(expectedRule, actualRules);
        }

        [Theory]
        [InlineAutoMoqData(typeof(StartDateValidationRule))]
        [InlineAutoMoqData(typeof(CommandSenderMustBeAnExistingMarketParticipantRule))]
        [InlineAutoMoqData(typeof(ChangingTariffTaxValueNotAllowedRule))]
        [InlineAutoMoqData(typeof(ChangingTariffVatValueNotAllowedRule))]
        [InlineAutoMoqData(typeof(ChargeUpdateNotYetSupportedRule))]
        public async Task CreateRulesForChargeCommandAsync_WhenCalledWithExistingTariff_ReturnsExpectedRules(
            Type expectedRule,
            [Frozen] Mock<IChargeRepository> repository,
            [Frozen] Mock<IRulesConfigurationRepository> rulesConfigurationRepository,
            BusinessValidationRulesFactory sut,
            ChargeCommandTestBuilder builder,
            Charge charge)
        {
            // Arrange
            var chargeCommand = builder.WithChargeType(ChargeType.Tariff).Build();
            ConfigureRepositoryMock(rulesConfigurationRepository);

            repository.Setup(
                    r => r.GetOrNullAsync(
                        It.IsAny<ChargeIdentifier>()))
                .ReturnsAsync(charge);

            repository.Setup(
                    r => r.GetAsync(
                        It.IsAny<ChargeIdentifier>()))
                .Returns(Task.FromResult(charge));

            // Act
            var actual = await sut.CreateRulesForChargeCommandAsync(chargeCommand).ConfigureAwait(false);
            var actualRules = actual.GetRules().Select(r => r.GetType());

            // Assert
            Assert.Equal(5, actual.GetRules().Count); // This assert is added to ensure that when the rule set is expanded, the test gets attention as well.
            Assert.Contains(expectedRule, actualRules);
        }

        [Theory]
        [InlineAutoMoqData]
        public static async Task CreateRulesForChargeCommandAsync_WhenCalledWithNull_ThrowsArgumentNullException(
            BusinessValidationRulesFactory sut)
        {
            // Arrange
            ChargeCommand? command = null;

            // Act / Assert
            await Assert.ThrowsAsync<ArgumentNullException>(
                    () => sut.CreateRulesForChargeCommandAsync(command!))
                .ConfigureAwait(false);
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

        private static void ConfigureRepositoryMock(
            Mock<IRulesConfigurationRepository> rulesConfigurationRepository)
        {
            var configuration = CreateConfiguration();
            rulesConfigurationRepository
                .Setup(r => r.GetConfigurationAsync())
                .Returns(Task.FromResult(configuration));
        }
    }
}
