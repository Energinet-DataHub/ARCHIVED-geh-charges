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
using Energinet.DataHub.MessageHub.Client.Model;
using FluentAssertions;
using GreenEnergyHub.Charges.Application.ChargeLinks.MessageHub;
using GreenEnergyHub.Charges.Domain.AvailableChargeData;
using GreenEnergyHub.Charges.Domain.ChargeCommandAcceptedEvents;
using GreenEnergyHub.Charges.Domain.ChargeCommands;
using GreenEnergyHub.Charges.Domain.Charges;
using GreenEnergyHub.Charges.Domain.MarketParticipants;
using GreenEnergyHub.Charges.TestCore.Attributes;
using GreenEnergyHub.Charges.TestCore.Reflection;
using Moq;
using NodaTime;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Application.Charges.MessageHub
{
    [UnitTest]
    public class ChargeDataAvailableNotifierTests
    {
        [Theory]
        [InlineAutoMoqData]
        public async Task NotifyAsync_WhenEventIsNull_ThrowsArgumentNullException(
            ChargeDataAvailableNotifier sut)
        {
            await sut
                .Invoking(notifier => notifier.NotifyAsync(null!))
                .Should()
                .ThrowExactlyAsync<ArgumentNullException>();
        }

        [Theory]
        [InlineAutoMoqData]
        public async Task NotifyAsync_WhenChargeIsTaxIndicator_SendsDataAvailableNotificationPerGridAccessProvider(
            ChargeCommandAcceptedEvent chargeCommandAcceptedEvent,
            List<MarketParticipant> gridAccessProviders,
            AvailableChargeData availableChargesData,
            [Frozen] Mock<IDataAvailableNotificationSender> dataAvailableNotificationSenderMock,
            [Frozen] Mock<IAvailableChargeDataFactory> availableChargesDataFactoryMock,
            [Frozen] Mock<IMarketParticipantRepository> marketParticipantRepositoryMock,
            ChargeDataAvailableNotifier sut)
        {
            // Arrange
            chargeCommandAcceptedEvent
                .Command.ChargeOperation.SetPrivateProperty(c => c.TaxIndicator, true);
            marketParticipantRepositoryMock
                .Setup(repository => repository.GetActiveGridAccessProvidersAsync())
                .ReturnsAsync(gridAccessProviders);

            availableChargesDataFactoryMock.Setup(
                    factory => factory.Create(
                        It.IsAny<ChargeCommand>(),
                        It.IsAny<MarketParticipant>(),
                        It.IsAny<Instant>(),
                        It.IsAny<Guid>()))
                .Returns(availableChargesData);

            // Act
            await sut.NotifyAsync(chargeCommandAcceptedEvent);

            // Assert
            dataAvailableNotificationSenderMock.Verify(
                sender => sender.SendAsync(
                    It.Is<DataAvailableNotificationDto>(
                        dto => dto.Origin == DomainOrigin.Charges
                               && dto.SupportsBundling
                               && gridAccessProviders
                                   .Select(provider => new GlobalLocationNumberDto(provider.Id))
                                   .Contains(dto.Recipient)
                               && dto.Uuid != Guid.Empty
                               && dto.RelativeWeight > 0)),
                Times.Exactly(gridAccessProviders.Count));
            dataAvailableNotificationSenderMock.VerifyNoOtherCalls();
        }

        [Theory]
        [InlineAutoMoqData]
        public async Task NotifyAsync_WhenChargeIsNotTaxIndicator_DoesNotSendDataAvailableNotification(
            ChargeCommandAcceptedEvent chargeCommandAcceptedEvent,
            [Frozen] Mock<IDataAvailableNotificationSender> dataAvailableNotificationSenderMock,
            ChargeDataAvailableNotifier sut)
        {
            // Arrange

            // Act
            await sut.NotifyAsync(chargeCommandAcceptedEvent);

            // Assert
            dataAvailableNotificationSenderMock.VerifyNoOtherCalls();
        }
    }
}
