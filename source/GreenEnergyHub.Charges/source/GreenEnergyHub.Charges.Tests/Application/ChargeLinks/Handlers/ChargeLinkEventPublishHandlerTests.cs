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
using System.Threading;
using System.Threading.Tasks;
using AutoFixture.Xunit2;
using FluentAssertions;
using GreenEnergyHub.Charges.Application.ChargeLinks.Handlers;
using GreenEnergyHub.Charges.Application.Messaging;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeLinkCreatedEvents;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeLinksAcceptedEvents;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeLinksCommands;
using GreenEnergyHub.TestHelpers;
using Moq;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Application.ChargeLinks.Handlers
{
    [UnitTest]
    public class ChargeLinkEventPublishHandlerTests
    {
        [Theory]
        [InlineAutoDomainData]
        public async Task HandleAsync_WhenCalled_UsesFactoryToCreateEventAndDispatchesIt(
            [Frozen] Mock<IChargeLinkCreatedEventFactory> factory,
            [Frozen] Mock<IMessageDispatcher<ChargeLinkCreatedEvent>> dispatcher,
            ChargeLinksAcceptedEvent command,
            IReadOnlyCollection<ChargeLinkCreatedEvent> createdEvents,
            ChargeLinkEventPublishHandler sut)
        {
            // Arrange
            factory.Setup(
                    f => f.CreateEvents(
                        It.IsAny<ChargeLinksCommand>()))
                .Returns(createdEvents);

            var dispatched = false;
            foreach (var chargeLinkCreatedEvent in createdEvents)
            {
                dispatcher.Setup(
                        d => d.DispatchAsync(
                            chargeLinkCreatedEvent,
                            It.IsAny<CancellationToken>()))
                    .Callback<ChargeLinkCreatedEvent, CancellationToken>((_, _) => dispatched = true);
            }

            // Act
            await sut.HandleAsync(command).ConfigureAwait(false);

            // Assert
            dispatched.Should().BeTrue();
        }
    }
}
