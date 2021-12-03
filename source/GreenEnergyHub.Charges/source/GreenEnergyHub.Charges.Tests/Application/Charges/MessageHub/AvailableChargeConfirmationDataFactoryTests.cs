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

using System.Threading.Tasks;
using AutoFixture.Xunit2;
using FluentAssertions;
using GreenEnergyHub.Charges.Application;
using GreenEnergyHub.Charges.Application.Charges.MessageHub;
using GreenEnergyHub.Charges.Domain.AvailableData;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommandAcceptedEvents;
using GreenEnergyHub.Charges.TestCore.Attributes;
using Moq;
using NodaTime;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Application.Charges.MessageHub
{
    [UnitTest]
    public class AvailableChargeConfirmationDataFactoryTests
    {
        [Theory]
        [InlineAutoMoqData]
        public async Task CreateAsync_WhenCalledWithAcceptedEvent_ReturnsAvailableData(
            [Frozen] Mock<IMessageMetaDataContext> messageMetaDataContext,
            ChargeCommandAcceptedEvent acceptedEvent,
            Instant now,
            AvailableChargeConfirmationDataFactory sut)
        {
            // Arrange
            messageMetaDataContext.Setup(
                    m => m.RequestDataTime)
                .Returns(now);

            // Act
            var actualList =
                await sut.CreateAsync(acceptedEvent);

            // Assert
            actualList.Should().ContainSingle();
            actualList[0].RecipientId.Should().Be(acceptedEvent.Command.Document.Sender.Id);
            actualList[0].RecipientRole.Should()
                    .Be(acceptedEvent.Command.Document.Sender.BusinessProcessRole);
            actualList[0].BusinessReasonCode.Should()
                    .Be(acceptedEvent.Command.Document.BusinessReasonCode);
            actualList[0].RequestDateTime.Should().Be(now);
            actualList[0].ReceiptStatus.Should().Be(ReceiptStatus.Confirmed);
            actualList[0].OriginalOperationId.Should().Be(acceptedEvent.Command.ChargeOperation.Id);
            actualList[0].ReasonCodes.Should().BeEmpty();
        }
    }
}
