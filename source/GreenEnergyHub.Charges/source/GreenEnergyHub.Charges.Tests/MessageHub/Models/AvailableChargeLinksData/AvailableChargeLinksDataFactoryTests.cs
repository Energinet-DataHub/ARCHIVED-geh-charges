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
using GreenEnergyHub.Charges.Domain.Charges;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeLinksAcceptedEvents;
using GreenEnergyHub.Charges.Domain.MarketParticipants;
using GreenEnergyHub.Charges.MessageHub.Models.AvailableChargeLinksData;
using GreenEnergyHub.Charges.TestCore.Reflection;
using GreenEnergyHub.Charges.Tests.Builders.Command;
using GreenEnergyHub.Charges.Tests.Builders.Testables;
using GreenEnergyHub.TestHelpers;
using GreenEnergyHub.TestHelpers.FluentAssertionsExtensions;
using Moq;
using NodaTime;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.MessageHub.Models.AvailableChargeLinksData
{
    [UnitTest]
    public class AvailableChargeLinksDataFactoryTests
    {
        [Theory]
        [InlineAutoDomainData]
        public async Task CreateAsync_WhenTaxCharges_ReturnsAvailableData(
            [Frozen] Mock<IMarketParticipantRepository> marketParticipantRepository,
            [Frozen] Mock<IChargeIdentifierFactory> chargeIdentifierFactory,
            [Frozen] Mock<IChargeRepository> chargeRepository,
            [Frozen] Mock<IMessageMetaDataContext> messageMetaDataContext,
            ChargeLinksAcceptedEvent acceptedEvent,
            ChargeBuilder chargeBuilder,
            TestMeteringPointAdministrator meteringPointAdministrator,
            TestGridAccessProvider gridAccessProvider,
            Instant now,
            AvailableChargeLinksDataFactory sut)
        {
            // Arrange
            var charge = chargeBuilder.WithTaxIndicator(TaxIndicator.Tax).Build();
            SetupChargeIdentifierFactoryMock(chargeIdentifierFactory);
            SetupChargeRepositoryMock(chargeRepository, charge);
            SetupMarketParticipantRepositoryMock(
                marketParticipantRepository,
                meteringPointAdministrator,
                gridAccessProvider);

            messageMetaDataContext.Setup(m => m.RequestDataTime).Returns(now);

            var expectedLinks = acceptedEvent.ChargeLinksCommand.Operations.ToList();

            // Act
            var actual = await sut.CreateAsync(acceptedEvent);

            // Assert
            actual.Should().HaveSameCount(expectedLinks);
            for (var i = 0; i < actual.Count; i++)
            {
                actual[i].Should().NotContainNullEnumerable();
                actual[i].RecipientId.Should().Be(gridAccessProvider.MarketParticipantId);
                actual[i].RecipientRole.Should().Be(gridAccessProvider.BusinessProcessRole);
                actual[i].BusinessReasonCode.Should().Be(acceptedEvent.ChargeLinksCommand.Document.BusinessReasonCode);
                actual[i].RequestDateTime.Should().Be(now);
                actual[i].ChargeId.Should().Be(expectedLinks[i].SenderProvidedChargeId);
                actual[i].ChargeOwner.Should().Be(expectedLinks[i].ChargeOwner);
                actual[i].ChargeType.Should().Be(expectedLinks[i].ChargeType);
                actual[i].MeteringPointId.Should().Be(expectedLinks[i].MeteringPointId);
                actual[i].Factor.Should().Be(expectedLinks[i].Factor);
                actual[i].StartDateTime.Should().Be(expectedLinks[i].StartDate);
                actual[i].EndDateTime.Should().Be(expectedLinks[i].EndDate.GetValueOrDefault());
            }
        }

        [Theory]
        [InlineAutoDomainData]
        public async Task CreateAsync_WhenNotTaxCharges_ReturnsEmptyList(
            [Frozen] Mock<IChargeIdentifierFactory> chargeIdentifierFactory,
            [Frozen] Mock<IMarketParticipantRepository> marketParticipantRepository,
            [Frozen] Mock<IChargeRepository> chargeRepository,
            ChargeLinksAcceptedEvent acceptedEvent,
            ChargeBuilder chargeBuilder,
            TestMeteringPointAdministrator meteringPointAdministrator,
            TestGridAccessProvider gridAccessProvider,
            AvailableChargeLinksDataFactory sut)
        {
            // Arrange
            var charge = chargeBuilder.WithTaxIndicator(TaxIndicator.NoTax).Build();
            charge.SetPrivateProperty(c => c.TaxIndicator, false);
            SetupChargeIdentifierFactoryMock(chargeIdentifierFactory);
            SetupChargeRepositoryMock(chargeRepository, charge);
            SetupMarketParticipantRepositoryMock(
                marketParticipantRepository,
                meteringPointAdministrator,
                gridAccessProvider);

            // Act
            var actualList = await sut.CreateAsync(acceptedEvent);

            // Assert
            actualList.Should().BeEmpty();
        }

        private static void SetupChargeRepositoryMock(Mock<IChargeRepository> chargeRepository, Charge charge)
        {
            chargeRepository
                .Setup(r => r.SingleAsync(It.IsAny<ChargeIdentifier>()))
                .ReturnsAsync(charge);
        }

        private static void SetupMarketParticipantRepositoryMock(
            Mock<IMarketParticipantRepository> marketParticipantRepository,
            TestMeteringPointAdministrator meteringPointAdministrator,
            TestGridAccessProvider gridAccessProvider)
        {
            marketParticipantRepository
                .Setup(r => r.GetMeteringPointAdministratorAsync())
                .ReturnsAsync(meteringPointAdministrator);
            marketParticipantRepository
                .Setup(r => r.GetGridAccessProviderAsync(It.IsAny<string>()))
                .ReturnsAsync(gridAccessProvider);
        }

        private static void SetupChargeIdentifierFactoryMock(Mock<IChargeIdentifierFactory> chargeIdentifierFactory)
        {
            chargeIdentifierFactory
                .Setup(x => x.CreateAsync(It.IsAny<string>(), It.IsAny<ChargeType>(), It.IsAny<string>()))
                .ReturnsAsync(It.IsAny<ChargeIdentifier>());
        }
    }
}
