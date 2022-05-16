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

using System.Threading.Tasks;
using AutoFixture.Xunit2;
using GreenEnergyHub.Charges.Application.ChargeCommands.Handlers;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommands;
using GreenEnergyHub.Charges.Tests.Builders.Application;
using GreenEnergyHub.Charges.Tests.Builders.Command;
using GreenEnergyHub.TestHelpers;
using Moq;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Application.ChargesCommand.Handlers
{
    [UnitTest]
    public class ChargeCommandBundleHandlerTests
    {
        [Theory]
        [InlineAutoDomainData]
        public async Task HandleAsync_WhenCalledWithMultipleChargeCommands_ShouldCallMultipleTimes(
            [Frozen] Mock<IChargeCommandHandler> chargeCommandBundleHandler,
            ChargeCommandBundleHandler sut)
        {
            // Arrange
            var commandBuilder = new ChargeCommandBuilder();
            var bundle = new ChargeCommandBundleBuilder()
                .WithChargeCommand(commandBuilder.Build())
                .WithChargeCommand(commandBuilder.Build())
                .WithChargeCommand(commandBuilder.Build())
                .Build();

            // Act
            await sut.HandleAsync(bundle).ConfigureAwait(false);

            // Assert
            chargeCommandBundleHandler
                .Verify(v => v.HandleAsync(It.IsAny<ChargeCommand>()), Times.Exactly(3));
        }
    }
}
