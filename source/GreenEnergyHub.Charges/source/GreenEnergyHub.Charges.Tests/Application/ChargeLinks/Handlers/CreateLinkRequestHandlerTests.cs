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
using System.Threading;
using System.Threading.Tasks;
using AutoFixture.Xunit2;
using Energinet.DataHub.Core.App.FunctionApp.Middleware.CorrelationId;
using GreenEnergyHub.Charges.Application.ChargeLinks.CreateDefaultChargeLinkReplier;
using GreenEnergyHub.Charges.Application.ChargeLinks.Handlers;
using GreenEnergyHub.Charges.Contracts;
using GreenEnergyHub.Charges.Domain.DefaultChargeLinks;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeLinksCommands;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeLinksReceivedEvents;
using GreenEnergyHub.Charges.Domain.Dtos.CreateDefaultChargeLinksRequests;
using GreenEnergyHub.Charges.Domain.MeteringPoints;
using GreenEnergyHub.Charges.MessageHub.AvailableData.Messaging;
using GreenEnergyHub.TestHelpers;
using Moq;
using NodaTime;
using NodaTime.Text;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Application.ChargeLinks.Handlers
{
    [UnitTest]
    public class CreateLinkRequestHandlerTests
    {
        [Theory]
        [InlineAutoDomainData]
        public async Task HandleAsync_WhenCalled_UsesFactoryToCreateEventAndDispatchesIt(
            [Frozen] Mock<IDefaultChargeLinkRepository> defaultChargeLinkRepository,
            [Frozen] Mock<IMeteringPointRepository> meteringPointRepository,
            [Frozen] Mock<IChargeLinksCommandFactory> chargeLinkCommandFactory,
            [Frozen] Mock<IMessageDispatcher<ChargeLinksReceivedEvent>> dispatcher,
            [Frozen] Mock<IMessageMetaDataContext> messageMetaDataContext,
            Guid defaultChargeLinkId,
            string replyTo,
            ChargeLinksCommand chargeLinksCommand,
            string meteringPointId,
            CreateLinkRequestHandler sut)
        {
            // Arrange
            foreach (var chargeLinkDto in chargeLinksCommand.Operations)
            {
                chargeLinkDto.EndDateTime = null;
            }

            messageMetaDataContext.Setup(m => m.IsReplyToSet()).Returns(true);
            messageMetaDataContext.Setup(m => m.ReplyTo).Returns(replyTo);
            var createLinkCommandEvent = new CreateDefaultChargeLinksRequest(meteringPointId);

            var defaultChargeLink = new DefaultChargeLink(
                defaultChargeLinkId,
                SystemClock.Instance.GetCurrentInstant(),
                InstantPattern.General.Parse("9999-12-31T23:59:59Z").Value,
                Guid.NewGuid(),
                MeteringPointType.Consumption);

            var defaultChargeLinks = new List<DefaultChargeLink> { defaultChargeLink };
            defaultChargeLinkRepository
                .Setup(f => f.GetAsync(It.IsAny<MeteringPointType>()))
                .ReturnsAsync(defaultChargeLinks);

            meteringPointRepository
                .Setup(f => f.GetOrNullAsync(It.IsAny<string>()))
                .ReturnsAsync(MeteringPoint.Create(
                    meteringPointId,
                    MeteringPointType.Consumption,
                    Guid.NewGuid(),
                    SystemClock.Instance.GetCurrentInstant(),
                    ConnectionState.New,
                    null));

            chargeLinkCommandFactory
                .Setup(f => f.CreateAsync(createLinkCommandEvent, defaultChargeLinks))
                .ReturnsAsync(chargeLinksCommand);

            // Act
            await sut.HandleAsync(createLinkCommandEvent).ConfigureAwait(false);

            // Assert
            dispatcher.Verify(
                x => x.DispatchAsync(
                    It.IsAny<ChargeLinksReceivedEvent>(),
                    It.IsAny<CancellationToken>()));
        }

        [Theory]
        [InlineAutoDomainData]
        public async Task HandleAsync_WhenCalledWithReplyBeingNull_ThrowsArgumentException(
            [Frozen] Mock<IMessageMetaDataContext> messageMetaDataContext,
            ChargeLinksCommand chargeLinksCommand,
            string meteringPointId,
            CreateLinkRequestHandler sut)
        {
            // Arrange
            foreach (var chargeLinkDto in chargeLinksCommand.Operations)
            {
                chargeLinkDto.EndDateTime = null;
            }

            messageMetaDataContext.Setup(m => m.ReplyTo).Returns((string)null!);
            var createLinkCommandEvent = new CreateDefaultChargeLinksRequest(meteringPointId);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => sut.HandleAsync(createLinkCommandEvent));
        }

        [Theory]
        [InlineAutoDomainData]
        public async Task HandleAsync_WithUnknownMeteringPointId_CallDefaultLinkClientWithFailedReply(
            [Frozen] Mock<ICorrelationContext> correlationContextMock,
            [Frozen] Mock<IMeteringPointRepository> meteringPointRepository,
            [Frozen] Mock<IMessageDispatcher<ChargeLinksReceivedEvent>> dispatcher,
            [Frozen] Mock<IMessageMetaDataContext> messageMetaDataContext,
            [Frozen] Mock<ICreateDefaultChargeLinksReplier> defaultChargeLinkClient,
            string correlationId,
            string replyTo,
            ChargeLinksCommand chargeLinksCommand,
            string meteringPointId,
            ErrorCode errorCode,
            CreateLinkRequestHandler sut)
        {
            // Arrange
            foreach (var chargeLinkDto in chargeLinksCommand.Operations)
            {
                chargeLinkDto.EndDateTime = null;
            }

            correlationContextMock.Setup(c => c.Id).Returns(correlationId);
            messageMetaDataContext.Setup(m => m.IsReplyToSet()).Returns(true);
            messageMetaDataContext.Setup(m => m.ReplyTo).Returns(replyTo);
            errorCode = ErrorCode.MeteringPointUnknown;
            var createLinkCommandEvent = new CreateDefaultChargeLinksRequest(meteringPointId);

            defaultChargeLinkClient.Setup(d => d.ReplyWithFailedAsync(meteringPointId, errorCode, replyTo));
            meteringPointRepository
                .Setup(f => f.GetOrNullAsync(It.IsAny<string>()))
                .ReturnsAsync((MeteringPoint?)null);

            // Act
            await sut.HandleAsync(createLinkCommandEvent).ConfigureAwait(false);

            // Assert
            defaultChargeLinkClient.Verify(x => x.ReplyWithFailedAsync(meteringPointId, errorCode, replyTo));

            dispatcher.Verify(
                x => x.DispatchAsync(
                    It.IsAny<ChargeLinksReceivedEvent>(),
                    It.IsAny<CancellationToken>()),
                Times.Never());
        }

        [Theory]
        [InlineAutoDomainData]
        public async Task HandleAsync_WhenCalledWithMeteringPointTypeWhichHasNoDefaultLinks_ReplyWithDefaultChargeLinksSucceeded(
            [Frozen] Mock<ICorrelationContext> correlationContextMock,
            [Frozen] Mock<IDefaultChargeLinkRepository> defaultChargeLinkRepository,
            [Frozen] Mock<IMeteringPointRepository> meteringPointRepository,
            [Frozen] Mock<IMessageDispatcher<ChargeLinksReceivedEvent>> dispatcher,
            [Frozen] Mock<IMessageMetaDataContext> messageMetaDataContext,
            [Frozen] Mock<ICreateDefaultChargeLinksReplier> defaultChargeLinkClient,
            string correlationId,
            string replyTo,
            ChargeLinksCommand chargeLinksCommand,
            string meteringPointId,
            CreateLinkRequestHandler sut)
        {
            // Arrange
            foreach (var chargeLinkDto in chargeLinksCommand.Operations)
            {
                chargeLinkDto.EndDateTime = null;
            }

            correlationContextMock.Setup(c => c.Id).Returns(correlationId);
            messageMetaDataContext.Setup(m => m.IsReplyToSet()).Returns(true);
            messageMetaDataContext.Setup(m => m.ReplyTo).Returns(replyTo);
            var createLinkCommandEvent = new CreateDefaultChargeLinksRequest(meteringPointId);

            defaultChargeLinkClient.Setup(d =>
                d.ReplyWithSucceededAsync(meteringPointId, true, replyTo));

            meteringPointRepository
                .Setup(f => f.GetOrNullAsync(It.IsAny<string>()))
                .ReturnsAsync(MeteringPoint.Create(
                    meteringPointId,
                    MeteringPointType.Consumption,
                    Guid.NewGuid(),
                    SystemClock.Instance.GetCurrentInstant(),
                    ConnectionState.New,
                    null));

            defaultChargeLinkRepository
                .Setup(f => f.GetAsync(It.IsAny<MeteringPointType>()))
                .ReturnsAsync(new List<DefaultChargeLink>());

            // Act
            await sut.HandleAsync(createLinkCommandEvent).ConfigureAwait(false);

            // Assert
            defaultChargeLinkClient.Verify(x => x.ReplyWithSucceededAsync(meteringPointId, false, replyTo));

            dispatcher.Verify(
                x => x.DispatchAsync(
                    It.IsAny<ChargeLinksReceivedEvent>(),
                    It.IsAny<CancellationToken>()),
                Times.Never());
        }
    }
}
