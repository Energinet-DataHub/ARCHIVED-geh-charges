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

using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using AutoFixture.Xunit2;
using GreenEnergyHub.Charges.Application.Charges.Handlers;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommands;
using GreenEnergyHub.Charges.Tests.Builders;
using GreenEnergyHub.TestHelpers;
using Moq;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Application.Charges.Handlers
{
    [UnitTest]
    public class ChargesMessageHandlerTests
    {
        [Theory]
        [InlineAutoDomainData]
        public async Task HandleAsync_WhenCalledWithMultipleTransactions_ShouldCallMultipleTimes(
            [NotNull] [Frozen] Mock<IChargeCommandHandler> changeOfChargesTransactionHandler,
            [NotNull] ChargesMessageHandler sut)
        {
            // Arrange
            var transactionBuilder = new ChargeCommandBuilder();
            var changeOfChargesMessage = new ChargesMessageBuilder()
                .WithTransaction(transactionBuilder.Build())
                .WithTransaction(transactionBuilder.Build())
                .WithTransaction(transactionBuilder.Build())
                .Build();

            // Act
            await sut.HandleAsync(changeOfChargesMessage).ConfigureAwait(false);

            // Assert
            changeOfChargesTransactionHandler
                .Verify(v => v.HandleAsync(It.IsAny<ChargeCommand>()), Times.Exactly(3));
        }
    }
}
