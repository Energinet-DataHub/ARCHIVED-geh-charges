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
using System.Threading;
using System.Threading.Tasks;
using AutoFixture.Xunit2;
using FluentAssertions;
using GreenEnergyHub.Charges.Application;
using GreenEnergyHub.Charges.Application.ChargeLinks.Handlers;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeLinksAcceptedEvents;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeLinksDataAvailableNotifiedEvents;
using GreenEnergyHub.Charges.Infrastructure.Core.MessageMetaData;
using GreenEnergyHub.Charges.Infrastructure.Core.MessagingExtensions;
using GreenEnergyHub.Charges.TestCore.Attributes;
using Moq;
using NodaTime;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Application.ChargeLinks.Handlers
{
    [UnitTest]
    public class ChargeLinkDataAvailableReplyHandlerTests
    {
        [Theory]
        [InlineAutoMoqData]
        public async Task ReplyAsync_WhenEventIsNull_ThrowsArgumentNullException(
            ChargeLinksDataAvailableNotifiedPublisher sut)
        {
            await sut
                .Invoking(notifier => notifier.PublishAsync(null!))
                .Should()
                .ThrowExactlyAsync<ArgumentNullException>();
        }

        [Theory]
        [InlineAutoMoqData]
        public async Task ReplyAsync_WhenCalledAndReplyIsSetIsTrue_ShouldDispatchMessage(
            [Frozen] Mock<IMessageDispatcher<ChargeLinksDataAvailableNotifiedEvent>> messageDispatcher,
            [Frozen] Mock<IChargeLinksDataAvailableNotifiedEventFactory> defaultChargeLinksCreatedEventFactory,
            [Frozen] Mock<IMessageMetaDataContext> messageMetaDataContext,
            ChargeLinksAcceptedEvent chargeLinksAcceptedEvent,
            ChargeLinksDataAvailableNotifiedPublisher sut)
        {
            // Arrange
            messageMetaDataContext.Setup(context => context.IsReplyToSet()).Returns(true);
            defaultChargeLinksCreatedEventFactory
                .Setup(x => x.Create(chargeLinksAcceptedEvent))
                .Returns(GetDefaultChargeLinksCreatedEvent);

            // Act
            await sut.PublishAsync(chargeLinksAcceptedEvent);

            // Assert
            defaultChargeLinksCreatedEventFactory.Verify(
                x => x.Create(chargeLinksAcceptedEvent), Times.Once);
            messageDispatcher.Verify(
                expression: x =>
                    x.DispatchAsync(It.IsAny<ChargeLinksDataAvailableNotifiedEvent>(), CancellationToken.None),
                Times.Once);
        }

        [Theory]
        [InlineAutoMoqData]
        public async Task ReplyAsync_WhenCalledAndReplyIsSetIsFalse_ShouldNotDispatchMessage(
            [Frozen] Mock<IMessageDispatcher<ChargeLinksDataAvailableNotifiedEvent>> messageDispatcher,
            [Frozen] Mock<IChargeLinksDataAvailableNotifiedEventFactory> defaultChargeLinksCreatedEventFactory,
            [Frozen] Mock<IMessageMetaDataContext> messageMetaDataContext,
            ChargeLinksAcceptedEvent chargeLinksAcceptedEvent,
            ChargeLinksDataAvailableNotifiedPublisher sut)
        {
            // Arrange
            messageMetaDataContext.Setup(context => context.IsReplyToSet()).Returns(false);

            // Act
            await sut.PublishAsync(chargeLinksAcceptedEvent);

            // Assert
            defaultChargeLinksCreatedEventFactory.Verify(
                x => x.Create(chargeLinksAcceptedEvent), Times.Never);
            messageDispatcher.Verify(
                expression: x =>
                    x.DispatchAsync(It.IsAny<ChargeLinksDataAvailableNotifiedEvent>(), CancellationToken.None),
                Times.Never);
        }

        private ChargeLinksDataAvailableNotifiedEvent GetDefaultChargeLinksCreatedEvent()
        {
            return new ChargeLinksDataAvailableNotifiedEvent(SystemClock.Instance.GetCurrentInstant(), "12345678910");
        }
    }
}
