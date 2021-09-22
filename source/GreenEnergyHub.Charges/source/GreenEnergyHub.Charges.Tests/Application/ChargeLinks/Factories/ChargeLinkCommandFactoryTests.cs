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

using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using AutoFixture.Xunit2;
using FluentAssertions;
using GreenEnergyHub.Charges.Application.ChargeLinks.Factories;
using GreenEnergyHub.Charges.Application.Charges.Repositories;
using GreenEnergyHub.Charges.Domain.ChargeLinks.Events.Integration;
using GreenEnergyHub.Charges.Domain.Charges;
using GreenEnergyHub.Charges.Domain.MarketDocument;
using GreenEnergyHub.Charges.TestCore.Attributes;
using Moq;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Application.ChargeLinks.Factories
{
    [UnitTest]
    public class ChargeLinkCommandFactoryTests
    {
        [Theory]
        [InlineAutoMoqData]
        public async Task WhenCreateIsCalled_WithDefaultChargeLinkAndCreateLinkCommandEvent_NewChargeLinkCommandIsCreated(
            [Frozen] [NotNull] Mock<IChargeRepository> chargeRepository,
            [NotNull] DefaultChargeLink defaultChargeLink,
            [NotNull] Charge charge,
            [NotNull] CreateLinkCommandEvent createLinkCommandEvent,
            [NotNull] string correlationId,
            [NotNull] ChargeLinkCommandFactory sut)
        {
            // Arrange
            chargeRepository.Setup(
                    f => f.GetChargeAsync(defaultChargeLink.ChargeId))
                .ReturnsAsync(charge);

            // Act
            var actual = await sut.CreateAsync(createLinkCommandEvent, defaultChargeLink, correlationId)
                .ConfigureAwait(false);

            // Assert
            Assert.NotNull(actual);
            actual.Document.Type.Should().Be(DocumentType.RequestChangeBillingMasterData);
            actual.Document.IndustryClassification.Should().Be(IndustryClassification.Electricity);
            actual.Document.BusinessReasonCode.Should().Be(BusinessReasonCode.UpdateMasterDataSettlement);
            actual.Document.Sender.BusinessProcessRole.Should().Be(MarketParticipantRole.SystemOperator);
            actual.Document.Sender.Id.Should().Be(charge.Owner);
            actual.Document.Recipient.BusinessProcessRole.Should().Be(MarketParticipantRole.MeteringPointAdministrator);
            actual.Document.Recipient.Id.Should().Be("5790001330552");
            actual.ChargeLink.ChargeId.Should().Be(charge.SenderProvidedChargeId);
            actual.ChargeLink.ChargeType.Should().Be(charge.Type);
            actual.ChargeLink.EndDateTime.Should().Be(defaultChargeLink.EndDateTime);
            actual.ChargeLink.ChargeOwner.Should().Be(charge.Owner);
            actual.ChargeLink.MeteringPointId.Should().Be(createLinkCommandEvent.MeteringPointId);
            actual.ChargeLink.StartDateTime.Should().Be(defaultChargeLink.GetStartDateTime(createLinkCommandEvent.StartDateTime));
            actual.ChargeLink.Factor.Should().Be(1);
        }
    }
}
