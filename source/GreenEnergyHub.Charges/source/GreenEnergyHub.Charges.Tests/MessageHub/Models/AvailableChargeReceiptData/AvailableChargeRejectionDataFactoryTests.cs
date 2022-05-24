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
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommandRejectedEvents;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommands;
using GreenEnergyHub.Charges.Domain.Dtos.Validation;
using GreenEnergyHub.Charges.Domain.MarketParticipants;
using GreenEnergyHub.Charges.Infrastructure.Core.Cim;
using GreenEnergyHub.Charges.Infrastructure.Core.Cim.MarketDocument;
using GreenEnergyHub.Charges.MessageHub.Models.AvailableChargeReceiptData;
using GreenEnergyHub.Charges.MessageHub.Models.AvailableData;
using GreenEnergyHub.Charges.TestCore.Attributes;
using GreenEnergyHub.Charges.Tests.Builders.Command;
using GreenEnergyHub.Charges.Tests.Builders.Testables;
using GreenEnergyHub.TestHelpers;
using Microsoft.Extensions.Logging;
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
            List<ChargeOperationDto> chargeOperations,
            ChargeCommandBuilder chargeCommandBuilder,
            Instant now,
            AvailableChargeRejectionDataFactory sut)
        {
            // Arrange
            var chargeCommand = chargeCommandBuilder.WithChargeOperations(chargeOperations).Build();
            messageMetaDataContext.Setup(m => m.RequestDataTime).Returns(now);

            marketParticipantRepository
                .Setup(r => r.GetMeteringPointAdministratorAsync())
                .ReturnsAsync(meteringPointAdministrator);

            SetupAvailableChargeReceiptValidationErrorFactory(
                availableChargeReceiptValidationErrorFactory, chargeCommand);

            var validationErrors = chargeCommand.ChargeOperations
                .Reverse() // GetReasons() should provide the correct ValidationError no matter what order they have here
                .Select(x => new ValidationError(ValidationRuleIdentifier.SenderIsMandatoryTypeValidation, x.Id, null))
                .ToList();

            var chargeCommandRejectedEvent =
                new ChargeCommandRejectedEvent(now, chargeCommand, validationErrors);

            // Act
            var actualList = await sut.CreateAsync(chargeCommandRejectedEvent);

            // Assert
            actualList.Should().HaveSameCount(chargeOperations);
            var operationOrder = -1;

            for (var i1 = 0; i1 < actualList.Count; i1++)
            {
                var actual = actualList[i1];
                actual.RecipientId.Should().Be(chargeCommand.Document.Sender.Id);
                actual.RecipientRole.Should().Be(chargeCommand.Document.Sender.BusinessProcessRole);
                actual.BusinessReasonCode.Should().Be(chargeCommand.Document.BusinessReasonCode);
                actual.RequestDateTime.Should().Be(now);
                actual.ReceiptStatus.Should().Be(ReceiptStatus.Rejected);
                actual.DocumentType.Should().Be(DocumentType.RejectRequestChangeOfPriceList);

                var expectedChargeOperationDto = chargeCommand.ChargeOperations.ToArray()[i1];
                actual.OriginalOperationId.Should().Be(expectedChargeOperationDto.Id);

                var actualValidationErrors = actual.ValidationErrors.ToList();
                var expectedValidationErrors = validationErrors.Where(x => x.OperationId == expectedChargeOperationDto.Id);
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

        [Theory]
        [InlineAutoDomainData]
        public async Task CreateAsync_WhenCalled_ShouldLogValidationErrors(
            ChargeCommandRejectedEvent rejectedEvent,
            MarketParticipant meteringPointAdministrator,
            [Frozen] Mock<IMessageMetaDataContext> messageMetaDataContext,
            [Frozen] Mock<IAvailableChargeReceiptValidationErrorFactory> availableChargeReceiptValidationErrorFactory,
            [Frozen] Mock<IMarketParticipantRepository> marketParticipantRepository,
            [Frozen] Mock<ILoggerFactory> loggerFactory,
            [Frozen] Mock<ILogger> logger)
        {
            // Arrange
            loggerFactory.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(logger.Object);
            marketParticipantRepository
                .Setup(x => x.GetMeteringPointAdministratorAsync())
                .ReturnsAsync(meteringPointAdministrator);

            SetupAvailableChargeReceiptValidationErrorFactory(
                availableChargeReceiptValidationErrorFactory, rejectedEvent.Command);

            var sut = new AvailableChargeRejectionDataFactory(
                messageMetaDataContext.Object,
                availableChargeReceiptValidationErrorFactory.Object,
                marketParticipantRepository.Object,
                loggerFactory.Object);

            // Act
            await sut.CreateAsync(rejectedEvent);

            // Assert
            var document = rejectedEvent.Command.Document;
            var expectedMessage = $"ValidationErrors for document Id {document.Id} with Type {document.Type} from GLN {document.Sender.Id}:\r\n" +
                                  "- ValidationRuleIdentifier: StartDateValidation\r\n" +
                                  "- ValidationRuleIdentifier: ChangingTariffTaxValueNotAllowed\r\n" +
                                  "- ValidationRuleIdentifier: SenderIsMandatoryTypeValidation\r\n";
            logger.VerifyLoggerWasCalled(expectedMessage, LogLevel.Error);
        }

        private static void SetupAvailableChargeReceiptValidationErrorFactory(
            Mock<IAvailableChargeReceiptValidationErrorFactory> availableChargeReceiptValidationErrorFactory,
            ChargeCommand chargeCommand)
        {
            // fake error code and text
            availableChargeReceiptValidationErrorFactory
                .Setup(f => f.Create(It.IsAny<ValidationError>(), chargeCommand, It.IsAny<ChargeOperationDto>()))
                .Returns<ValidationError, ChargeCommand, ChargeOperationDto>((validationError, _, _) =>
                    new AvailableReceiptValidationError(
                        ReasonCode.D01, validationError.ValidationRuleIdentifier.ToString()));
        }
    }
}
