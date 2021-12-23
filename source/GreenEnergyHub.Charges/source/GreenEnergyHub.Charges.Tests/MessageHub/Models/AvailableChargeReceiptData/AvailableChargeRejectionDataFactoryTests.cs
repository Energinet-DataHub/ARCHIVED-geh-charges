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
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommandRejectedEvents;
using GreenEnergyHub.Charges.Infrastructure.Core;
using GreenEnergyHub.Charges.Infrastructure.Core.Cim.MarketDocument;
using GreenEnergyHub.Charges.Infrastructure.Core.MessageMetaData;
using GreenEnergyHub.Charges.MessageHub.Models.AvailableChargeReceiptData;
using GreenEnergyHub.Charges.TestCore.Attributes;
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
            [Frozen] Mock<IMessageMetaDataContext> messageMetaDataContext,
            ChargeCommandRejectedEvent rejectedEvent,
            Instant now,
            AvailableChargeRejectionDataFactory sut)
        {
            // Arrange
            messageMetaDataContext.Setup(m => m.RequestDataTime).Returns(now);

            // Act
            var actualList = await sut.CreateAsync(rejectedEvent);

            // Assert
            actualList.Should().ContainSingle();
            var actual = actualList.Single();
            actual.RecipientId.Should().Be(rejectedEvent.Command.Document.Sender.Id);
            actual.RecipientRole.Should()
                .Be(rejectedEvent.Command.Document.Sender.BusinessProcessRole);
            actual.BusinessReasonCode.Should()
                .Be(rejectedEvent.Command.Document.BusinessReasonCode);
            actual.RequestDateTime.Should().Be(now);
            actual.ReceiptStatus.Should().Be(ReceiptStatus.Rejected);
            actual.OriginalOperationId.Should().Be(rejectedEvent.Command.ChargeOperation.Id);
            actual.ValidationErrors.Should().HaveSameCount(rejectedEvent.ValidationErrors);

            var actualReasons = actualList[0].ValidationErrors.ToList();
            var expectedReasons = rejectedEvent.ValidationErrors.ToList();

            for (var i = 0; i < actualReasons.Count; i++)
            {
                actualReasons[i].ReasonCode.Should().Be(ReasonCode.IncorrectChargeInformation);
                // actualReasons[i].Text.Should().Be(expectedReasons[i]); //TODO BJARKE
            }
        }
    }
}
