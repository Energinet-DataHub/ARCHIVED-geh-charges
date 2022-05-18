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
using System.Linq;
using System.Threading.Tasks;
using AutoFixture.Xunit2;
using FluentAssertions;
using GreenEnergyHub.Charges.Domain.Charges;
using GreenEnergyHub.Charges.Domain.DefaultChargeLinks;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeLinksCommands;
using GreenEnergyHub.Charges.Domain.Dtos.CreateDefaultChargeLinksRequests;
using GreenEnergyHub.Charges.Domain.MarketParticipants;
using GreenEnergyHub.Charges.Domain.MeteringPoints;
using GreenEnergyHub.Charges.TestCore.Attributes;
using GreenEnergyHub.Charges.Tests.Builders.Testables;
using Moq;
using NodaTime;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Domain.Dtos.ChargeLinksCommands
{
    [UnitTest]
    public class ChargeLinksCommandFactoryTests
    {
        [Theory]
        [InlineAutoMoqData]
        public async Task WhenCreateIsCalled_WithDefaultChargeLinkAndCreateLinkCommandEvent_NewChargeLinkCommandIsCreated(
            [Frozen] Mock<IChargeRepository> chargeRepository,
            [Frozen] Mock<IMeteringPointRepository> meteringPointRepository,
            [Frozen] Mock<IMarketParticipantRepository> marketParticipantRepository,
            TestMeteringPointAdministrator recipient,
            TestSystemOperator systemOperator,
            TestMarketParticipant chargeOwner,
            Guid defaultChargeLinkId,
            Guid chargeId,
            Guid chargePeriodId,
            MeteringPoint meteringPoint,
            CreateDefaultChargeLinksRequest createDefaultChargeLinksRequest,
            ChargeLinksCommandFactory sut)
        {
            // Arrange
            var defaultChargeLink = new DefaultChargeLink(
                defaultChargeLinkId,
                Instant.MinValue,
                Instant.MaxValue,
                chargeId,
                meteringPoint.MeteringPointType);

            var charge = new Charge(
                chargeId,
                "SenderProvidedId",
                chargeOwner.Id,
                ChargeType.Fee,
                Resolution.P1D,
                true,
                new List<Point> { new(1, 200m, SystemClock.Instance.GetCurrentInstant()) },
                new List<ChargePeriod>
                {
                    new ChargePeriod(
                        chargePeriodId,
                        "Name",
                        "Description",
                        VatClassification.Unknown,
                        true,
                        SystemClock.Instance.GetCurrentInstant(),
                        Instant.FromUtc(9999, 12, 31, 23, 59, 59)),
                });

            chargeRepository
                .Setup(f => f.GetByIdsAsync(new List<Guid> { defaultChargeLink.ChargeId }))
                .ReturnsAsync(new List<Charge> { charge });

            meteringPointRepository
                .Setup(f => f.GetMeteringPointAsync(createDefaultChargeLinksRequest.MeteringPointId))
                .ReturnsAsync(meteringPoint);

            marketParticipantRepository
                .Setup(m => m.GetSystemOperatorAsync())
                .ReturnsAsync(systemOperator);

            marketParticipantRepository
                .Setup(m => m.GetAsync(new List<Guid> { charge.OwnerId }))
                .ReturnsAsync(new List<MarketParticipant> { chargeOwner });

            marketParticipantRepository
                .Setup(m => m.GetMeteringPointAdministratorAsync())
                .ReturnsAsync(recipient);

            // Act
            var actual = await sut
                .CreateAsync(createDefaultChargeLinksRequest, new List<DefaultChargeLink> { defaultChargeLink })
                .ConfigureAwait(false);

            // Assert
            Assert.NotNull(actual);
            actual.Document.Type.Should().Be(DocumentType.RequestChangeBillingMasterData);
            actual.Document.IndustryClassification.Should().Be(IndustryClassification.Electricity);
            actual.Document.BusinessReasonCode.Should().Be(BusinessReasonCode.UpdateMasterDataSettlement);
            actual.Document.Sender.BusinessProcessRole.Should().Be(MarketParticipantRole.SystemOperator);
            actual.Document.Sender.Id.Should().Be(systemOperator.MarketParticipantId);
            actual.Document.Recipient.BusinessProcessRole.Should().Be(MarketParticipantRole.MeteringPointAdministrator);
            actual.Document.Recipient.Id.Should().Be(recipient.MarketParticipantId);
            actual.ChargeLinksOperations.First().SenderProvidedChargeId.Should().Be(charge.SenderProvidedChargeId);
            actual.ChargeLinksOperations.First().ChargeType.Should().Be(charge.Type);
            actual.ChargeLinksOperations.First().EndDateTime.Should().Be(defaultChargeLink.EndDateTime);
            actual.ChargeLinksOperations.First().ChargeOwner.Should().Be(chargeOwner.MarketParticipantId);
            actual.ChargeLinksOperations.First().MeteringPointId.Should().Be(meteringPoint.MeteringPointId);
            actual.ChargeLinksOperations.First().StartDateTime.Should().Be(defaultChargeLink.GetStartDateTime(meteringPoint.EffectiveDate));
            actual.ChargeLinksOperations.First().Factor.Should().Be(1);
        }
    }
}
