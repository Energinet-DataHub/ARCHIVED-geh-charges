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
using GreenEnergyHub.Charges.Application.Messaging;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeLinksCommands;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeLinksRejectionEvents;
using GreenEnergyHub.Charges.Domain.Dtos.Validation;
using GreenEnergyHub.Charges.Domain.MarketParticipants;
using GreenEnergyHub.Charges.Infrastructure.Core.Cim;
using GreenEnergyHub.Charges.Infrastructure.Core.Cim.MarketDocument;
using GreenEnergyHub.Charges.MessageHub.Models.AvailableChargeLinksReceiptData;
using GreenEnergyHub.Charges.MessageHub.Models.AvailableData;
using GreenEnergyHub.TestHelpers;
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
            ChargeLinksRejectedEvent rejectedEvent,
            Instant now,
            AvailableChargeLinksRejectionDataFactory sut)
        {
            // Arrange
            rejectedEvent.ChargeLinksCommand.Document.Sender.BusinessProcessRole = marketParticipantRole;
            messageMetaDataContext.Setup(m => m.RequestDataTime).Returns(now);
            var expectedLinks = rejectedEvent.ChargeLinksCommand.ChargeLinksOperations.ToList();
            marketParticipantRepository
                .Setup(r => r.GetMeteringPointAdministratorAsync())
                .ReturnsAsync(meteringPointAdministrator);

            // fake error code and text
            availableChargeLinksReceiptValidationErrorFactory
                .Setup(f => f.Create(It.IsAny<ValidationError>(), rejectedEvent.ChargeLinksCommand, It.IsAny<ChargeLinkDto>()))
                .Returns<ValidationError, ChargeLinksCommand, ChargeLinkDto>((validationError, _, _) =>
                    new AvailableReceiptValidationError(
                        ReasonCode.D01, validationError.ValidationRuleIdentifier.ToString()));

            var expectedValidationErrors = rejectedEvent.ValidationErrors
                .Select(x => x.ValidationRuleIdentifier.ToString()).ToList();

            // Act
            var actualList = await sut.CreateAsync(rejectedEvent);

            // Assert
            actualList.Should().HaveSameCount(expectedLinks);
            for (var i1 = 0; i1 < actualList.Count; i1++)
            {
                actualList[i1].RecipientId.Should().Be(rejectedEvent.ChargeLinksCommand.Document.Sender.Id);
                actualList[i1].RecipientRole.Should()
                    .Be(rejectedEvent.ChargeLinksCommand.Document.Sender.BusinessProcessRole);
                actualList[i1].BusinessReasonCode.Should()
                    .Be(rejectedEvent.ChargeLinksCommand.Document.BusinessReasonCode);
                actualList[i1].RequestDateTime.Should().Be(now);
                actualList[i1].ReceiptStatus.Should().Be(ReceiptStatus.Rejected);
                actualList[i1].DocumentType.Should().Be(DocumentType.RejectRequestChangeBillingMasterData);
                actualList[i1].OriginalOperationId.Should().Be(expectedLinks[i1].OperationId);
                actualList[i1].MeteringPointId.Should().Be(expectedLinks[i1].MeteringPointId);
                var actualValidationErrors = actualList[i1].ValidationErrors.ToList();

                for (var i2 = 0; i2 < actualValidationErrors.Count; i2++)
                {
                    actualValidationErrors[i2].ReasonCode.ToString().Should().Be("D01");
                    actualValidationErrors[i2].Text.Should().Be(expectedValidationErrors[i2]);
                }
            }
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
    }
}
