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
using GreenEnergyHub.Charges.Domain.Dtos.Events;
using GreenEnergyHub.Charges.Domain.Dtos.SharedDtos;
using GreenEnergyHub.Charges.Domain.MarketParticipants;
using GreenEnergyHub.Charges.Infrastructure.Core.Cim.MarketDocument;
using GreenEnergyHub.Charges.MessageHub.Models.AvailableChargeReceiptData;
using GreenEnergyHub.Charges.TestCore.Attributes;
using GreenEnergyHub.Charges.TestCore.Builders.Testables;
using GreenEnergyHub.Charges.Tests.MessageHub.Models.Shared;
using Moq;
using NodaTime;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.MessageHub.Models.AvailableChargeReceiptData
{
    [UnitTest]
    public class AvailableChargeConfirmationDataFactoryTests
    {
        [Theory]
        [InlineAutoMoqData]
        public async Task CreateAsync_WhenCalledWithAcceptedEvent_ReturnsAvailableData(
            TestMeteringPointAdministrator meteringPointAdministrator,
            [Frozen] Mock<IMarketParticipantRepository> marketParticipantRepository,
            [Frozen] Mock<IMessageMetaDataContext> messageMetaDataContext,
            ChargeInformationOperationsAcceptedEvent acceptedEvent,
            Instant now,
            AvailableChargeReceiptDataFactory sut)
        {
            // Arrange
            messageMetaDataContext.Setup(m => m.RequestDataTime).Returns(now);
            var documentDto = acceptedEvent.Document;
            documentDto.Sender.BusinessProcessRole = MarketParticipantRole.GridAccessProvider;
            var actorId = Guid.NewGuid();
            MarketParticipantRepositoryMockBuilder.SetupMarketParticipantRepositoryMock(
                marketParticipantRepository, meteringPointAdministrator, documentDto.Sender, actorId);

            // Act
            var actualList = await sut.CreateAsync(acceptedEvent);

            // Assert
            actualList.Should().HaveCount(3);
            actualList[0].ActorId.Should().Be(actorId);
            actualList[0].RecipientId.Should().Be(documentDto.Sender.MarketParticipantId);
            actualList[0].RecipientRole.Should().Be(documentDto.Sender.BusinessProcessRole);
            actualList[0].BusinessReasonCode.Should().Be(documentDto.BusinessReasonCode);
            actualList[0].RequestDateTime.Should().Be(now);
            actualList[0].ReceiptStatus.Should().Be(ReceiptStatus.Confirmed);
            actualList[0].DocumentType.Should().Be(DocumentType.ConfirmRequestChangeOfPriceList);
            actualList[0].OriginalOperationId.Should().Be(acceptedEvent.Operations.First().OperationId);
            actualList[0].ValidationErrors.Should().BeEmpty();
            var expectedList = actualList.OrderBy(x => x.OperationOrder);
            actualList.SequenceEqual(expectedList).Should().BeTrue();
        }
    }
}
