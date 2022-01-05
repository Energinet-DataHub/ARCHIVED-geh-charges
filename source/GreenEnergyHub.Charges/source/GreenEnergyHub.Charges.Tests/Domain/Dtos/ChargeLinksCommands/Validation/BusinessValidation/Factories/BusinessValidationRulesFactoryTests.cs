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
using System.Threading.Tasks;
using AutoFixture.Xunit2;
using GreenEnergyHub.Charges.Domain.Charges;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeLinksCommands;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeLinksCommands.Validation.BusinessValidation.Factories;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeLinksCommands.Validation.BusinessValidation.ValidationRules;
using GreenEnergyHub.Charges.Domain.MeteringPoints;
using GreenEnergyHub.Charges.TestCore.Attributes;
using GreenEnergyHub.Charges.Tests.Builders;
using Moq;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Domain.Dtos.ChargeLinksCommands.Validation.BusinessValidation.Factories
{
    [UnitTest]
    public class BusinessValidationRulesFactoryTests
    {
        [Theory]
        [InlineAutoMoqData(typeof(MeteringPointDoesNotExistRule))]
        public async Task CreateRulesForChargeCommandAsync_WhenMeteringPointDoesNotExist_ReturnsExpectedMandatoryRules(
            Type expectedRule,
            [Frozen] Mock<IMeteringPointRepository> repository,
            BusinessValidationRulesFactory sut,
            ChargeLinksCommandBuilder builder)
        {
            // Arrange
            var chargeCommand = builder.Build();

            MeteringPoint? meteringPoint = null;
            repository.Setup(r => r.GetOrNullAsync(It.IsAny<string>()))
                .ReturnsAsync(meteringPoint);

            // Act
            var actual = await sut.CreateRulesForChargeLinksCommandAsync(chargeCommand).ConfigureAwait(false);
            var actualRules = actual.GetRules().Select(r => r.GetType());

            // Assert
            Assert.Equal(1, actual.GetRules().Count);
            Assert.Contains(expectedRule, actualRules);
        }

        [Theory]
        [InlineAutoMoqData(typeof(ChargeDoesNotExistRule))]
        public async Task CreateRulesForChargeCommandAsync_WhenChargeDoesNotExistForLinks_ReturnsExpectedMandatoryRules(
            Type expectedRule,
            [Frozen] Mock<IMeteringPointRepository> meteringPointRepository,
            [Frozen] Mock<IChargeRepository> chargeRepository,
            MeteringPoint meteringPoint,
            BusinessValidationRulesFactory sut,
            ChargeLinksCommandBuilder linksCommandBuilder,
            ChargeLinkDtoBuilder linksBuilder)
        {
            // Arrange
            var link = linksBuilder.Build();
            var links = new List<ChargeLinkDto> { link };
            var chargeCommand = linksCommandBuilder.WithChargeLinks(links).Build();

            meteringPointRepository.Setup(r => r.GetOrNullAsync(It.IsAny<string>()))
                .ReturnsAsync(meteringPoint);

            Charge? charge = null;
            chargeRepository.Setup(r => r.GetOrNullAsync(It.IsAny<ChargeIdentifier>()))
                .ReturnsAsync(charge);

            // Act
            var actual = await sut.CreateRulesForChargeLinksCommandAsync(chargeCommand).ConfigureAwait(false);
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
            BusinessValidationRulesFactory sut,
            ChargeLinksCommandBuilder linksCommandBuilder,
            ChargeLinkDtoBuilder linksBuilder)
        {
            // Arrange
            var link = linksBuilder.Build();
            var links = new List<ChargeLinkDto> { link };
            var chargeCommand = linksCommandBuilder.WithChargeLinks(links).Build();

            meteringPointRepository.Setup(r => r.GetOrNullAsync(It.IsAny<string>()))
                .ReturnsAsync(meteringPoint);

            chargeRepository.Setup(r => r.GetOrNullAsync(It.IsAny<ChargeIdentifier>()))
                .ReturnsAsync(charge);

            // Act
            var actual = await sut.CreateRulesForChargeLinksCommandAsync(chargeCommand).ConfigureAwait(false);
            var actualRules = actual.GetRules().Select(r => r.GetType());

            // Assert
            Assert.Equal(3, actual.GetRules().Count);
            Assert.Contains(expectedRule, actualRules);
        }

        [Theory]
        [InlineAutoMoqData]
        public static async Task CreateRulesForChargeCommandAsync_WhenCalledWithNull_ThrowsArgumentNullException(
            BusinessValidationRulesFactory sut)
        {
            // Arrange
            ChargeLinksCommand? command = null;

            // Act / Assert
            await Assert.ThrowsAsync<ArgumentNullException>(
                    () => sut.CreateRulesForChargeLinksCommandAsync(command!))
                .ConfigureAwait(false);
        }
    }
}
