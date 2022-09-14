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
using Energinet.DataHub.Core.TestCommon.AutoFixture.Attributes;
using FluentAssertions;
using GreenEnergyHub.Charges.Application.Charges.Events;
using GreenEnergyHub.Charges.Application.Messaging;
using GreenEnergyHub.Charges.Domain.Dtos.ChargePriceCommands;
using GreenEnergyHub.Charges.Domain.Dtos.SharedDtos;
using GreenEnergyHub.Charges.Domain.MarketParticipants;
using GreenEnergyHub.Charges.Infrastructure.Core.Cim.MarketDocument;
using GreenEnergyHub.Charges.MessageHub.Models.AvailableOperationReceiptData;
using GreenEnergyHub.Charges.Tests.Builders.Command;
using GreenEnergyHub.Charges.Tests.Builders.Testables;
using GreenEnergyHub.Charges.Tests.MessageHub.Models.Shared;
using Moq;
using NodaTime;
using Xunit;

namespace GreenEnergyHub.Charges.Tests.MessageHub.Models.AvailableOperationReceiptData
{
    public class AvailableChargePriceOperationConfirmationsFactoryTests
    {
        [Theory]
        [InlineAutoMoqData]
        public async Task CreateAsync_WhenCalledWithChargePriceConfirmedEvent_ReturnsAvailableData(
            TestMeteringPointAdministrator meteringPointAdministrator,
            [Frozen] Mock<IMarketParticipantRepository> marketParticipantRepository,
            [Frozen] Mock<IMessageMetaDataContext> messageMetaDataContext,
            IReadOnlyCollection<ChargePriceOperationDto> chargePriceOperations,
            DocumentDtoBuilder documentDtoBuilder,
            Instant now,
            AvailableChargePriceOperationConfirmationsFactory sut)
        {
            // Arrange
            messageMetaDataContext.Setup(m => m.RequestDataTime).Returns(now);
            var actorId = Guid.NewGuid();
            var document = documentDtoBuilder.WithBusinessReasonCode(BusinessReasonCode.UpdateChargePrices).Build();
            MarketParticipantRepositoryMockBuilder.SetupMarketParticipantRepositoryMock(
                marketParticipantRepository, meteringPointAdministrator, document.Sender, actorId);

            var chargePriceOperationsConfirmedEvent =
                new ChargePriceOperationsConfirmedEvent(now, document, chargePriceOperations);

            // Act
            var actualList = await sut.CreateAsync(chargePriceOperationsConfirmedEvent);

            // Assert
            actualList.Should().HaveSameCount(chargePriceOperations);
            var operationOrder = -1;

            for (var i1 = 0; i1 < actualList.Count; i1++)
            {
                var actual = actualList[i1];
                actual.ActorId.Should().Be(actorId);
                actual.RecipientId.Should().Be(document.Sender.MarketParticipantId);
                actual.RecipientRole.Should().Be(document.Sender.BusinessProcessRole);
                actual.BusinessReasonCode.Should().Be(document.BusinessReasonCode);
                actual.RequestDateTime.Should().Be(now);
                actual.ReceiptStatus.Should().Be(ReceiptStatus.Confirmed);
                actual.DocumentType.Should().Be(DocumentType.ConfirmRequestChangeOfPriceList);

                var expectedChargeOperationDto = chargePriceOperations.ToArray()[i1];
                actual.OriginalOperationId.Should().Be(expectedChargeOperationDto.OperationId);

                actual.OperationOrder.Should().BeGreaterThan(operationOrder);
                operationOrder = actual.OperationOrder;
            }
        }
    }
}
