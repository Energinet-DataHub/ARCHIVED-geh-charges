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

using System.Threading;
using System.Threading.Tasks;
using AutoFixture.Xunit2;
using GreenEnergyHub.Charges.Application.Messaging;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommandReceivedEvents;
using GreenEnergyHub.Charges.Domain.MarketParticipants;
using GreenEnergyHub.Charges.Tests.Builders.Command;
using GreenEnergyHub.TestHelpers;
using Moq;
using Xunit;
using Xunit.Categories;
using ChargeCommandHandler = GreenEnergyHub.Charges.Application.Charges.Handlers.ChargeCommandHandler;

namespace GreenEnergyHub.Charges.Tests.Application.Charges.Handlers
{
    [UnitTest]
    public class ChargeCommandHandlerTests
    {
        [Theory]
        [InlineAutoDomainData]
        public async Task HandleAsync_WhenCalledWithChargeCommand_ShouldDispatchReceivedEvent(
            [Frozen] Mock<IMessageDispatcher<ChargeCommandReceivedEvent>> chargeEventPublisher,
            ChargeCommandHandler sut)
        {
            // Arrange
            var document = new DocumentDtoBuilder()
                .WithBusinessReasonCode(BusinessReasonCode.UpdateChargeInformation)
                .Build();
            var transaction = new ChargeCommandBuilder().WithDocumentDto(document).Build();

            // Act
            await sut.HandleAsync(transaction).ConfigureAwait(false);

            // Assert
            chargeEventPublisher.Verify(
                x => x.DispatchAsync(
                    It.Is<ChargeCommandReceivedEvent>(
                        localEvent => localEvent.Command == transaction),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
