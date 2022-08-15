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
using System.Threading.Tasks;
using AutoFixture.Xunit2;
using GreenEnergyHub.Charges.Application.Charges.Acknowledgement;
using GreenEnergyHub.Charges.Application.Charges.Handlers;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeInformationCommands;
using GreenEnergyHub.Charges.Domain.Dtos.SharedDtos;
using GreenEnergyHub.Charges.Tests.Builders.Command;
using GreenEnergyHub.TestHelpers;
using Moq;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Application.Charges.Handlers
{
    [UnitTest]
    public class ChargeIntegrationEventsPublisherTests
    {
        [Theory]
        [InlineAutoDomainData]
        public async Task HandleAsync_WhenCalledWithPrices_ShouldCallChargePricesSender(
            [Frozen] Mock<IChargePublisher> chargeSender,
            [Frozen] Mock<IChargePricesUpdatedPublisher> chargePricesUpdatedSender,
            DocumentDtoBuilder documentDtoBuilder,
            List<ChargeInformationOperationDto> chargeOperations,
            ChargeInformationCommandBuilder chargeInformationCommandBuilder,
            ChargeCommandAcceptedEventBuilder chargeCommandAcceptedEventBuilder,
            ChargeIntegrationEventsPublisher sut)
        {
            // Arrange
            var document = documentDtoBuilder.WithBusinessReasonCode(BusinessReasonCode.UpdateChargePrices).Build();
            var chargeCommand = chargeInformationCommandBuilder.WithDocumentDto(document).WithChargeOperations(chargeOperations).Build();
            var acceptedEvent = chargeCommandAcceptedEventBuilder.WithChargeCommand(chargeCommand).Build();

            // Act
            await sut.PublishAsync(acceptedEvent).ConfigureAwait(false);

            // Assert
            chargeSender.Verify(x => x.PublishChargeCreatedAsync(It.IsAny<ChargeInformationOperationDto>()), Times.Never);
            chargePricesUpdatedSender.Verify(x => x.PublishChargePricesAsync(It.IsAny<ChargeInformationOperationDto>()), Times.Exactly(3));
        }

        [Theory]
        [InlineAutoDomainData]
        public async Task HandleAsync_WhenCalledWithoutPrices_ShouldOnlyCallChargeCreatedSender(
            [Frozen] Mock<IChargePublisher> chargeSender,
            [Frozen] Mock<IChargePricesUpdatedPublisher> chargePricesUpdatedSender,
            DocumentDtoBuilder documentDtoBuilder,
            ChargeInformationCommandBuilder chargeInformationCommandBuilder,
            ChargeCommandAcceptedEventBuilder chargeCommandAcceptedEventBuilder,
            ChargeIntegrationEventsPublisher sut)
        {
            // Arrange
            var document = documentDtoBuilder.WithBusinessReasonCode(BusinessReasonCode.UpdateChargeInformation).Build();
            var chargeCommand = chargeInformationCommandBuilder.WithDocumentDto(document).Build();
            var acceptedEvent = chargeCommandAcceptedEventBuilder.WithChargeCommand(chargeCommand).Build();

            // Act
            await sut.PublishAsync(acceptedEvent).ConfigureAwait(false);

            // Assert
            chargeSender.Verify(x => x.PublishChargeCreatedAsync(It.IsAny<ChargeInformationOperationDto>()), Times.Once);
            chargePricesUpdatedSender.Verify(x => x.PublishChargePricesAsync(It.IsAny<ChargeInformationOperationDto>()), Times.Never);
        }
    }
}
