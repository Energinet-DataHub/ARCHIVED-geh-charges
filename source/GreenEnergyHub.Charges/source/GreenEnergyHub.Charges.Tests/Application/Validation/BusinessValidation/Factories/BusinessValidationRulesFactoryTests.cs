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
using System.Threading.Tasks;
using AutoFixture.Xunit2;
using GreenEnergyHub.Charges.Application.Validation;
using GreenEnergyHub.Charges.Application.Validation.BusinessValidation.Factories;
using GreenEnergyHub.Charges.Domain.ChangeOfCharges.Transaction;
using GreenEnergyHub.Charges.TestCore;
using Moq;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Application.Validation.BusinessValidation.Factories
{
    [UnitTest]
    public class BusinessValidationRulesFactoryTests
    {
        [Theory]
        [InlineAutoMoqData(OperationType.Create, true, false, false)]
        [InlineAutoMoqData(OperationType.Update, false, true, false)]
        [InlineAutoMoqData(OperationType.Stop, false, false, true)]
        public async Task CreateRulesForChargeCommandAsync_WhenCalledWithCommand_UsesOnlyCorrectFactoryAndReturnsResult(
            OperationType operationType,
            bool expectCreateFactoryUsed,
            bool expectUpdateFactoryUsed,
            bool expectStopFactoryUsed,
            [Frozen] [NotNull] Mock<IBusinessCreateValidationRulesFactory> createFactory,
            [Frozen] [NotNull] Mock<IBusinessUpdateValidationRulesFactory> updateFactory,
            [Frozen] [NotNull] Mock<IBusinessStopValidationRulesFactory> stopFactory,
            [NotNull] IValidationRuleSet validationRules,
            [NotNull] ChargeCommand command,
            [NotNull] BusinessValidationRulesFactory sut)
        {
            // Arrange
            command.ChargeOperation.OperationType = operationType;

            var createUsed = false;
            createFactory.Setup(
                    c => c.CreateRulesForCreateCommandAsync(command))
                .Returns(Task.FromResult(validationRules))
                .Callback<ChargeCommand>(_ => createUsed = true);

            var updateUsed = false;
            updateFactory.Setup(
                    u => u.CreateRulesForUpdateCommandAsync(command))
                .Returns(Task.FromResult(validationRules))
                .Callback<ChargeCommand>(_ => updateUsed = true);

            var stopUsed = false;
            stopFactory.Setup(
                    s => s.CreateRulesForStopCommandAsync(command))
                .Returns(Task.FromResult(validationRules))
                .Callback<ChargeCommand>(_ => stopUsed = true);

            // Act
            var result = await sut.CreateRulesForChargeCommandAsync(command).ConfigureAwait(false);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(validationRules, result);
            Assert.Equal(expectCreateFactoryUsed, createUsed);
            Assert.Equal(expectUpdateFactoryUsed, updateUsed);
            Assert.Equal(expectStopFactoryUsed, stopUsed);
        }

        [Theory]
        [InlineAutoMoqData]
        public static async Task CreateRulesForChargeCommandAsync_WhenCalledWithNull_ThrowsArgumentNullException(
            [NotNull] BusinessValidationRulesFactory sut)
        {
            // Arrange
            ChargeCommand? command = null;

            // Act / Assert
            await Assert.ThrowsAsync<ArgumentNullException>(
                () => sut.CreateRulesForChargeCommandAsync(command!))
                .ConfigureAwait(false);
        }

        [Theory]
        [InlineAutoMoqData]
        public static async Task CreateRulesForChargeCommandAsync_WhenCalledWithUnknownOperationType_ThrowsNotImplementedException(
            [NotNull] ChargeCommand command,
            [NotNull] BusinessValidationRulesFactory sut)
        {
            // Arrange
            command.ChargeOperation.OperationType = OperationType.Unknown;

            // Act / Assert
            await Assert.ThrowsAsync<NotImplementedException>(
                    () => sut.CreateRulesForChargeCommandAsync(command!))
                .ConfigureAwait(false);
        }
    }
}
