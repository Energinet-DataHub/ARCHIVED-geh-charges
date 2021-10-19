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
using System.Threading.Tasks;
using AutoFixture.Xunit2;
using Energinet.DataHub.MessageHub.Client.DataAvailable;
using Energinet.DataHub.MessageHub.Client.Model;
using FluentAssertions;
using GreenEnergyHub.Charges.Application.ChargeLinks.MessageHub;
using GreenEnergyHub.Charges.Domain.ChargeLinkCommandAcceptedEvents;
using GreenEnergyHub.Charges.Domain.Charges;
using GreenEnergyHub.Charges.TestCore.Attributes;
using GreenEnergyHub.Charges.TestCore.Reflection;
using Moq;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Application.ChargeLinks.MessageHub
{
    [UnitTest]
    public class ChargeLinkCreatedDataAvailableNotifierTests
    {
        [Theory]
        [InlineAutoMoqData]
        public async Task NotifyAsync_WhenEventIsNull_ThrowsArgumentNullException(
            ChargeLinkCreatedDataAvailableNotifier sut)
        {
            await sut
                .Invoking(notifier => notifier.NotifyAsync(null!))
                .Should()
                .ThrowExactlyAsync<ArgumentNullException>();
        }

        [Theory]
        [InlineAutoMoqData]
        public async Task NotifyAsync_WhenChargeIsTaxIndicator_SendsDataAvailableNotification(
            ChargeLinkCommandAcceptedEvent chargeLinkCommandAcceptedEvent,
            Charge charge,
            [Frozen] Mock<IChargeRepository> chargeRepositoryMock,
            [Frozen] Mock<IDataAvailableNotificationSender> dataAvailableNotificationSenderMock,
            ChargeLinkCreatedDataAvailableNotifier sut)
        {
            // Arrange
            var link = chargeLinkCommandAcceptedEvent.ChargeLink;
            charge.SetPrivateProperty(c => c.TaxIndicator, true);
            chargeRepositoryMock.Setup(
                    repository => repository.GetChargeAsync(link.SenderProvidedChargeId, link.ChargeOwner, link.ChargeType))
                .ReturnsAsync(charge);

            // Act
            await sut.NotifyAsync(chargeLinkCommandAcceptedEvent);

            // Assert
            dataAvailableNotificationSenderMock.Verify(
                sender => sender.SendAsync(
                    It.Is<DataAvailableNotificationDto>(
                        dto => dto.Origin == DomainOrigin.Charges
                               && dto.SupportsBundling
                               && dto.Recipient.Equals(
                                   new GlobalLocationNumberDto(chargeLinkCommandAcceptedEvent.Document.Sender.Id))
                               && dto.Uuid != Guid.Empty
                               && dto.RelativeWeight > 0)),
                Times.Once);
        }
    }
}
