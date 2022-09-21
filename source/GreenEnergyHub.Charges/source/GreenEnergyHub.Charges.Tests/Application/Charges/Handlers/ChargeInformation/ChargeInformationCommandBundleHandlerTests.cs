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
using GreenEnergyHub.Charges.Application.Charges.Handlers.ChargeInformation;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeInformationCommands;
using GreenEnergyHub.Charges.Domain.Dtos.SharedDtos;
using GreenEnergyHub.Charges.TestCore.Builders.Application;
using GreenEnergyHub.Charges.TestCore.Builders.Command;
using GreenEnergyHub.TestHelpers;
using Moq;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Application.Charges.Handlers.ChargeInformation
{
    [UnitTest]
    public class ChargeInformationCommandBundleHandlerTests
    {
        [Theory]
        [InlineAutoDomainData]
        public async Task HandleAsync_WhenCalledWithMultipleChargeCommands_ShouldCallMultipleTimes(
            [Frozen] Mock<IChargeInformationCommandHandler> chargeCommandBundleHandler,
            ChargeInformationCommandBundleHandler sut)
        {
            // Arrange
            var document = new DocumentDtoBuilder()
                .WithBusinessReasonCode(BusinessReasonCode.UpdateChargeInformation)
                .Build();
            var commandBuilder = new ChargeInformationCommandBuilder();
            var bundle = new ChargeInformationCommandBundleBuilder()
                .WithDocument(document)
                .WithCommand(commandBuilder.Build())
                .WithCommand(commandBuilder.Build())
                .WithCommand(commandBuilder.Build())
                .Build();

            // Act
            await sut.HandleAsync(bundle).ConfigureAwait(false);

            // Assert
            chargeCommandBundleHandler
                .Verify(v => v.HandleAsync(It.IsAny<ChargeInformationCommand>()), Times.Exactly(3));
        }
    }
}
