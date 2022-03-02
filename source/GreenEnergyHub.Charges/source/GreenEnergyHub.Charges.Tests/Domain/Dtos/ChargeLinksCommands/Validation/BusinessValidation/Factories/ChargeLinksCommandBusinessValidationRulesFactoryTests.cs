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
using GreenEnergyHub.Charges.Domain.Charges;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeLinksCommands;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeLinksCommands.Validation.BusinessValidation.Factories;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeLinksCommands.Validation.BusinessValidation.ValidationRules;
using GreenEnergyHub.Charges.Domain.Dtos.Validation;
using GreenEnergyHub.Charges.Domain.MeteringPoints;
using GreenEnergyHub.Charges.Infrastructure.Core.Cim.ValidationErrors;
using GreenEnergyHub.Charges.MessageHub.Models.Shared;
using GreenEnergyHub.Charges.TestCore.Attributes;
using GreenEnergyHub.Charges.Tests.Builders;
using GreenEnergyHub.Charges.Tests.Builders.Command;
using Moq;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Domain.Dtos.ChargeLinksCommands.Validation.BusinessValidation.Factories
{
    [UnitTest]
    public class ChargeLinksCommandBusinessValidationRulesFactoryTests
    {
        [Theory]
        [InlineAutoMoqData(typeof(MeteringPointMustExistRule))]
        public async Task CreateRulesForChargeCommandAsync_WhenMeteringPointDoesNotExist_ReturnsExpectedMandatoryRules(
            Type expectedRule,
            [Frozen] Mock<IMeteringPointRepository> repository,
            ChargeLinksCommandBusinessValidationRulesFactory sut,
            ChargeLinksCommandBuilder builder)
        {
            // Arrange
            var chargeLinksCommand = builder.Build();

            MeteringPoint? meteringPoint = null;
            SetupMeteringPointRepositoryMock(repository, chargeLinksCommand, meteringPoint);

            // Act
            var actual = await sut.CreateRulesAsync(chargeLinksCommand).ConfigureAwait(false);
            var actualRules = actual.GetRules().Select(r => r.GetType());

            // Assert
            Assert.Equal(1, actual.GetRules().Count);
            Assert.Contains(expectedRule, actualRules);
        }

        [Theory]
        [InlineAutoMoqData(typeof(ChargeMustExistRule))]
        public async Task CreateRulesForChargeCommandAsync_WhenChargeDoesNotExistForLinks_ReturnsExpectedMandatoryRules(
            Type expectedRule,
            [Frozen] Mock<IMeteringPointRepository> meteringPointRepository,
            [Frozen] Mock<IChargeRepository> chargeRepository,
            MeteringPoint meteringPoint,
            ChargeLinksCommandBusinessValidationRulesFactory sut,
            ChargeLinksCommandBuilder linksCommandBuilder,
            ChargeLinkDtoBuilder linksBuilder)
        {
            // Arrange
            var link = linksBuilder.Build();
            var links = new List<ChargeLinkDto> { link };
            var chargeLinksCommand = linksCommandBuilder.WithChargeLinks(links).Build();

            Charge? charge = null;
            SetupChargeRepositoryMock(chargeRepository, charge);
            SetupMeteringPointRepositoryMock(meteringPointRepository, chargeLinksCommand, meteringPoint);

            // Act
            var actual = await sut.CreateRulesAsync(chargeLinksCommand).ConfigureAwait(false);
            var actualRules = actual.GetRules().Select(r => r.GetType());

            // Assert
            Assert.Equal(2, actual.GetRules().Count);
            Assert.Contains(expectedRule, actualRules);
        }

        [Theory]
        [InlineAutoMoqData(typeof(ChargeLinksUpdateNotYetSupportedRule))]
        public async Task CreateRulesForChargeCommandAsync_WhenChargeDoesExist_ReturnsExpectedMandatoryRulesForSingleChargeLinks(
            Type expectedRule,
            [Frozen] Mock<IMeteringPointRepository> meteringPointRepository,
            [Frozen] Mock<IChargeRepository> chargeRepository,
            MeteringPoint meteringPoint,
            Charge charge,
            ChargeLinksCommandBusinessValidationRulesFactory sut,
            ChargeLinksCommandBuilder linksCommandBuilder,
            ChargeLinkDtoBuilder linksBuilder)
        {
            // Arrange
            var link = linksBuilder.Build();
            var links = new List<ChargeLinkDto> { link };
            var chargeLinksCommand = linksCommandBuilder.WithChargeLinks(links).Build();

            SetupChargeRepositoryMock(chargeRepository, charge);
            SetupMeteringPointRepositoryMock(meteringPointRepository, chargeLinksCommand, meteringPoint);

            // Act
            var actual = await sut.CreateRulesAsync(chargeLinksCommand).ConfigureAwait(false);
            var actualRules = actual.GetRules().Select(r => r.GetType());

            // Assert
            Assert.Equal(3, actual.GetRules().Count);
            Assert.Contains(expectedRule, actualRules);
        }

        [Theory]
        [InlineAutoMoqData]
        public static async Task CreateRulesForChargeCommandAsync_WhenCalledWithNull_ThrowsArgumentNullException(
            ChargeLinksCommandBusinessValidationRulesFactory sut)
        {
            // Arrange
            ChargeLinksCommand? command = null;

            // Act / Assert
            await Assert.ThrowsAsync<ArgumentNullException>(
                    () => sut.CreateRulesAsync(command!))
                .ConfigureAwait(false);
        }

        [Theory]
        [InlineAutoMoqData(CimValidationErrorTextToken.ChargeLinkStartDate)]
        [InlineAutoMoqData(CimValidationErrorTextToken.DocumentSenderProvidedChargeId)]
        public async Task CreateRulesAsync_AllRulesThatNeedTriggeredByForErrorMessage_MustImplementIValidationRuleWithExtendedData(
            CimValidationErrorTextToken cimValidationErrorTextToken,
            [Frozen] Mock<IMeteringPointRepository> meteringPointRepository,
            [Frozen] Mock<IChargeRepository> chargeRepository,
            ChargeLinksCommandBusinessValidationRulesFactory sut,
            ChargeLinksCommand chargeLinksCommand,
            MeteringPoint meteringPoint,
            Charge charge)
        {
            // Arrange
            SetupChargeRepositoryMock(chargeRepository, charge);
            SetupMeteringPointRepositoryMock(meteringPointRepository, chargeLinksCommand, meteringPoint);

            // Act
            var validationRules = (await sut.CreateRulesAsync(chargeLinksCommand)).GetRules();

            // Assert
            var type = typeof(CimValidationErrorTextTemplateMessages);
            foreach (var fieldInfo in type.GetFields(BindingFlags.Static | BindingFlags.Public))
            {
                if (!fieldInfo.GetCustomAttributes().Any()) continue;

                var errorMessageForAttribute = (ErrorMessageForAttribute)fieldInfo.GetCustomAttributes()
                    .Single(x => x.GetType() == typeof(ErrorMessageForAttribute));

                var validationRuleIdentifier = errorMessageForAttribute.ValidationRuleIdentifier;
                var errorText = fieldInfo.GetValue(null)!.ToString();
                var validationErrorTextTokens = CimValidationErrorTextTokenMatcher.GetTokens(errorText!);
                var validationRule = validationRules.FirstOrDefault(x => x.ValidationRuleIdentifier == validationRuleIdentifier);

                if (validationErrorTextTokens.Contains(cimValidationErrorTextToken) && validationRule != null)
                {
                    Assert.True(validationRule is IValidationRuleWithExtendedData);
                }
            }
        }

        private static void SetupChargeRepositoryMock(Mock<IChargeRepository> chargeRepository, Charge? charge)
        {
            chargeRepository.Setup(r => r.GetOrNullAsync(It.IsAny<ChargeIdentifier>())).ReturnsAsync(charge);
        }

        private static void SetupMeteringPointRepositoryMock(
            Mock<IMeteringPointRepository> repository,
            ChargeLinksCommand chargeLinksCommand,
            MeteringPoint? meteringPoint)
        {
            repository.Setup(r => r.GetOrNullAsync(chargeLinksCommand.MeteringPointId)).ReturnsAsync(meteringPoint);
        }
    }
}
