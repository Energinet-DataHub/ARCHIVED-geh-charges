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

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.AutoMoq;
using AutoFixture.Xunit2;
using FluentAssertions;
using GreenEnergyHub.Charges.Domain.ChargeLinks;
using GreenEnergyHub.Charges.Domain.Charges;
using GreenEnergyHub.Charges.Domain.DefaultChargeLinks;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeLinkCommands;
using GreenEnergyHub.Charges.Domain.Dtos.CreateLinkRequest;
using GreenEnergyHub.Charges.Domain.MarketParticipants;
using GreenEnergyHub.Charges.Domain.MeteringPoints;
using GreenEnergyHub.Charges.TestCore.Attributes;
using Moq;
using NodaTime;
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
            [Frozen] Mock<IChargeRepository> chargeRepository,
            [Frozen] Mock<IMeteringPointRepository> meteringPointRepository,
            [Frozen] Mock<IMarketParticipantRepository> marketParticipantRepository,
            MarketParticipant systemOperator,
            Guid chargeId,
            MeteringPoint meteringPoint,
            CreateLinkCommandEvent createLinkCommandEvent,
            ChargeLinkCommandFactory sut)
        {
            // Arrange
            var defaultChargeLink = new DefaultChargeLink(
                Instant.MinValue,
                Instant.MaxValue,
                chargeId,
                meteringPoint.MeteringPointType);

            var charge = new Charge(
                chargeId,
                "ChargeOperationId",
                "SenderProvidedId",
                "Name",
                "description",
                systemOperator.Id,
                SystemClock.Instance.GetCurrentInstant(),
                Instant.FromUtc(9999, 12, 31, 23, 59, 59),
                ChargeType.Fee,
                VatClassification.Unknown,
                Resolution.P1D,
                true,
                false,
                new List<Point>
                {
                    new Point { Position = 0, Time = SystemClock.Instance.GetCurrentInstant(), Price = 200m },
                });

            chargeRepository.Setup(
                    f => f.GetChargesAsync(new List<Guid> { defaultChargeLink.ChargeId }))
                .ReturnsAsync(new List<Charge> { charge });

            meteringPointRepository.Setup(
                    f => f.GetMeteringPointAsync(createLinkCommandEvent.MeteringPointId))
                .ReturnsAsync(meteringPoint);

            marketParticipantRepository
                .Setup(m => m.GetSystemOperatorAsync()).ReturnsAsync(systemOperator);

            // Act
            var actual =
                await sut.CreateAsync(
                        createLinkCommandEvent,
                        new List<DefaultChargeLink> { defaultChargeLink })
                .ConfigureAwait(false);

            // Assert
            Assert.NotNull(actual);
            actual.Document.Type.Should().Be(DocumentType.RequestChangeBillingMasterData);
            actual.Document.IndustryClassification.Should().Be(IndustryClassification.Electricity);
            actual.Document.BusinessReasonCode.Should().Be(BusinessReasonCode.UpdateMasterDataSettlement);
            actual.Document.Sender.BusinessProcessRole.Should().Be(MarketParticipantRole.SystemOperator);
            actual.Document.Sender.Id.Should().Be(systemOperator.Id);
            actual.Document.Recipient.BusinessProcessRole.Should().Be(MarketParticipantRole.MeteringPointAdministrator);
            actual.Document.Recipient.Id.Should().Be("5790001330552");
            actual.ChargeLinks.First().SenderProvidedChargeId.Should().Be(charge.SenderProvidedChargeId);
            actual.ChargeLinks.First().ChargeType.Should().Be(charge.Type);
            actual.ChargeLinks.First().EndDateTime.Should().Be(defaultChargeLink.EndDateTime);
            actual.ChargeLinks.First().ChargeOwner.Should().Be(charge.Owner);
            actual.MeteringPointId.Should().Be(createLinkCommandEvent.MeteringPointId);
            actual.ChargeLinks.First().StartDateTime.Should().Be(defaultChargeLink.GetStartDateTime(meteringPoint.EffectiveDate));
            actual.ChargeLinks.First().Factor.Should().Be(1);
        }
    }
}
