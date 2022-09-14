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
using FluentAssertions;
using GreenEnergyHub.Charges.Application.Messaging;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommandRejectedEvents;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeInformationCommands;
using GreenEnergyHub.Charges.Domain.Dtos.SharedDtos;
using GreenEnergyHub.Charges.Domain.Dtos.Validation;
using GreenEnergyHub.Charges.Domain.MarketParticipants;
using GreenEnergyHub.Charges.Infrastructure.Core.Cim;
using GreenEnergyHub.Charges.Infrastructure.Core.Cim.MarketDocument;
using GreenEnergyHub.Charges.MessageHub.Models.AvailableChargeReceiptData;
using GreenEnergyHub.Charges.MessageHub.Models.AvailableData;
using GreenEnergyHub.Charges.TestCore.Attributes;
using GreenEnergyHub.Charges.Tests.Builders.Command;
using GreenEnergyHub.Charges.Tests.Builders.Testables;
using GreenEnergyHub.Charges.Tests.MessageHub.Models.Shared;
using Moq;
using NodaTime;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.MessageHub.Models.AvailableChargeReceiptData
{
    [UnitTest]
    public class AvailableChargeRejectionDataFactoryTests
    {
        [Theory]
        [InlineAutoMoqData]
        public async Task CreateAsync_WhenCalledWithRejectedEvent_ReturnsAvailableData(
            TestMeteringPointAdministrator meteringPointAdministrator,
            [Frozen] Mock<IMarketParticipantRepository> marketParticipantRepository,
            [Frozen] Mock<IMessageMetaDataContext> messageMetaDataContext,
            [Frozen] Mock<IAvailableChargeReceiptValidationErrorFactory> availableChargeReceiptValidationErrorFactory,
            List<ChargeInformationOperationDto> chargeOperations,
            ChargeInformationCommandBuilder chargeInformationCommandBuilder,
            Instant now,
            AvailableChargeRejectionDataFactory sut)
        {
            // Arrange
            var chargeCommand = chargeInformationCommandBuilder.WithChargeOperations(chargeOperations).Build();
            messageMetaDataContext.Setup(m => m.RequestDataTime).Returns(now);
            var actorId = Guid.NewGuid();
            MarketParticipantRepositoryMockBuilder.SetupMarketParticipantRepositoryMock(
                marketParticipantRepository, meteringPointAdministrator, chargeCommand.Document.Sender, actorId);

            SetupAvailableChargeReceiptValidationErrorFactoryMock(availableChargeReceiptValidationErrorFactory);

            var validationErrors = chargeCommand.Operations
                .Reverse() // GetReasons() should provide the correct ValidationError no matter what order they have here
                .Select(x => new ValidationError(ValidationRuleIdentifier.SenderIsMandatoryTypeValidation, x.OperationId, null))
                .ToList();

            var chargeCommandRejectedEvent =
                new ChargeInformationCommandRejectedEvent(now, chargeCommand, validationErrors);

            // Act
            var actualList = await sut.CreateAsync(chargeCommandRejectedEvent);

            // Assert
            actualList.Should().HaveSameCount(chargeOperations);
            var operationOrder = -1;

            for (var i1 = 0; i1 < actualList.Count; i1++)
            {
                var actual = actualList[i1];
                var chargeCommandDocument = chargeCommand.Document;
                actual.ActorId.Should().Be(actorId);
                actual.RecipientId.Should().Be(chargeCommandDocument.Sender.MarketParticipantId);
                actual.RecipientRole.Should().Be(chargeCommandDocument.Sender.BusinessProcessRole);
                actual.BusinessReasonCode.Should().Be(chargeCommandDocument.BusinessReasonCode);
                actual.RequestDateTime.Should().Be(now);
                actual.ReceiptStatus.Should().Be(ReceiptStatus.Rejected);
                actual.DocumentType.Should().Be(DocumentType.RejectRequestChangeOfPriceList);

                var expectedChargeOperationDto = chargeCommand.Operations.ToArray()[i1];
                actual.OriginalOperationId.Should().Be(expectedChargeOperationDto.OperationId);

                var actualValidationErrors = actual.ValidationErrors.ToList();
                var expectedValidationErrors = validationErrors.Where(x => x.OperationId == expectedChargeOperationDto.OperationId);
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

        private static void SetupAvailableChargeReceiptValidationErrorFactoryMock(
            Mock<IAvailableChargeReceiptValidationErrorFactory> availableChargeReceiptValidationErrorFactory)
        {
            // fake error code and text
            availableChargeReceiptValidationErrorFactory
                .Setup(f => f.Create(It.IsAny<ValidationError>(), It.IsAny<DocumentDto>(), It.IsAny<ChargeInformationOperationDto>()))
                .Returns<ValidationError, DocumentDto, ChargeInformationOperationDto>((validationError, _, _) =>
                    new AvailableReceiptValidationError(
                        ReasonCode.D01, validationError.ValidationRuleIdentifier.ToString()));
        }
    }
}
