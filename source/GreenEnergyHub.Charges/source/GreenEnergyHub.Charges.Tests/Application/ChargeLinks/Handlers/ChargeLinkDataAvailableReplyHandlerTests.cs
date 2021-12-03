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
using GreenEnergyHub.Charges.Domain.Dtos.DefaultChargeLinksDataAvailableNotifiedEvents;
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
            ChargeLinkDataAvailableReplyHandler sut)
        {
            await sut
                .Invoking(notifier => notifier.ReplyAsync(null!))
                .Should()
                .ThrowExactlyAsync<ArgumentNullException>();
        }

        [Theory]
        [InlineAutoMoqData]
        public async Task ReplyAsync_WhenCalledAndReplyIsSetIsTrue_ShouldDispatchMessage(
            [Frozen] Mock<IMessageDispatcher<DefaultChargeLinksCreatedEvent>> messageDispatcher,
            [Frozen] Mock<IDefaultChargeLinksCreatedEventFactory> defaultChargeLinksCreatedEventFactory,
            [Frozen] Mock<IMessageMetaDataContext> messageMetaDataContext,
            ChargeLinksAcceptedEvent chargeLinksAcceptedEvent,
            ChargeLinkDataAvailableReplyHandler sut)
        {
            // Arrange
            messageMetaDataContext.Setup(context => context.IsReplyToSet()).Returns(true);
            defaultChargeLinksCreatedEventFactory
                .Setup(x => x.Create(chargeLinksAcceptedEvent))
                .Returns(GetDefaultChargeLinksCreatedEvent);

            // Act
            await sut.ReplyAsync(chargeLinksAcceptedEvent);

            // Assert
            defaultChargeLinksCreatedEventFactory.Verify(
                x => x.Create(chargeLinksAcceptedEvent), Times.Once);
            messageDispatcher.Verify(
                expression: x =>
                    x.DispatchAsync(It.IsAny<DefaultChargeLinksCreatedEvent>(), CancellationToken.None),
                Times.Once);
        }

        [Theory]
        [InlineAutoMoqData]
        public async Task ReplyAsync_WhenCalledAndReplyIsSetIsFalse_ShouldNotDispatchMessage(
            [Frozen] Mock<IMessageDispatcher<DefaultChargeLinksCreatedEvent>> messageDispatcher,
            [Frozen] Mock<IDefaultChargeLinksCreatedEventFactory> defaultChargeLinksCreatedEventFactory,
            [Frozen] Mock<IMessageMetaDataContext> messageMetaDataContext,
            ChargeLinksAcceptedEvent chargeLinksAcceptedEvent,
            ChargeLinkDataAvailableReplyHandler sut)
        {
            // Arrange
            messageMetaDataContext.Setup(context => context.IsReplyToSet()).Returns(false);

            // Act
            await sut.ReplyAsync(chargeLinksAcceptedEvent);

            // Assert
            defaultChargeLinksCreatedEventFactory.Verify(
                x => x.Create(chargeLinksAcceptedEvent), Times.Never);
            messageDispatcher.Verify(
                expression: x =>
                    x.DispatchAsync(It.IsAny<DefaultChargeLinksCreatedEvent>(), CancellationToken.None),
                Times.Never);
        }

        private DefaultChargeLinksCreatedEvent GetDefaultChargeLinksCreatedEvent()
        {
            return new DefaultChargeLinksCreatedEvent(SystemClock.Instance.GetCurrentInstant(), "12345678910");
        }
    }
}
