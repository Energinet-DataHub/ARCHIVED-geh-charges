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
using AutoFixture.Xunit2;
using Energinet.DataHub.MessageHub.Client.DataAvailable;
using Energinet.DataHub.MessageHub.Model.Model;
using FluentAssertions;
using GreenEnergyHub.Charges.Application;
using GreenEnergyHub.Charges.Application.ChargeLinks.MessageHub;
using GreenEnergyHub.Charges.Domain.AvailableChargeLinkReceiptData;
using GreenEnergyHub.Charges.Domain.AvailableData;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeLinksAcceptedEvents;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeLinksCommands;
using GreenEnergyHub.Charges.Domain.MarketParticipants;
using GreenEnergyHub.Charges.TestCore.Attributes;
using Moq;
using NodaTime;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Application.ChargeLinks.MessageHub
{
    [UnitTest]
    public class ChargeLinkConfirmationDataAvailableNotifierTests
    {
        [Theory]
        [InlineAutoMoqData]
        public async Task NotifyAsync_WhenSenderIsSystemOperator_DoesNothing(
            [Frozen] Mock<IAvailableChargeLinkReceiptDataFactory> availableChargeLinkReceiptFactory,
            [Frozen] Mock<IMessageMetaDataContext> messageMetaDataContext,
            [Frozen] Mock<IAvailableDataRepository<AvailableChargeLinkReceiptData>> availableChargeLinkReceiptDataRepository,
            [Frozen] Mock<IDataAvailableNotificationSender> dataAvailableNotificationSender,
            [Frozen] Mock<ICorrelationContext> correlationContext,
            ChargeLinksAcceptedEvent acceptedEvent,
            ChargeLinkConfirmationDataAvailableNotifier sut)
        {
            // Arrange
            acceptedEvent.ChargeLinksCommand.Document.Sender.BusinessProcessRole =
                MarketParticipantRole.SystemOperator;

            // Act
            await sut.NotifyAsync(acceptedEvent);

            // Assert
            availableChargeLinkReceiptFactory.Verify(
                a => a.CreateConfirmations(
                    It.IsAny<ChargeLinksCommand>(),
                    It.IsAny<Instant>()),
                Times.Never);

            messageMetaDataContext.Verify(
                m => m.RequestDataTime,
                Times.Never);

            availableChargeLinkReceiptDataRepository.Verify(
                a => a.StoreAsync(It.IsAny<List<AvailableChargeLinkReceiptData>>()),
                Times.Never);

            dataAvailableNotificationSender.Verify(
                d => d.SendAsync(
                    It.IsAny<string>(),
                    It.IsAny<DataAvailableNotificationDto>()),
                Times.Never);

            correlationContext.Verify(
                c => c.Id,
                Times.Never);
        }

        [Theory]
        [InlineAutoMoqData(MarketParticipantRole.GridAccessProvider)]
        [InlineAutoMoqData(MarketParticipantRole.EnergySupplier)]
        public async Task NotifyAsync_WhenSenderIsNotSystemOperator_Notifies(
            MarketParticipantRole role,
            [Frozen] Mock<IAvailableChargeLinkReceiptDataFactory> availableChargeLinkReceiptFactory,
            [Frozen] Mock<IMessageMetaDataContext> messageMetaDataContext,
            [Frozen] Mock<IAvailableDataRepository<AvailableChargeLinkReceiptData>> availableChargeLinkReceiptDataRepository,
            [Frozen] Mock<IDataAvailableNotificationSender> dataAvailableNotificationSender,
            [Frozen] Mock<ICorrelationContext> correlationContext,
            ChargeLinksAcceptedEvent acceptedEvent,
            IReadOnlyCollection<AvailableChargeLinkReceiptData> confirmations,
            ChargeLinkConfirmationDataAvailableNotifier sut)
        {
            // Arrange
            acceptedEvent.ChargeLinksCommand.Document.Sender.BusinessProcessRole = role;

            var requestDateTime = SystemClock.Instance.GetCurrentInstant();
            var correlationId = Guid.NewGuid().ToString();

            messageMetaDataContext.Setup(
                    m => m.RequestDataTime)
                .Returns(requestDateTime);

            availableChargeLinkReceiptFactory.Setup(
                    a => a.CreateConfirmations(
                        acceptedEvent.ChargeLinksCommand,
                        requestDateTime))
                .Returns(confirmations);

            correlationContext.Setup(c => c.Id)
                .Returns(correlationId);

            // Act
            await sut.NotifyAsync(acceptedEvent);

            // Assert
            messageMetaDataContext.Verify(
                    m => m.RequestDataTime,
                    Times.Once);

            availableChargeLinkReceiptFactory.Verify(
                a => a.CreateConfirmations(
                    acceptedEvent.ChargeLinksCommand,
                    requestDateTime),
                Times.Once);

            availableChargeLinkReceiptDataRepository.Verify(
                a => a.StoreAsync(
                    It.IsAny<List<AvailableChargeLinkReceiptData>>()),
                Times.Once);

            dataAvailableNotificationSender.Verify(
                d => d.SendAsync(
                    correlationId,
                    It.IsAny<DataAvailableNotificationDto>()),
                Times.Exactly(acceptedEvent.ChargeLinksCommand.ChargeLinks.Count));
        }
    }
}
