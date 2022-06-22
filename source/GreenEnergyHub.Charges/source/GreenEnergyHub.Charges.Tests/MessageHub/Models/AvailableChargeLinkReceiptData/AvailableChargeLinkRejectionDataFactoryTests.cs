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

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture.Xunit2;
using FluentAssertions;
using GreenEnergyHub.Charges.Application.Messaging;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeLinksCommands;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeLinksRejectionEvents;
using GreenEnergyHub.Charges.Domain.Dtos.SharedDtos;
using GreenEnergyHub.Charges.Domain.Dtos.Validation;
using GreenEnergyHub.Charges.Domain.MarketParticipants;
using GreenEnergyHub.Charges.Infrastructure.Core.Cim;
using GreenEnergyHub.Charges.Infrastructure.Core.Cim.MarketDocument;
using GreenEnergyHub.Charges.MessageHub.Models.AvailableChargeLinksReceiptData;
using GreenEnergyHub.Charges.MessageHub.Models.AvailableData;
using GreenEnergyHub.Charges.Tests.Builders.Command;
using GreenEnergyHub.Charges.Tests.MessageHub.Models.Shared;
using GreenEnergyHub.TestHelpers;
using Microsoft.Extensions.Logging;
using Moq;
using NodaTime;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.MessageHub.Models.AvailableChargeLinkReceiptData
{
    [UnitTest]
    public class AvailableChargeLinkRejectionDataFactoryTests
    {
        [Theory]
        [InlineAutoDomainData(MarketParticipantRole.EnergySupplier)]
        [InlineAutoDomainData(MarketParticipantRole.GridAccessProvider)]
        public async Task CreateAsync_WhenSenderNotSystemOperator_ReturnsAvailableData(
            MarketParticipantRole marketParticipantRole,
            MarketParticipant meteringPointAdministrator,
            [Frozen] Mock<IMarketParticipantRepository> marketParticipantRepository,
            [Frozen] Mock<IMessageMetaDataContext> messageMetaDataContext,
            [Frozen] Mock<IAvailableChargeLinksReceiptValidationErrorFactory> availableChargeLinksReceiptValidationErrorFactory,
            List<ChargeLinkDto> chargeLinkDtos,
            ChargeLinksCommandBuilder chargeLinksCommandBuilder,
            Instant now,
            AvailableChargeLinksRejectionDataFactory sut)
        {
            // Arrange
            var chargeLinksCommand = chargeLinksCommandBuilder.WithChargeLinks(chargeLinkDtos).Build();
            chargeLinksCommand.Document.Sender.BusinessProcessRole = marketParticipantRole;
            messageMetaDataContext.Setup(m => m.RequestDataTime).Returns(now);

            var validationErrors = chargeLinksCommand.Operations
                .Reverse() // GetReasons() should provide the correct ValidationError no matter what order they have here
                .Select(x => new ValidationError(
                    ValidationRuleIdentifier.SenderIsMandatoryTypeValidation, x.OperationId, null))
                .ToList();

            var rejectedEvent = new ChargeLinksRejectedEvent(now, chargeLinksCommand, validationErrors);

            MarketParticipantRepositoryMockBuilder.SetupMarketParticipantRepositoryMock(
                marketParticipantRepository, meteringPointAdministrator, chargeLinksCommand.Document.Sender);

            SetupAvailableChargeLinksReceiptValidationErrorFactory(
                availableChargeLinksReceiptValidationErrorFactory, chargeLinksCommand);

            // Act
            var actualList = await sut.CreateAsync(rejectedEvent);

            // Assert
            actualList.Should().HaveSameCount(chargeLinkDtos);
            var operationOrder = -1;

            for (var i1 = 0; i1 < actualList.Count; i1++)
            {
                var actual = actualList[i1];
                actual.RecipientId.Should().Be(chargeLinksCommand.Document.Sender.MarketParticipantId);
                actual.RecipientRole.Should().Be(chargeLinksCommand.Document.Sender.BusinessProcessRole);
                actual.BusinessReasonCode.Should().Be(chargeLinksCommand.Document.BusinessReasonCode);
                actual.RequestDateTime.Should().Be(now);
                actual.ReceiptStatus.Should().Be(ReceiptStatus.Rejected);
                actual.DocumentType.Should().Be(DocumentType.RejectRequestChangeBillingMasterData);

                var expectedChargeLinkDto = chargeLinksCommand.Operations.ToArray()[i1];
                actual.OriginalOperationId.Should().Be(expectedChargeLinkDto.OperationId);
                actual.MeteringPointId.Should().Be(expectedChargeLinkDto.MeteringPointId);

                var actualValidationErrors = actual.ValidationErrors.ToList();
                var expectedValidationErrors = validationErrors.Where(x => x.OperationId == expectedChargeLinkDto.OperationId);
                actual.ValidationErrors.Should().HaveSameCount(expectedValidationErrors);
                actual.OperationOrder.Should().BeGreaterThan(operationOrder);
                operationOrder = actual.OperationOrder;

                for (var i2 = 0; i2 < actualValidationErrors.Count; i2++)
                {
                    var expectedText = validationErrors[i2].ValidationRuleIdentifier.ToString();
                    actualValidationErrors[i2].Text.Should().Be(expectedText);
                    actualValidationErrors[i2].ReasonCode.ToString().Should().Be("D01");
                }
            }
        }

        [Theory]
        [InlineAutoDomainData]
        public async Task CreateAsync_WhenCalled_ShouldLogValidationErrors(
            Instant now,
            ChargeLinksRejectedEvent rejectedEvent,
            MarketParticipantRole marketParticipantRole,
            MarketParticipant meteringPointAdministrator,
            [Frozen] Mock<IMarketParticipantRepository> marketParticipantRepository,
            [Frozen] Mock<IMessageMetaDataContext> messageMetaDataContext,
            [Frozen] Mock<IAvailableChargeLinksReceiptValidationErrorFactory> availableChargeLinksReceiptValidationErrorFactory,
            [Frozen] Mock<ILoggerFactory> loggerFactory,
            [Frozen] Mock<ILogger> logger)
        {
            // Arrange
            loggerFactory.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(logger.Object);
            rejectedEvent.ChargeLinksCommand.Document.Sender.BusinessProcessRole = marketParticipantRole;
            messageMetaDataContext.Setup(m => m.RequestDataTime).Returns(now);
            MarketParticipantRepositoryMockBuilder.SetupMarketParticipantRepositoryMock(
                marketParticipantRepository, meteringPointAdministrator, rejectedEvent.ChargeLinksCommand.Document.Sender);

            SetupAvailableChargeLinksReceiptValidationErrorFactory(
                availableChargeLinksReceiptValidationErrorFactory, rejectedEvent.ChargeLinksCommand);

            var sut = new AvailableChargeLinksRejectionDataFactory(
                messageMetaDataContext.Object,
                availableChargeLinksReceiptValidationErrorFactory.Object,
                marketParticipantRepository.Object,
                loggerFactory.Object);

            // Act
            await sut.CreateAsync(rejectedEvent);

            // Assert
            var document = rejectedEvent.ChargeLinksCommand.Document;
            var expectedMessage = $"ValidationErrors for document Id {document.Id} with Type {document.Type} from GLN {document.Sender.MarketParticipantId}:\r\n" +
                                   "- ValidationRuleIdentifier: StartDateValidation\r\n" +
                                   "- ValidationRuleIdentifier: ChangingTariffTaxValueNotAllowed\r\n" +
                                   "- ValidationRuleIdentifier: SenderIsMandatoryTypeValidation\r\n";
            logger.VerifyLoggerWasCalled(expectedMessage, LogLevel.Error);
        }

        [Theory]
        [InlineAutoDomainData]
        public async Task CreateAsync_WhenSenderIsSystemOperator_ReturnsEmptyList(
            ChargeLinksRejectedEvent rejectedEvent,
            AvailableChargeLinksRejectionDataFactory sut)
        {
            // Arrange
            rejectedEvent.ChargeLinksCommand.Document.Sender.BusinessProcessRole = MarketParticipantRole.SystemOperator;

            // Act
            var actualList = await sut.CreateAsync(rejectedEvent);

            // Assert
            actualList.Should().BeEmpty();
        }

        private static void SetupAvailableChargeLinksReceiptValidationErrorFactory(
            Mock<IAvailableChargeLinksReceiptValidationErrorFactory> availableChargeLinksReceiptValidationErrorFactory,
            ChargeLinksCommand chargeLinksCommand)
        {
            // fake error code and text
            availableChargeLinksReceiptValidationErrorFactory
                .Setup(f => f.Create(It.IsAny<ValidationError>(), chargeLinksCommand, It.IsAny<ChargeLinkDto>()))
                .Returns<ValidationError, ChargeLinksCommand, ChargeLinkDto>((validationError, _, _) =>
                    new AvailableReceiptValidationError(
                        ReasonCode.D01, validationError.ValidationRuleIdentifier.ToString()));
        }
    }
}
