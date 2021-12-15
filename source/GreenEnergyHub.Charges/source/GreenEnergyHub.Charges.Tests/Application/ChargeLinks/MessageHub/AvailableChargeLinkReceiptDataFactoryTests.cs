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

using System.Linq;
using System.Threading.Tasks;
using AutoFixture.Xunit2;
using FluentAssertions;
using GreenEnergyHub.Charges.Application;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeLinksAcceptedEvents;
using GreenEnergyHub.Charges.Domain.MarketParticipants;
using GreenEnergyHub.Charges.Infrastructure.Core.MessageMetaData;
using GreenEnergyHub.Charges.MessageHub.Application.ChargeLinks;
using GreenEnergyHub.Charges.MessageHub.Models.AvailableData;
using GreenEnergyHub.TestHelpers;
using Moq;
using NodaTime;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Application.ChargeLinks.MessageHub
{
    [UnitTest]
    public class AvailableChargeLinkReceiptDataFactoryTests
    {
        [Theory]
        [InlineAutoDomainData(MarketParticipantRole.EnergySupplier)]
        [InlineAutoDomainData(MarketParticipantRole.GridAccessProvider)]
        public async Task CreateAsync_WhenSenderNotSystemOperator_ReturnsAvailableData(
            MarketParticipantRole marketParticipantRole,
            [Frozen] Mock<IMessageMetaDataContext> messageMetaDataContext,
            ChargeLinksAcceptedEvent acceptedEvent,
            Instant now,
            AvailableChargeLinkReceiptDataFactory sut)
        {
            // Arrange
            acceptedEvent.ChargeLinksCommand.Document.Sender.BusinessProcessRole = marketParticipantRole;

            messageMetaDataContext.Setup(
                    m => m.RequestDataTime)
                .Returns(now);

            var expectedLinks = acceptedEvent.ChargeLinksCommand.ChargeLinks.ToList();

            // Act
            var actualList =
                await sut.CreateAsync(acceptedEvent);

            // Assert
            actualList.Should().HaveSameCount(expectedLinks);
            for (var i = 0; i < actualList.Count; i++)
            {
                actualList[i].RecipientId.Should().Be(acceptedEvent.ChargeLinksCommand.Document.Sender.Id);
                actualList[i].RecipientRole.Should()
                    .Be(acceptedEvent.ChargeLinksCommand.Document.Sender.BusinessProcessRole);
                actualList[i].BusinessReasonCode.Should()
                    .Be(acceptedEvent.ChargeLinksCommand.Document.BusinessReasonCode);
                actualList[i].RequestDateTime.Should().Be(now);
                actualList[i].ReceiptStatus.Should().Be(ReceiptStatus.Confirmed);
                actualList[i].OriginalOperationId.Should().Be(expectedLinks[i].OperationId);
                actualList[i].MeteringPointId.Should().Be(acceptedEvent.ChargeLinksCommand.MeteringPointId);
                actualList[i].ReasonCodes.Should().BeEmpty();
            }
        }

        [Theory]
        [InlineAutoDomainData]
        public async Task CreateAsync_WhenSenderIsSystemOperator_ReturnsEmptyList(
            ChargeLinksAcceptedEvent acceptedEvent,
            AvailableChargeLinkReceiptDataFactory sut)
        {
            // Arrange
            acceptedEvent.ChargeLinksCommand.Document.Sender.BusinessProcessRole = MarketParticipantRole.SystemOperator;

            // Act
            var actualList =
                await sut.CreateAsync(acceptedEvent);

            // Assert
            actualList.Should().BeEmpty();
        }
    }
}
