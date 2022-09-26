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
using System.Linq;
using System.Threading.Tasks;
using AutoFixture.Xunit2;
using FluentAssertions;
using GreenEnergyHub.Charges.Application.Messaging;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeLinksAcceptedEvents;
using GreenEnergyHub.Charges.Domain.Dtos.SharedDtos;
using GreenEnergyHub.Charges.Domain.MarketParticipants;
using GreenEnergyHub.Charges.Infrastructure.Core.Cim.MarketDocument;
using GreenEnergyHub.Charges.MessageHub.Models.AvailableChargeLinksReceiptData;
using GreenEnergyHub.Charges.TestCore.Builders.Command;
using GreenEnergyHub.Charges.Tests.MessageHub.Models.Shared;
using GreenEnergyHub.TestHelpers;
using Moq;
using NodaTime;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.MessageHub.Models.AvailableChargeLinkReceiptData
{
    [UnitTest]
    public class AvailableChargeLinkReceiptDataFactoryTests
    {
        [Theory]
        [InlineAutoDomainData(MarketParticipantRole.EnergySupplier)]
        [InlineAutoDomainData(MarketParticipantRole.GridAccessProvider)]
        public async Task CreateAsync_WhenSenderNotSystemOperator_ReturnsAvailableData(
            MarketParticipantRole marketParticipantRole,
            MarketParticipant meteringPointAdministrator,
            [Frozen] Mock<IMarketParticipantRepository> marketParticipantRepository,
            [Frozen] Mock<IMessageMetaDataContext> messageMetaDataContext,
            Instant now,
            AvailableChargeLinksReceiptDataFactory sut)
        {
            // Arrange
            messageMetaDataContext.Setup(m => m.RequestDataTime).Returns(now);
            var acceptedEvent = BuildEvent(marketParticipantRole);
            var expectedLinks = acceptedEvent.Command.Operations.ToList();
            var actorId = Guid.NewGuid();
            MarketParticipantRepositoryMockBuilder.SetupMarketParticipantRepositoryMock(
                marketParticipantRepository, meteringPointAdministrator, acceptedEvent.Command.Document.Sender, actorId);

            // Act
            var actualList = await sut.CreateAsync(acceptedEvent);

            // Assert
            actualList.Should().HaveSameCount(expectedLinks);
            for (var i = 0; i < actualList.Count; i++)
            {
                actualList[i].ActorId.Should().Be(actorId);
                actualList[i].RecipientId.Should().Be(acceptedEvent.Command.Document.Sender.MarketParticipantId);
                actualList[i].RecipientRole.Should().Be(acceptedEvent.Command.Document.Sender.BusinessProcessRole);
                actualList[i].BusinessReasonCode.Should().Be(acceptedEvent.Command.Document.BusinessReasonCode);
                actualList[i].RequestDateTime.Should().Be(now);
                actualList[i].ReceiptStatus.Should().Be(ReceiptStatus.Confirmed);
                actualList[i].DocumentType.Should().Be(DocumentType.ConfirmRequestChangeBillingMasterData);
                actualList[i].OriginalOperationId.Should().Be(expectedLinks[i].OperationId);
                actualList[i].MeteringPointId.Should().Be(expectedLinks[i].MeteringPointId);
                actualList[i].ValidationErrors.Should().BeEmpty();
            }
        }

        [Theory]
        [InlineAutoDomainData]
        public async Task CreateAsync_WhenSenderIsSystemOperator_ReturnsEmptyList(
            AvailableChargeLinksReceiptDataFactory sut)
        {
            // Arrange
            var acceptedEvent = BuildEvent(MarketParticipantRole.SystemOperator);

            // Act
            var actualList = await sut.CreateAsync(acceptedEvent);

            // Assert
            actualList.Should().BeEmpty();
        }

        private static ChargeLinksAcceptedEvent BuildEvent(MarketParticipantRole marketParticipantRole)
        {
            var marketParticipant = new MarketParticipantDtoBuilder()
                .WithMarketParticipantRole(marketParticipantRole)
                .Build();
            var documentDto = new DocumentDtoBuilder().WithSender(marketParticipant).Build();
            var command = new ChargeLinksCommandBuilder().WithDocument(documentDto).Build();
            return new ChargeLinksAcceptedEventBuilder().WithCommand(command).Build();
        }
    }
}
