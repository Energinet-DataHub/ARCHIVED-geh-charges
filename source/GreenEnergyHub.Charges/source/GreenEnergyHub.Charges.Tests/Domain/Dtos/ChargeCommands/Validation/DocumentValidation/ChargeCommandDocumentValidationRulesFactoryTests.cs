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

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture.Xunit2;
using GreenEnergyHub.Charges.Domain.Charges;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommands.Validation.DocumentValidation.Factories;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommands.Validation.DocumentValidation.ValidationRules;
using GreenEnergyHub.Charges.Domain.Dtos.Validation;
using GreenEnergyHub.Charges.Domain.MarketParticipants;
using GreenEnergyHub.Charges.TestCore.Attributes;
using GreenEnergyHub.Charges.Tests.Builders.Command;
using GreenEnergyHub.Charges.Tests.Builders.Testables;
using Moq;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Domain.Dtos.ChargeCommands.Validation.DocumentValidation
{
    [UnitTest]
    public class ChargeCommandDocumentValidationRulesFactoryTests
    {
        [Theory]
        [InlineAutoMoqData]
        public async Task CreateRulesForChargeCommand_ShouldContainRulesTest(
            TestMarketParticipant sender,
            [Frozen] Mock<IMarketParticipantRepository> marketParticipantRepository,
            ChargeCommandDocumentValidationRulesFactory sut)
        {
            // Arrange
            marketParticipantRepository
                .Setup(r => r.GetOrNullAsync(It.IsAny<string>()))
                .ReturnsAsync(sender);
            var chargeCommand = new ChargeCommandBuilder().Build();
            var expectedRules = new List<IValidationRule>
            {
                new CommandSenderMustBeAnExistingMarketParticipantRule(sender),
                new BusinessReasonCodeMustBeUpdateChargeInformationOrChargePricesRule(chargeCommand.Document),
                new DocumentTypeMustBeRequestChangeOfPriceListRule(chargeCommand.Document),
                new RecipientIsMandatoryTypeValidationRule(chargeCommand.Document),
                new SenderIsMandatoryTypeValidationRule(chargeCommand.Document),
            };

            // Act
            var rules = await sut.CreateRulesAsync(chargeCommand).ConfigureAwait(false);
            var actualRuleTypes = rules.GetRules().Select(r => r.GetType()).ToList();
            var expectedRuleTypes = expectedRules.Select(r => r.GetType()).ToList();

            // Assert
            Assert.True(actualRuleTypes.SequenceEqual(expectedRuleTypes));
        }

        [Theory]
        [InlineAutoMoqData]
        public async Task CreateRulesForChargeCommandAsync_WhenCalledWithExistingChargeNotTariff_ReturnsExpectedRules(
            TestMarketParticipant sender,
            [Frozen] Mock<IMarketParticipantRepository> marketParticipantRepository,
            [Frozen] Mock<IChargeRepository> chargeRepository,
            ChargeCommandDocumentValidationRulesFactory sut,
            Charge charge)
        {
            // Arrange
            var chargeOperationDto = new ChargeOperationDtoBuilder().WithChargeType(ChargeType.Fee).Build();
            var chargeCommand = new ChargeCommandBuilder().WithChargeOperation(chargeOperationDto).Build();
            chargeRepository
                .Setup(r => r.GetOrNullAsync(It.IsAny<ChargeIdentifier>()))
                .ReturnsAsync(charge);
            chargeRepository
                .Setup(r => r.GetAsync(It.IsAny<ChargeIdentifier>()))
                .Returns(Task.FromResult(charge));
            marketParticipantRepository
                .Setup(repo => repo.GetOrNullAsync(It.IsAny<string>()))
                .ReturnsAsync(sender);

            // Act
            var actual = await sut.CreateRulesAsync(chargeCommand).ConfigureAwait(false);
            var actualRules = actual.GetRules().Select(r => r.GetType()).ToList();

            // Assert
            Assert.Equal(5, actual.GetRules().Count); // This assert is added to ensure that when the rule set is expanded, the test gets attention as well.
            Assert.Contains(typeof(CommandSenderMustBeAnExistingMarketParticipantRule), actualRules);
            Assert.Contains(typeof(BusinessReasonCodeMustBeUpdateChargeInformationOrChargePricesRule), actualRules);
            Assert.Contains(typeof(DocumentTypeMustBeRequestChangeOfPriceListRule), actualRules);
            Assert.Contains(typeof(RecipientIsMandatoryTypeValidationRule), actualRules);
            Assert.Contains(typeof(SenderIsMandatoryTypeValidationRule), actualRules);
        }
    }
}
