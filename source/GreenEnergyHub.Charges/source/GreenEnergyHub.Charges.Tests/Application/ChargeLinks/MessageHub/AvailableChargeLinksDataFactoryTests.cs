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
using GreenEnergyHub.Charges.Application;
using GreenEnergyHub.Charges.Domain.Charges;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeLinksAcceptedEvents;
using GreenEnergyHub.Charges.Domain.MarketParticipants;
using GreenEnergyHub.Charges.Infrastructure.Core.MessageMetaData;
using GreenEnergyHub.Charges.MessageHub.Application.ChargeLinks;
using GreenEnergyHub.Charges.TestCore.Reflection;
using GreenEnergyHub.TestHelpers;
using GreenEnergyHub.TestHelpers.FluentAssertionsExtensions;
using Moq;
using NodaTime;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Application.ChargeLinks.MessageHub
{
    [UnitTest]
    public class AvailableChargeLinksDataFactoryTests
    {
        [Theory]
        [InlineAutoDomainData]
        public async Task CreateAsync_WhenTaxCharges_ReturnsAvailableData(
            [Frozen] Mock<IMarketParticipantRepository> marketParticipantRepository,
            [Frozen] Mock<IChargeRepository> chargeRepository,
            [Frozen] Mock<IMessageMetaDataContext> messageMetaDataContext,
            ChargeLinksAcceptedEvent acceptedEvent,
            Charge charge,
            MarketParticipant marketParticipant,
            Instant now,
            AvailableChargeLinksDataFactory sut)
        {
            // Arrange
            marketParticipantRepository.Setup(
                    m => m.GetGridAccessProvider(acceptedEvent.ChargeLinksCommand.MeteringPointId))
                .Returns(marketParticipant);

            charge.SetPrivateProperty(c => c.TaxIndicator, true);
            chargeRepository.Setup(r => r.GetAsync(It.IsAny<ChargeIdentifier>()))
                .ReturnsAsync(charge);

            messageMetaDataContext.Setup(
                    m => m.RequestDataTime)
                .Returns(now);

            var expectedLinks = acceptedEvent.ChargeLinksCommand.ChargeLinks.ToList();

            // Act
            var actualList =
                await sut.CreateAsync(acceptedEvent);

            // Assert
            actualList.Should().HaveSameCount(expectedLinks);
            for (var i = 0; i < actualList.Count; i++)
            {
                actualList[i].Should().NotContainNullsOrEmptyEnumerables();
                actualList[i].RecipientId.Should().Be(marketParticipant.MarketParticipantId);
                actualList[i].RecipientRole.Should().Be(marketParticipant.BusinessProcessRole);
                actualList[i].BusinessReasonCode.Should()
                    .Be(acceptedEvent.ChargeLinksCommand.Document.BusinessReasonCode);
                actualList[i].RequestDateTime.Should().Be(now);
                actualList[i].ChargeId.Should().Be(expectedLinks[i].SenderProvidedChargeId);
                actualList[i].ChargeOwner.Should().Be(expectedLinks[i].ChargeOwnerId);
                actualList[i].ChargeType.Should().Be(expectedLinks[i].ChargeType);
                actualList[i].MeteringPointId.Should().Be(acceptedEvent.ChargeLinksCommand.MeteringPointId);
                actualList[i].Factor.Should().Be(expectedLinks[i].Factor);
                actualList[i].StartDateTime.Should().Be(expectedLinks[i].StartDateTime);
                actualList[i].EndDateTime.Should().Be(expectedLinks[i].EndDateTime.GetValueOrDefault());
            }
        }

        [Theory]
        [InlineAutoDomainData]
        public async Task CreateAsync_WhenNotTaxCharges_ReturnsEmptyList(
            [Frozen] Mock<IMarketParticipantRepository> marketParticipantRepository,
            [Frozen] Mock<IChargeRepository> chargeRepository,
            ChargeLinksAcceptedEvent acceptedEvent,
            Charge charge,
            MarketParticipant marketParticipant,
            AvailableChargeLinksDataFactory sut)
        {
            // Arrange
            marketParticipantRepository.Setup(
                    m => m.GetGridAccessProvider(acceptedEvent.ChargeLinksCommand.MeteringPointId))
                .Returns(marketParticipant);

            charge.SetPrivateProperty(c => c.TaxIndicator, false);
            chargeRepository
                .Setup(r => r.GetAsync(It.IsAny<ChargeIdentifier>()))
                .ReturnsAsync(charge);

            // Act
            var actualList =
                await sut.CreateAsync(acceptedEvent);

            // Assert
            actualList.Should().BeEmpty();
        }
    }
}
