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
using System.Linq;
using System.Threading.Tasks;
using AutoFixture.Xunit2;
using Energinet.DataHub.Core.TestCommon.AutoFixture.Attributes;
using FluentAssertions;
using GreenEnergyHub.Charges.Application.Charges.Events;
using GreenEnergyHub.Charges.Application.Messaging;
using GreenEnergyHub.Charges.Domain.Dtos.ChargePriceCommands;
using GreenEnergyHub.Charges.Domain.Dtos.SharedDtos;
using GreenEnergyHub.Charges.Domain.Dtos.Validation;
using GreenEnergyHub.Charges.Domain.MarketParticipants;
using GreenEnergyHub.Charges.Infrastructure.Core.Cim;
using GreenEnergyHub.Charges.Infrastructure.Core.Cim.MarketDocument;
using GreenEnergyHub.Charges.MessageHub.Models.AvailableData;
using GreenEnergyHub.Charges.MessageHub.Models.AvailableOperationReceiptData;
using GreenEnergyHub.Charges.Tests.Builders.Command;
using GreenEnergyHub.Charges.Tests.Builders.Testables;
using GreenEnergyHub.Charges.Tests.MessageHub.Models.Shared;
using Moq;
using NodaTime;
using Xunit;

namespace GreenEnergyHub.Charges.Tests.MessageHub.Models.AvailableOperationReceiptData
{
    public class AvailableChargePriceOperationRejectionsFactoryTests
    {
        [Theory]
        [InlineAutoMoqData]
        public async Task CreateAsync_WhenCalledWithChargePriceRejectedEvent_ReturnsAvailableData(
            TestMeteringPointAdministrator meteringPointAdministrator,
            [Frozen] Mock<IMarketParticipantRepository> marketParticipantRepository,
            [Frozen] Mock<IMessageMetaDataContext> messageMetaDataContext,
            [Frozen] Mock<IAvailableChargePriceReceiptValidationErrorFactory> availableChargePriceReceiptValidationErrorFactory,
            List<ChargePriceOperationDto> chargePriceOperations,
            ChargePriceCommandBuilder chargePriceCommandBuilder,
            Instant now,
            AvailableChargePriceOperationRejectionsFactory sut)
        {
            // Arrange
            var chargePriceCommand = chargePriceCommandBuilder.WithChargeOperations(chargePriceOperations).Build();
            messageMetaDataContext.Setup(m => m.RequestDataTime).Returns(now);
            var actorId = Guid.NewGuid();
            MarketParticipantRepositoryMockBuilder.SetupMarketParticipantRepositoryMock(
                marketParticipantRepository, meteringPointAdministrator, chargePriceCommand.Document.Sender, actorId);

            SetupAvailablePriceReceiptValidationErrorFactoryMock(
                availableChargePriceReceiptValidationErrorFactory, chargePriceCommand);

            var validationErrors = chargePriceCommand.Operations
                .Reverse() // GetReasons() should provide the correct ValidationError no matter what order they have here
                .Select(x => new ValidationError(ValidationRuleIdentifier.SenderIsMandatoryTypeValidation, x.OperationId, null))
                .ToList();

            var chargePriceOperationsRejectedEvent =
                new ChargePriceOperationsRejectedEvent(now, chargePriceCommand, validationErrors);

            // Act
            var actualList = await sut.CreateAsync(chargePriceOperationsRejectedEvent);

            // Assert
            actualList.Should().HaveSameCount(chargePriceOperations);
            var operationOrder = -1;

            for (var i1 = 0; i1 < actualList.Count; i1++)
            {
                var actual = actualList[i1];
                var chargeCommandDocument = chargePriceCommand.Document;
                actual.ActorId.Should().Be(actorId);
                actual.RecipientId.Should().Be(chargeCommandDocument.Sender.MarketParticipantId);
                actual.RecipientRole.Should().Be(chargeCommandDocument.Sender.BusinessProcessRole);
                actual.BusinessReasonCode.Should().Be(chargeCommandDocument.BusinessReasonCode);
                actual.RequestDateTime.Should().Be(now);
                actual.ReceiptStatus.Should().Be(ReceiptStatus.Rejected);
                actual.DocumentType.Should().Be(DocumentType.RejectRequestChangeOfPriceList);

                var expectedChargeOperationDto = chargePriceCommand.Operations.ToArray()[i1];
                actual.OriginalOperationId.Should().Be(expectedChargeOperationDto.OperationId);

                var actualValidationErrors = actual.ValidationErrors.ToList();
                var expectedValidationErrors =
                    validationErrors.Where(x => x.OperationId == expectedChargeOperationDto.OperationId);
                actual.ValidationErrors.Should().HaveSameCount(expectedValidationErrors);
                actual.OperationOrder.Should().BeGreaterThan(operationOrder);
                operationOrder = actual.OperationOrder;

                for (var i2 = 0; i2 < actualValidationErrors.Count; i2++)
                {
                    var expectedText = validationErrors[i2].ValidationRuleIdentifier.ToString();
                    actualValidationErrors[i2].Text.Should().Be(expectedText);
                    actualValidationErrors[i2].ReasonCode.ToString().Should().NotBeNullOrWhiteSpace();
                }
            }
        }

        private static void SetupAvailablePriceReceiptValidationErrorFactoryMock(
            Mock<IAvailableChargePriceReceiptValidationErrorFactory> availablePriceReceiptValidationErrorFactory,
            ChargePriceCommand chargePriceCommand)
        {
            // fake error code and text
            availablePriceReceiptValidationErrorFactory
                .Setup(f => f.Create(It.IsAny<ValidationError>(), chargePriceCommand, It.IsAny<ChargePriceOperationDto>()))
                .Returns<ValidationError, ChargePriceCommand, ChargePriceOperationDto>((validationError, _, _) =>
                    new AvailableReceiptValidationError(
                        ReasonCode.D01, validationError.ValidationRuleIdentifier.ToString()));
        }
    }
}
