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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using AutoFixture.Xunit2;
using FluentAssertions;
using GreenEnergyHub.Charges.Core;
using GreenEnergyHub.Charges.Domain.Charges;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommands;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommands.Validation.BusinessValidation;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommands.Validation.BusinessValidation.Factories;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommands.Validation.BusinessValidation.ValidationRules;
using GreenEnergyHub.Charges.Domain.Dtos.Validation;
using GreenEnergyHub.Charges.Domain.MarketParticipants;
using GreenEnergyHub.Charges.Infrastructure.Core.Cim.ValidationErrors;
using GreenEnergyHub.Charges.MessageHub.Models.Shared;
using GreenEnergyHub.Charges.TestCore.Attributes;
using GreenEnergyHub.Charges.Tests.Builders.Command;
using GreenEnergyHub.Charges.Tests.Builders.Testables;
using Moq;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Domain.Dtos.ChargeCommands.Validation.BusinessValidation
{
    [UnitTest]
    public class ChargeOperationBusinessValidationRulesFactoryTests
    {
        [Theory]
        [InlineAutoMoqData(typeof(StartDateValidationRule))]
        public async Task CreateRulesAsync_WhenCalledWithNewCharge_ReturnsExpectedMandatoryRules(
            Type expectedRule,
            [Frozen] Mock<IChargeIdentifierFactory> chargeIdentifierFactory,
            [Frozen] Mock<IChargeRepository> chargeRepository,
            [Frozen] Mock<IRulesConfigurationRepository> rulesConfigurationRepository,
            ChargeOperationBusinessValidationRulesFactory sut,
            ChargeOperationDtoBuilder builder)
        {
            // Arrange
            var operation = builder.Build();
            Charge? charge = null;

            SetupConfigureRepositoryMock(rulesConfigurationRepository);
            SetupChargeIdentifierFactoryMock(chargeIdentifierFactory);
            SetupChargeRepositoryMock(chargeRepository, charge!);

            // Act
            var actual = await sut.CreateRulesAsync(operation).ConfigureAwait(false);
            var actualRules = actual.GetRules().Select(r => r.ValidationRule.GetType());

            // Assert
            actual.GetRules().Count.Should().Be(1); // This assert is added to ensure that when the rule set is expanded, the test gets attention as well.
            actualRules.Should().Contain(expectedRule);
        }

        [Theory]
        [InlineAutoMoqData(typeof(StartDateValidationRule))]
        [InlineAutoMoqData(typeof(UpdateChargeMustHaveEffectiveDateBeforeOrOnStopDateRule))]
        [InlineAutoMoqData(typeof(ChargeResolutionCanNotBeUpdatedRule))]
        public async Task CreateRulesAsync_WhenCalledWithExistingChargeNotTariff_ReturnsExpectedRules(
            Type expectedRule,
            [Frozen] Mock<IChargeIdentifierFactory> chargeIdentifierFactory,
            [Frozen] Mock<IChargeRepository> chargeRepository,
            [Frozen] Mock<IRulesConfigurationRepository> rulesConfigurationRepository,
            ChargeOperationBusinessValidationRulesFactory sut,
            Charge charge)
        {
            // Arrange
            var chargeOperationDto = new ChargeOperationDtoBuilder().WithChargeType(ChargeType.Fee).Build();
            SetupConfigureRepositoryMock(rulesConfigurationRepository);
            SetupChargeIdentifierFactoryMock(chargeIdentifierFactory);
            SetupChargeRepositoryMock(chargeRepository, charge);

            // Act
            var actual = await sut.CreateRulesAsync(chargeOperationDto).ConfigureAwait(false);
            var actualRules = actual.GetRules().Select(r => r.ValidationRule.GetType());

            // Assert
            Assert.Equal(3, actual.GetRules().Count); // This assert is added to ensure that when the rule set is expanded, the test gets attention as well.
            Assert.Contains(expectedRule, actualRules);
        }

        [Theory]
        [InlineAutoMoqData(typeof(StartDateValidationRule))]
        [InlineAutoMoqData(typeof(ChangingTariffTaxValueNotAllowedRule))]
        [InlineAutoMoqData(typeof(UpdateChargeMustHaveEffectiveDateBeforeOrOnStopDateRule))]
        [InlineAutoMoqData(typeof(ChargeResolutionCanNotBeUpdatedRule))]
        public async Task CreateRulesAsync_WhenCalledWithExistingTariff_ReturnsExpectedRules(
            Type expectedRule,
            [Frozen] Mock<IChargeIdentifierFactory> chargeIdentifierFactory,
            [Frozen] Mock<IChargeRepository> chargeRepository,
            [Frozen] Mock<IRulesConfigurationRepository> rulesConfigurationRepository,
            ChargeOperationBusinessValidationRulesFactory sut,
            Charge charge)
        {
            // Arrange
            var chargeOperationDto = new ChargeOperationDtoBuilder().WithChargeType(ChargeType.Tariff).Build();
            SetupConfigureRepositoryMock(rulesConfigurationRepository);
            SetupChargeRepositoryMock(chargeRepository, charge);
            SetupChargeIdentifierFactoryMock(chargeIdentifierFactory);

            // Act
            var actual = await sut.CreateRulesAsync(chargeOperationDto).ConfigureAwait(false);

            // Assert
            var actualRules = actual.GetRules().Select(r => r.ValidationRule.GetType());
            Assert.Equal(4, actual.GetRules().Count); // This assert is added to ensure that when the rule set is expanded, the test gets attention as well.
            Assert.Contains(expectedRule, actualRules);
        }

        [Theory]
        [InlineAutoMoqData]
        public static async Task CreateRulesAsync_WhenCalledWithNull_ThrowsArgumentNullException(
            ChargeOperationBusinessValidationRulesFactory sut)
        {
            // Arrange
            ChargeOperationDto? chargeOperationDto = null;

            // Act / Assert
            await Assert
                .ThrowsAsync<ArgumentNullException>(() => sut.CreateRulesAsync(chargeOperationDto!))
                .ConfigureAwait(false);
        }

        [Theory]
        [InlineAutoMoqData(CimValidationErrorTextToken.ChargePointPosition)]
        [InlineAutoMoqData(CimValidationErrorTextToken.ChargePointPrice)]
        public async Task CreateRulesAsync_WithChargeCommandAllRulesThatNeedTriggeredByForErrorMessage_MustImplementIValidationRuleWithExtendedData(
            CimValidationErrorTextToken cimValidationErrorTextToken,
            [Frozen] Mock<IChargeIdentifierFactory> chargeIdentifierFactory,
            [Frozen] Mock<IChargeRepository> chargeRepository,
            [Frozen] Mock<IRulesConfigurationRepository> rulesConfigurationRepository,
            ChargeOperationBusinessValidationRulesFactory sut,
            ChargeCommand chargeCommand,
            Charge charge)
        {
            // Arrange
            SetupConfigureRepositoryMock(rulesConfigurationRepository);
            SetupChargeIdentifierFactoryMock(chargeIdentifierFactory);
            SetupChargeRepositoryMock(chargeRepository, charge);

            // Act
            var validationRules = new List<IValidationRuleContainer>();
            foreach (var operation in chargeCommand.ChargeOperations)
            {
                validationRules.AddRange((await sut.CreateRulesAsync(operation)).GetRules().ToList());
            }

            // Assert
            AssertAllRulesThatNeedTriggeredByForErrorMessageImplementsIValidationRuleWithExtendedData(
                cimValidationErrorTextToken, validationRules);
        }

        private static void AssertAllRulesThatNeedTriggeredByForErrorMessageImplementsIValidationRuleWithExtendedData(
            CimValidationErrorTextToken cimValidationErrorTextToken,
            IReadOnlyCollection<IValidationRuleContainer> validationRules)
        {
            var type = typeof(CimValidationErrorTextTemplateMessages);
            foreach (var fieldInfo in type.GetFields(BindingFlags.Static | BindingFlags.Public))
            {
                if (!fieldInfo.GetCustomAttributes().Any()) continue;

                var errorMessageForAttribute = (ErrorMessageForAttribute)fieldInfo.GetCustomAttributes()
                    .Single(x => x.GetType() == typeof(ErrorMessageForAttribute));

                var validationRuleIdentifier = errorMessageForAttribute.ValidationRuleIdentifier;
                var errorText = fieldInfo.GetValue(null)!.ToString();
                var validationErrorTextTokens = CimValidationErrorTextTokenMatcher.GetTokens(errorText!);
                var validationRuleContainer = validationRules
                    .FirstOrDefault(x => x.ValidationRule.ValidationRuleIdentifier == validationRuleIdentifier);

                if (validationErrorTextTokens.Contains(cimValidationErrorTextToken) && validationRuleContainer != null)
                    Assert.True(validationRuleContainer.ValidationRule is IValidationRuleWithExtendedData);
            }
        }

        /// <summary>
        /// Workaround because we haven't yet found a way to have AutoFixture create objects
        /// without parameterless constructors.
        /// </summary>
        private static RulesConfiguration CreateConfiguration()
        {
            return new RulesConfiguration(new StartDateValidationRuleConfiguration(new Interval<int>(31, 1095)));
        }

        private static void SetupConfigureRepositoryMock(
            Mock<IRulesConfigurationRepository> rulesConfigurationRepository)
        {
            var configuration = CreateConfiguration();
            rulesConfigurationRepository
                .Setup(r => r.GetConfigurationAsync())
                .Returns(Task.FromResult(configuration));
        }

        private static void SetupMarketParticipantMock(TestMarketParticipant sender, Mock<IMarketParticipantRepository> marketParticipantRepository)
        {
            marketParticipantRepository
                .Setup(repo => repo.SingleAsync(It.IsAny<string>()))
                .ReturnsAsync(sender);
        }

        private static void SetupChargeIdentifierFactoryMock(Mock<IChargeIdentifierFactory> chargeIdentifierFactory)
        {
            chargeIdentifierFactory
                .Setup(x => x.CreateAsync(It.IsAny<string>(), It.IsAny<ChargeType>(), It.IsAny<string>()))
                .ReturnsAsync(It.IsAny<ChargeIdentifier>());
        }

        private static void SetupChargeRepositoryMock(Mock<IChargeRepository> chargeRepository, Charge charge)
        {
            chargeRepository
                .Setup(r => r.SingleOrNullAsync(It.IsAny<ChargeIdentifier>()))
                .ReturnsAsync(charge);

            chargeRepository
                .Setup(r => r.SingleAsync(It.IsAny<ChargeIdentifier>()))
                .Returns(Task.FromResult(charge));
        }
    }
}
