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
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.AutoMoq;
using AutoFixture.Xunit2;
using GreenEnergyHub.Charges.Application;
using GreenEnergyHub.Charges.Application.ChargeLinks.Handlers;
using GreenEnergyHub.Charges.Domain.ChargeLinks;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeLinkCommandAcceptedEvents;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeLinkCommandReceivedEvents;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeLinkCommands;
using GreenEnergyHub.TestHelpers;
using Moq;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Application.ChargeLinks.Handlers
{
    [UnitTest]
    public class ChargeLinkCommandReceivedHandlerTests
    {
        [Theory]
        [InlineAutoDomainData]
        public async Task HandleAsync_ShouldDispatch_AcceptedEvent(
            [NotNull] [Frozen] Mock<IMessageDispatcher<ChargeLinkCommandAcceptedEvent>> messageDispatcher,
            [NotNull] [Frozen] Mock<IChargeLinkFactory> chargeLinkFactory,
            [NotNull] [Frozen] Mock<IChargeLinkCommandAcceptedEventFactory> chargeLinkCommandAcceptedEventFactory,
            [NotNull] ChargeLinkCommandReceivedEvent chargeLinkCommandReceivedEvent,
            [NotNull] ChargeLinkCommandAcceptedEvent chargeLinkCommandAcceptedEvent,
            [NotNull] ChargeLinkCommandReceivedHandler sut)
        {
            // Arrange
            var fixture = new Fixture().Customize(new AutoMoqCustomization());
            fixture.Customizations.Add(new StringGenerator(() => Guid.NewGuid().ToString()[..16]));
            var chargeLink = fixture.Create<ChargeLink>();

            chargeLinkFactory
                .Setup(x => x.CreateAsync(It.IsAny<ChargeLinkCommandReceivedEvent>()))
                .ReturnsAsync(new List<ChargeLink> { chargeLink, chargeLink });

            chargeLinkCommandAcceptedEventFactory.Setup(
                    x => x.Create(
                        It.IsAny<IReadOnlyCollection<ChargeLinkCommand>>()))
                .Returns(chargeLinkCommandAcceptedEvent);

            // Act
            await sut.HandleAsync(chargeLinkCommandReceivedEvent).ConfigureAwait(false);

            // Assert
            messageDispatcher.Verify(
                x => x.DispatchAsync(chargeLinkCommandAcceptedEvent, It.IsAny<CancellationToken>()));
        }
    }
}
