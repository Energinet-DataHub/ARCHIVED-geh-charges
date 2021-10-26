﻿// Copyright 2020 Energinet DataHub A/S
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
using AutoFixture.Xunit2;
using Energinet.DataHub.Charges.Libraries.DefaultChargeLink;
using Energinet.DataHub.Charges.Libraries.Models;
using GreenEnergyHub.Charges.Application;
using GreenEnergyHub.Charges.Application.ChargeLinks.Handlers;
using GreenEnergyHub.Charges.Domain.ChargeLinkCommandReceivedEvents;
using GreenEnergyHub.Charges.Domain.ChargeLinkCommands;
using GreenEnergyHub.Charges.Domain.CreateLinkCommandEvents;
using GreenEnergyHub.Charges.Domain.DefaultChargeLinks;
using GreenEnergyHub.Charges.Domain.MeteringPoints;
using GreenEnergyHub.TestHelpers;
using Moq;
using NodaTime;
using NodaTime.Text;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Application.ChargeLinks.Handlers
{
    [UnitTest]
    public class CreateLinkCommandEventHandlerTests
    {
        [Theory]
        [InlineAutoDomainData]
        public async Task HandleAsync_WhenCalled_UsesFactoryToCreateEventAndDispatchesIt(
            [Frozen] [NotNull] Mock<IDefaultChargeLinkRepository> defaultChargeLinkRepository,
            [Frozen] [NotNull] Mock<IMeteringPointRepository> meteringPointRepository,
            [Frozen] [NotNull] Mock<IChargeLinkCommandFactory> chargeLinkCommandFactory,
            [Frozen] [NotNull] Mock<IMessageDispatcher<ChargeLinkCommandReceivedEvent>> dispatcher,
            [Frozen] [NotNull] Mock<IMessageMetaDataContext> messageMetaDataContext,
            [NotNull] string correlationId,
            [NotNull] string replyTo,
            [NotNull] ChargeLinkCommand chargeLinkCommand,
            [NotNull] string meteringPointId,
            [NotNull] CreateLinkCommandEventHandler sut)
        {
            // Arrange
            chargeLinkCommand.ChargeLink.EndDateTime = null;
            messageMetaDataContext.Setup(m => m.IsReplyToSet()).Returns(true);
            messageMetaDataContext.Setup(m => m.ReplyTo).Returns(replyTo);
            var createLinkCommandEvent = new CreateLinkCommandEvent(meteringPointId);

            var defaultChargeLink = new DefaultChargeLink(
                SystemClock.Instance.GetCurrentInstant(),
                InstantPattern.General.Parse("9999-12-31T23:59:59Z").Value,
                Guid.NewGuid(),
                MeteringPointType.Consumption);

            defaultChargeLinkRepository.Setup(
                    f => f.GetAsync(
                        It.IsAny<MeteringPointType>()))
                .ReturnsAsync(new List<DefaultChargeLink> { defaultChargeLink });

            meteringPointRepository.Setup(
                    f => f.GetOrNullAsync(
                        It.IsAny<string>()))
                .ReturnsAsync(MeteringPoint.Create(
                    meteringPointId,
                    MeteringPointType.Consumption,
                    "gridArea",
                    SystemClock.Instance.GetCurrentInstant(),
                    ConnectionState.New,
                    null));

            chargeLinkCommandFactory.Setup(
                    f => f.CreateAsync(
                        createLinkCommandEvent,
                        defaultChargeLink,
                        correlationId))
                .ReturnsAsync(chargeLinkCommand);

            // Act
            await sut.HandleAsync(createLinkCommandEvent, correlationId).ConfigureAwait(false);

            // Assert
            dispatcher.Verify(
                x => x.DispatchAsync(
                    It.IsAny<ChargeLinkCommandReceivedEvent>(),
                    It.IsAny<CancellationToken>()));
        }

        [Theory]
        [InlineAutoDomainData]
        public async Task HandleAsync_WhenCalledWithReplyBeingNull_ThrowsArgumentException(
            [Frozen] [NotNull] Mock<IMessageMetaDataContext> messageMetaDataContext,
            [NotNull] string correlationId,
            [NotNull] ChargeLinkCommand chargeLinkCommand,
            [NotNull] string meteringPointId,
            [NotNull] CreateLinkCommandEventHandler sut)
        {
            // Arrange
            chargeLinkCommand.ChargeLink.EndDateTime = null;
            messageMetaDataContext.Setup(m => m.ReplyTo).Returns((string)null!);
            var createLinkCommandEvent = new CreateLinkCommandEvent(meteringPointId);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                sut.HandleAsync(createLinkCommandEvent, correlationId));
        }

        [Theory]
        [InlineAutoDomainData]
        public async Task HandleAsync_WithUnknownMeteringPointId_CallDefaultLinkClientWithFailedDto(
            [Frozen] [NotNull] Mock<IMeteringPointRepository> meteringPointRepository,
            [Frozen] [NotNull] Mock<IMessageDispatcher<ChargeLinkCommandReceivedEvent>> dispatcher,
            [Frozen] [NotNull] Mock<IMessageMetaDataContext> messageMetaDataContext,
            [Frozen] [NotNull] Mock<IDefaultChargeLinkClient> defaultChargeLinkClient,
            [NotNull] string correlationId,
            [NotNull] string replyTo,
            [NotNull] ChargeLinkCommand chargeLinkCommand,
            [NotNull] CreateDefaultChargeLinksFailedDto createDefaultChargeLinksFailedDto,
            [NotNull] string meteringPointId,
            [NotNull] CreateLinkCommandEventHandler sut)
        {
            // Arrange
            chargeLinkCommand.ChargeLink.EndDateTime = null;
            messageMetaDataContext.Setup(m => m.IsReplyToSet()).Returns(true);
            messageMetaDataContext.Setup(m => m.ReplyTo).Returns(replyTo);
            var createLinkCommandEvent = new CreateLinkCommandEvent(meteringPointId);

            defaultChargeLinkClient.Setup(d =>
                d.CreateDefaultChargeLinksFailedReplyAsync(
                    createDefaultChargeLinksFailedDto,
                    correlationId,
                    replyTo));

            meteringPointRepository.Setup(
                    f => f.GetOrNullAsync(
                        It.IsAny<string>()))
                .ReturnsAsync((MeteringPoint?)null);

            // Act
            await sut.HandleAsync(createLinkCommandEvent, correlationId).ConfigureAwait(false);

            // Assert
            defaultChargeLinkClient.Verify(
                x => x.CreateDefaultChargeLinksFailedReplyAsync(
                    It.IsAny<CreateDefaultChargeLinksFailedDto>(),
                    correlationId,
                    replyTo));

            dispatcher.Verify(
                x => x.DispatchAsync(
                    It.IsAny<ChargeLinkCommandReceivedEvent>(),
                    It.IsAny<CancellationToken>()),
                Times.Never());
        }

        [Theory]
        [InlineAutoDomainData]
        public async Task HandleAsync_WhenCalledWithMeteringPointTypeWhichHasNoDefaultLinks_ReplyWithDefaultChargeLinksSucceededDto(
            [Frozen] [NotNull] Mock<IDefaultChargeLinkRepository> defaultChargeLinkRepository,
            [Frozen] [NotNull] Mock<IMeteringPointRepository> meteringPointRepository,
            [Frozen] [NotNull] Mock<IMessageDispatcher<ChargeLinkCommandReceivedEvent>> dispatcher,
            [Frozen] [NotNull] Mock<IMessageMetaDataContext> messageMetaDataContext,
            [Frozen] [NotNull] Mock<IDefaultChargeLinkClient> defaultChargeLinkClient,
            [NotNull] CreateDefaultChargeLinksSucceededDto createDefaultChargeLinksSucceededDto,
            [NotNull] string correlationId,
            [NotNull] string replyTo,
            [NotNull] ChargeLinkCommand chargeLinkCommand,
            [NotNull] string meteringPointId,
            [NotNull] CreateLinkCommandEventHandler sut)
        {
            // Arrange
            chargeLinkCommand.ChargeLink.EndDateTime = null;
            messageMetaDataContext.Setup(m => m.IsReplyToSet()).Returns(true);
            messageMetaDataContext.Setup(m => m.ReplyTo).Returns(replyTo);
            var createLinkCommandEvent = new CreateLinkCommandEvent(meteringPointId);

            defaultChargeLinkClient.Setup(d =>
                d.CreateDefaultChargeLinksSucceededReplyAsync(
                    createDefaultChargeLinksSucceededDto,
                    correlationId,
                    replyTo));

            meteringPointRepository.Setup(
                    f => f.GetOrNullAsync(
                        It.IsAny<string>()))
                .ReturnsAsync(MeteringPoint.Create(
                    meteringPointId,
                    MeteringPointType.Consumption,
                    "gridArea",
                    SystemClock.Instance.GetCurrentInstant(),
                    ConnectionState.New,
                    null));

            defaultChargeLinkRepository.Setup(
                    f => f.GetAsync(
                        It.IsAny<MeteringPointType>()))
                .ReturnsAsync(new List<DefaultChargeLink>());

            // Act
            await sut.HandleAsync(createLinkCommandEvent, correlationId).ConfigureAwait(false);

            // Assert
            defaultChargeLinkClient.Verify(
                x => x.CreateDefaultChargeLinksSucceededReplyAsync(
                    It.IsAny<CreateDefaultChargeLinksSucceededDto>(),
                    correlationId,
                    replyTo));

            dispatcher.Verify(
                x => x.DispatchAsync(
                    It.IsAny<ChargeLinkCommandReceivedEvent>(),
                    It.IsAny<CancellationToken>()),
                Times.Never());
        }
    }
}
