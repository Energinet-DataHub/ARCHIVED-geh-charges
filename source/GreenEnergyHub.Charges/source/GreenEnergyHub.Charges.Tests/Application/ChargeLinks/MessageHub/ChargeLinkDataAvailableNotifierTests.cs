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
using System.Linq;
using System.Threading.Tasks;
using AutoFixture.Xunit2;
using Energinet.DataHub.MessageHub.Client.DataAvailable;
using Energinet.DataHub.MessageHub.Model.Model;
using FluentAssertions;
using GreenEnergyHub.Charges.Application.ChargeLinks.MessageHub;
using GreenEnergyHub.Charges.Domain.AvailableChargeLinksData;
using GreenEnergyHub.Charges.Domain.Charges;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeLinkCommandAcceptedEvents;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeLinkCommands;
using GreenEnergyHub.Charges.Domain.MarketParticipants;
using GreenEnergyHub.Charges.TestCore.Attributes;
using GreenEnergyHub.Charges.TestCore.Reflection;
using Moq;
using NodaTime;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Application.ChargeLinks.MessageHub
{
    [UnitTest]
    public class ChargeLinkDataAvailableNotifierTests
    {
        [Theory]
        [InlineAutoMoqData]
        public async Task NotifyAsync_WhenEventIsNull_ThrowsArgumentNullException(
            ChargeLinkDataAvailableNotifier sut)
        {
            await sut
                .Invoking(notifier => notifier.NotifyAsync(null!))
                .Should()
                .ThrowExactlyAsync<ArgumentNullException>();
        }

        [Theory]
        [InlineAutoMoqData]
        public async Task NotifyAsync_WhenChargeIsTaxIndicator_SendsDataAvailableNotificationPerCommand(
            ChargeLinkCommandAcceptedEvent chargeLinkCommandAcceptedEvent,
            Charge charge,
            MarketParticipant gridAccessProvider,
            AvailableChargeLinksData availableChargeLinksData,
            [Frozen] Mock<IChargeRepository> chargeRepositoryMock,
            [Frozen] Mock<IDataAvailableNotificationSender> dataAvailableNotificationSenderMock,
            [Frozen] Mock<IAvailableChargeLinksDataFactory> availableChargeLinksDataFactoryMock,
            [Frozen] Mock<IMarketParticipantRepository> marketParticipantRepositoryMock,
            ChargeLinkDataAvailableNotifier sut)
        {
            // Arrange
            charge.SetPrivateProperty(c => c.TaxIndicator, true);

            FixMeteringPointIds(chargeLinkCommandAcceptedEvent.ChargeLinkCommands);

            chargeRepositoryMock
                .Setup(repository => repository.GetChargeAsync(It.IsAny<ChargeIdentifier>()))
                .ReturnsAsync(charge);

            marketParticipantRepositoryMock
                .Setup(repository => repository.GetGridAccessProvider(
                    chargeLinkCommandAcceptedEvent.ChargeLinkCommands.First().ChargeLink.MeteringPointId))
                .Returns(gridAccessProvider);

            availableChargeLinksDataFactoryMock.Setup(
                    factory => factory.CreateAvailableChargeLinksData(
                        It.IsAny<ChargeLinkCommand>(),
                        It.IsAny<MarketParticipant>(),
                        It.IsAny<Instant>(),
                        It.IsAny<Guid>()))
                .Returns(availableChargeLinksData);

            // Act
            await sut.NotifyAsync(chargeLinkCommandAcceptedEvent);

            // Assert
            dataAvailableNotificationSenderMock.Verify(
                sender => sender.SendAsync(
                    It.IsAny<string>(),
                    It.Is<DataAvailableNotificationDto>(
                        dto => dto.Origin == DomainOrigin.Charges
                               && dto.SupportsBundling
                               && dto.Recipient.Equals(
                                   new GlobalLocationNumberDto(gridAccessProvider.Id))
                               && dto.Uuid != Guid.Empty
                               && dto.RelativeWeight > 0)),
                Times.Exactly(chargeLinkCommandAcceptedEvent.ChargeLinkCommands.Count));

            dataAvailableNotificationSenderMock.VerifyNoOtherCalls();
        }

        [Theory]
        [InlineAutoMoqData]
        public async Task NotifyAsync_WhenChargeIsNotTaxIndicator_DoesNotSendDataAvailableNotification(
            ChargeLinkCommandAcceptedEvent chargeLinkCommandAcceptedEvent,
            Charge charge,
            [Frozen] Mock<IChargeRepository> chargeRepositoryMock,
            [Frozen] Mock<IDataAvailableNotificationSender> dataAvailableNotificationSenderMock,
            ChargeLinkDataAvailableNotifier sut)
        {
            // Arrange
            FixMeteringPointIds(chargeLinkCommandAcceptedEvent.ChargeLinkCommands);

            charge.SetPrivateProperty(c => c.TaxIndicator, false);

            chargeRepositoryMock.Setup(repository =>
                    repository.GetChargeAsync(It.IsAny<ChargeIdentifier>()))
                .ReturnsAsync(charge);

            // Act
            await sut.NotifyAsync(chargeLinkCommandAcceptedEvent);

            // Assert
            dataAvailableNotificationSenderMock.VerifyNoOtherCalls();
        }

        // This is a workaround because the model contains multiple metering point IDs while
        // business does not.
        private void FixMeteringPointIds(IReadOnlyCollection<ChargeLinkCommand> chargeLinkCommands)
        {
            var meteringPointId = chargeLinkCommands.First().ChargeLink.MeteringPointId;
            foreach (var command in chargeLinkCommands)
            {
                command.ChargeLink.MeteringPointId = meteringPointId;
            }
        }
    }
}
