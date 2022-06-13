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
using FluentAssertions;
using GreenEnergyHub.Charges.Domain.Charges;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeLinksCommands;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeLinksCommands.Validation.BusinessValidation.Factories;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeLinksCommands.Validation.BusinessValidation.ValidationRules;
using GreenEnergyHub.Charges.Domain.MeteringPoints;
using GreenEnergyHub.Charges.TestCore.Attributes;
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
            ChargeLinkDtoBuilder builder)
        {
            // Arrange
            MeteringPoint? meteringPoint = null;
            var chargeLinkDto = builder.Build();
            SetupMeteringPointRepositoryMock(repository, chargeLinkDto, meteringPoint);

            // Act
            var actual = await sut.CreateRulesAsync(chargeLinkDto).ConfigureAwait(false);
            var actualRules = actual.GetRules().Select(r => r.ValidationRule.GetType());

            // Assert
            actual.GetRules().Count.Should().Be(1);
            actualRules.Should().Contain(expectedRule);
        }

        [Theory]
        [InlineAutoMoqData(typeof(ChargeMustExistRule))]
        public async Task CreateRulesForChargeCommandAsync_WhenChargeDoesNotExistForLinks_ReturnsExpectedMandatoryRules(
            Type expectedRule,
            [Frozen] Mock<IMeteringPointRepository> meteringPointRepository,
            [Frozen] Mock<IChargeIdentifierFactory> chargeIdentifierFactory,
            [Frozen] Mock<IChargeRepository> chargeRepository,
            MeteringPoint meteringPoint,
            ChargeLinksCommandBusinessValidationRulesFactory sut,
            ChargeLinkDtoBuilder linksBuilder)
        {
            // Arrange
            var link = linksBuilder.WithMeteringPointId(meteringPoint.MeteringPointId).Build();
            Charge? charge = null;
            SetupChargeIdentifierFactoryMock(chargeIdentifierFactory);
            SetupChargeRepositoryMock(chargeRepository, charge);
            SetupMeteringPointRepositoryMock(meteringPointRepository, link, meteringPoint);

            // Act
            var actual = await sut.CreateRulesAsync(link).ConfigureAwait(false);
            var actualRules = actual.GetRules().Select(r => r.ValidationRule.GetType());

            // Assert
            actual.GetRules().Count.Should().Be(2);
            actualRules.Should().Contain(expectedRule);
        }

        [Theory]
        [InlineAutoMoqData(typeof(ChargeMustExistRule))]
        [InlineAutoMoqData(typeof(ChargeLinksUpdateNotYetSupportedRule))]
        public async Task CreateRulesForChargeCommandAsync_WhenChargeDoesExist_ReturnsExpectedMandatoryRulesForSingleChargeLinks(
            Type expectedRule,
            [Frozen] Mock<IMeteringPointRepository> meteringPointRepository,
            [Frozen] Mock<IChargeIdentifierFactory> chargeIdentifierFactory,
            [Frozen] Mock<IChargeRepository> chargeRepository,
            MeteringPoint meteringPoint,
            ChargeBuilder chargeBuilder,
            ChargeLinksCommandBusinessValidationRulesFactory sut,
            ChargeLinkDtoBuilder linksBuilder)
        {
            // Arrange
            var charge = chargeBuilder.Build();
            var link = linksBuilder.Build();
            SetupChargeIdentifierFactoryMock(chargeIdentifierFactory);
            SetupChargeRepositoryMock(chargeRepository, charge);
            SetupMeteringPointRepositoryMock(meteringPointRepository, link, meteringPoint);

            // Act
            var actual = await sut.CreateRulesAsync(link).ConfigureAwait(false);
            var actualRules = actual.GetRules().Select(r => r.ValidationRule.GetType());

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
            ChargeLinkDto? chargeLinkDto = null;

            // Act / Assert
            await Assert.ThrowsAsync<ArgumentNullException>(
                    () => sut.CreateRulesAsync(chargeLinkDto!))
                .ConfigureAwait(false);
        }

        private static void SetupChargeIdentifierFactoryMock(Mock<IChargeIdentifierFactory> chargeIdentifierFactory)
        {
            chargeIdentifierFactory
                .Setup(x => x.CreateAsync(It.IsAny<string>(), It.IsAny<ChargeType>(), It.IsAny<string>()))
                .ReturnsAsync(It.IsAny<ChargeIdentifier>());
        }

        private static void SetupChargeRepositoryMock(Mock<IChargeRepository> chargeRepository, Charge? charge)
        {
            chargeRepository.Setup(r => r.SingleOrNullAsync(It.IsAny<ChargeIdentifier>())).ReturnsAsync(charge);
        }

        private static void SetupMeteringPointRepositoryMock(
            Mock<IMeteringPointRepository> repository,
            ChargeLinkDto chargeLinkDto,
            MeteringPoint? meteringPoint)
        {
            repository.Setup(r => r.GetOrNullAsync(chargeLinkDto.MeteringPointId)).ReturnsAsync(meteringPoint);
        }
    }
}
