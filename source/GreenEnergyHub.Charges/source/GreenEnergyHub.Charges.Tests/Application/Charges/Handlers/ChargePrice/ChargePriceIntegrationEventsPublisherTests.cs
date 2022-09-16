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
using GreenEnergyHub.Charges.Application.Charges.Handlers.ChargePrice;
using GreenEnergyHub.Charges.Domain.Dtos.ChargePriceCommands;
using GreenEnergyHub.Charges.Tests.Builders.Command;
using GreenEnergyHub.TestHelpers;
using Moq;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Application.Charges.Handlers.ChargePrice
{
    [UnitTest]
    public class ChargePriceIntegrationEventsPublisherTests
    {
        [Theory]
        [InlineAutoDomainData]
        public async Task HandleAsync_WhenCalledWithPrices_ShouldCallChargePricesSenderForEachOperation(
            [Frozen] Mock<IChargePricesUpdatedPublisher> chargePricesUpdatedSender,
            ChargePriceOperationDtoBuilder chargePriceOperationDtoBuilder,
            ChargePriceOperationsAcceptedEventBuilder chargePriceOperationsAcceptedEventBuilder,
            ChargePriceIntegrationEventsPublisher sut)
        {
            // Arrange
            var operations = new List<ChargePriceOperationDto>
            {
                    chargePriceOperationDtoBuilder.Build(),
                    chargePriceOperationDtoBuilder.Build(),
                    chargePriceOperationDtoBuilder.Build(),
            };

            var chargePriceOperationsConfirmedEvent = chargePriceOperationsAcceptedEventBuilder.WithOperations(operations).Build();

            // Act
            await sut.PublishAsync(chargePriceOperationsConfirmedEvent).ConfigureAwait(false);

            // Assert
            chargePricesUpdatedSender.Verify(
                x => x.PublishChargePricesAsync(It.IsAny<ChargePriceOperationDto>()),
                Times.Exactly(operations.Count));
        }
    }
}
