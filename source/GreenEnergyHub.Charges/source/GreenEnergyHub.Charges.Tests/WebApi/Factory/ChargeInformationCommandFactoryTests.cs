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
using System.Linq;
using AutoFixture.Xunit2;
using Energinet.DataHub.Charges.Contracts.Charge;
using FluentAssertions;
using GreenEnergyHub.Charges.Domain.Charges;
using GreenEnergyHub.Charges.Domain.Dtos.SharedDtos;
using GreenEnergyHub.Charges.Infrastructure.Core.Cim.Charges;
using GreenEnergyHub.Charges.WebApi.Factories;
using Moq;
using NodaTime;
using NodaTime.Extensions;
using Xunit;
using Xunit.Categories;
using Resolution = GreenEnergyHub.Charges.Domain.Charges.Resolution;

namespace GreenEnergyHub.Charges.Tests.WebApi.Factory
{
    [UnitTest]
    public class ChargeInformationCommandFactoryTests
    {
        [Theory]
        [InlineAutoData]
        public void Create_CreateChargeV1Dto_ReturnsChargeInformationCommand(
            [Frozen] Mock<IClock> clock,
            CreateChargeV1Dto charge,
            string meteringPointAdministratorGln)
        {
            // Arrange
            var sut = new ChargeInformationCommandFactory(clock.Object, meteringPointAdministratorGln);

            // Act
            var actual = sut.Create(charge);

            // Assert
            actual.Document.Should().NotBeNull();
            actual.Operations.Should().NotBeNull();
            actual.Operations.Should().HaveCount(1);

            var senderMarketParticipant = charge.SenderMarketParticipant;

            var actualDocument = actual.Document;
            actualDocument.Sender.MarketParticipantId.Should()
                .Be(senderMarketParticipant.MarketParticipantId);
            actualDocument.Sender.BusinessProcessRole.Should()
                .Be(MarketParticipantRoleMapper.Map(senderMarketParticipant.BusinessProcessRole));
            actualDocument.RequestDate.Should().Be(clock.Object.GetCurrentInstant());
            actualDocument.CreatedDateTime.Should().Be(clock.Object.GetCurrentInstant());
            actualDocument.Type.Should().Be(DocumentType.RequestChangeOfPriceList);
            actualDocument.Sender.MarketParticipantId.Should()
                .Be(senderMarketParticipant.MarketParticipantId);
            actualDocument.Sender.BusinessProcessRole.Should()
                .Be(MarketParticipantRoleMapper.Map(senderMarketParticipant.BusinessProcessRole));
            actualDocument.Sender.B2CActorId.Should().Be(Guid.Empty);
            actualDocument.Recipient.BusinessProcessRole.Should().Be(MarketParticipantRole.MeteringPointAdministrator);
            actualDocument.Recipient.MarketParticipantId.Should().Be(meteringPointAdministratorGln);
            actualDocument.Recipient.B2CActorId.Should().Be(Guid.Empty);
            actualDocument.IndustryClassification.Should().Be(IndustryClassification.Electricity);
            actualDocument.BusinessReasonCode.Should().Be(BusinessReasonCode.UpdateChargeInformation);

            var actualOperation = actual.Operations.Single();
            actualOperation.Resolution.Should().Be((Resolution)charge.Resolution);
            actualOperation.ChargeDescription.Should().Be(charge.Description);
            actualOperation.ChargeName.Should().Be(charge.ChargeName);
            actualOperation.ChargeOwner.Should().Be(senderMarketParticipant.MarketParticipantId);
            actualOperation.StartDateTime.Should().Be(charge.EffectiveDate.UtcDateTime.ToInstant());
            actualOperation.SenderProvidedChargeId.Should().Be(charge.SenderProvidedChargeId);
            actualOperation.EndDateTime.Should().BeNull();
            actualOperation.TaxIndicator.Should()
                .Be(charge.TaxIndicator ? TaxIndicator.Tax : TaxIndicator.NoTax);
            actualOperation.TransparentInvoicing.Should()
                .Be(charge.TransparentInvoicing ?
                    TransparentInvoicing.Transparent : TransparentInvoicing.NonTransparent);
        }
    }
}
