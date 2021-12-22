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
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.AutoMoq;
using AutoFixture.Xunit2;
using GreenEnergyHub.Charges.Application.ChargeLinks.Acknowledgement;
using GreenEnergyHub.Charges.Application.ChargeLinks.Handlers;
using GreenEnergyHub.Charges.Domain.ChargeLinks;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommands.Validation;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeLinksAcceptedEvents;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeLinksCommands;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeLinksCommands.Validation;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeLinksReceivedEvents;
using GreenEnergyHub.TestHelpers;
using Moq;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Application.ChargeLinks.Handlers
{
    [UnitTest]
    public class ChargeLinksReceivedEventHandlerTests
    {
        [Theory]
        [InlineAutoDomainData]
        public async Task HandleAsync_ShouldDispatch_AcceptedEvent(
            [Frozen] Mock<IChargeLinksConfirmationService> chargeLinksConfirmationService,
            [Frozen] Mock<IChargeLinkFactory> chargeLinkFactory,
            [Frozen] Mock<IChargeLinksAcceptedEventFactory> chargeLinkCommandAcceptedEventFactory,
            [Frozen] Mock<IChargeLinksCommandValidator> chargeLinksCommandValidator,
            ChargeLinksReceivedEvent chargeLinksReceivedEvent,
            ChargeLinksAcceptedEvent chargeLinksAcceptedEvent,
            ChargeLinksReceivedEventHandler sut)
        {
            // Arrange
            var fixture = new Fixture().Customize(new AutoMoqCustomization());
            fixture.Customizations.Add(new StringGenerator(() => Guid.NewGuid().ToString()[..16]));
            var chargeLink = fixture.Create<ChargeLink>();

            chargeLinksCommandValidator.Setup(x => x.ValidateAsync(It.IsAny<ChargeLinksCommand>()))
                .ReturnsAsync(ValidationResult.CreateSuccess());

            chargeLinkFactory
                .Setup(x => x.CreateAsync(It.IsAny<ChargeLinksReceivedEvent>()))
                .ReturnsAsync(new List<ChargeLink> { chargeLink, chargeLink });

            chargeLinkCommandAcceptedEventFactory.Setup(
                    x => x.Create(
                        It.IsAny<ChargeLinksCommand>()))
                .Returns(chargeLinksAcceptedEvent);

            // Act
            await sut.HandleAsync(chargeLinksReceivedEvent).ConfigureAwait(false);

            // Assert
            chargeLinksConfirmationService.Verify(x => x.AcceptAsync(It.IsAny<ChargeLinksCommand>()));
        }
    }
}
