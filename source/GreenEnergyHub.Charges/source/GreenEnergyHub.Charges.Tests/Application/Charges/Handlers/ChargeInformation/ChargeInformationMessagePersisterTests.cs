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

using System.Collections.Generic;
using System.Threading.Tasks;
using AutoFixture.Xunit2;
using GreenEnergyHub.Charges.Application.Charges.Handlers.ChargeInformation;
using GreenEnergyHub.Charges.Domain.Charges;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeInformationCommands;
using GreenEnergyHub.Charges.TestCore.Builders.Command;
using GreenEnergyHub.TestHelpers;
using Moq;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Application.Charges.Handlers.ChargeInformation
{
    [UnitTest]
    public class ChargeInformationMessagePersisterTests
    {
        [Theory]
        [InlineAutoDomainData]
        public async Task PersistMessageAsync_WhenCalled_ShouldCallChargeMessageRepository(
            [Frozen] Mock<IChargeMessageRepository> chargeMessageRepository,
            ChargeInformationOperationDtoBuilder chargeInformationOperationDtoBuilder,
            ChargeInformationOperationsAcceptedEventBuilder chargeInformationOperationsAcceptedEventBuilder,
            ChargeInformationMessagePersister sut)
        {
            // Arrange
            var acceptedEvent = chargeInformationOperationsAcceptedEventBuilder
                .WithOperations(new List<ChargeInformationOperationDto>
                    {
                        chargeInformationOperationDtoBuilder.Build(),
                        chargeInformationOperationDtoBuilder.Build(),
                    })
                .Build();

            // Act
            await sut.PersistMessageAsync(acceptedEvent).ConfigureAwait(false);

            // Assert
            chargeMessageRepository.Verify(x => x.AddAsync(It.IsAny<ChargeMessage>()), Times.Once());
        }
    }
}
