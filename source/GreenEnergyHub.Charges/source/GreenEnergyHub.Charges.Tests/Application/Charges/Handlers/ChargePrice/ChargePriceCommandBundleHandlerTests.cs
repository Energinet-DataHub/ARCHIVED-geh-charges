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
using GreenEnergyHub.Charges.Application.Charges.Handlers;
using GreenEnergyHub.Charges.Domain.Dtos.ChargePriceCommands;
using GreenEnergyHub.Charges.Domain.Dtos.SharedDtos;
using GreenEnergyHub.Charges.Tests.Builders.Application;
using GreenEnergyHub.Charges.Tests.Builders.Command;
using GreenEnergyHub.TestHelpers;
using Moq;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Application.Charges.Handlers.ChargePrice
{
    [UnitTest]
    public class ChargePriceCommandBundleHandlerTests
    {
        [Theory]
        [InlineAutoDomainData]
        public async Task HandleAsync_WhenCalledWithMultipleChargeCommands_ShouldCallMultipleTimes(
            [Frozen] Mock<IChargePriceCommandHandler> chargePriceCommandBundleHandler,
            ChargePriceCommandBundleHandler sut)
        {
            // Arrange
            var document = new DocumentDtoBuilder()
                .WithBusinessReasonCode(BusinessReasonCode.UpdateChargePrices)
                .Build();
            var commandBuilder = new ChargePriceCommandBuilder();
            var bundle = new ChargePriceCommandBundleBuilder()
                .WithDocument(document)
                .WithCommand(commandBuilder.Build())
                .WithCommand(commandBuilder.Build())
                .WithCommand(commandBuilder.Build())
                .Build();

            // Act
            await sut.HandleAsync(bundle).ConfigureAwait(false);

            // Assert
            chargePriceCommandBundleHandler
                .Verify(v => v.HandleAsync(It.IsAny<ChargePriceCommand>()), Times.Exactly(3));
        }
    }
}
