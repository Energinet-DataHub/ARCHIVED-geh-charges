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
        public void Create_ReturnsChargeInformationCommand_BasedOnCreateChargeInformationV1Dto(
            [Frozen] Mock<IClock> clock,
            CreateChargeInformationV1Dto chargeInformation)
        {
            // Arrange
            var sut = new ChargeInformationCommandFactory(clock.Object);

            // Act
            var actual = sut.Create(chargeInformation);

            // Assert
            actual.Document.Should().NotBeNull();
            actual.Operations.Should().NotBeNull();
            actual.Operations.Should().HaveCount(1);

            var senderMarketParticipant = chargeInformation.SenderMarketParticipant;

            var actualDocument = actual.Document;
            actualDocument.Sender.MarketParticipantId.Should()
                .Be(senderMarketParticipant.MarketParticipantId);
            actualDocument.Sender.BusinessProcessRole.Should()
                .Be(MarketParticipantRoleMapper.Map(senderMarketParticipant.BusinessProcessRole));
            actualDocument.RequestDate.Should().Be(clock.Object.GetCurrentInstant());
            actualDocument.CreatedDateTime.Should().Be(clock.Object.GetCurrentInstant());
            actualDocument.Type.Should().Be(DocumentType.RejectRequestChangeOfPriceList);
            actualDocument.Sender.MarketParticipantId.Should()
                .Be(senderMarketParticipant.MarketParticipantId);
            actualDocument.Sender.BusinessProcessRole.Should()
                .Be(MarketParticipantRoleMapper.Map(senderMarketParticipant.BusinessProcessRole));
            actualDocument.Sender.B2CActorId.Should().Be(Guid.Empty);
            actualDocument.Recipient.BusinessProcessRole.Should().Be(MarketParticipantRole.MeteringPointAdministrator);
            actualDocument.Recipient.MarketParticipantId.Should().Be(MarketParticipantConstants.MeteringPointAdministratorGln);
            actualDocument.Recipient.B2CActorId.Should().Be(Guid.Empty);
            actualDocument.IndustryClassification.Should().Be(IndustryClassification.Electricity);
            actualDocument.BusinessReasonCode.Should().Be(BusinessReasonCode.UpdateChargeInformation);

            var actualOperation = actual.Operations.First();
            actualOperation.Resolution.Should().Be((Resolution)chargeInformation.Resolution);
            actualOperation.ChargeDescription.Should().Be(chargeInformation.Description);
            actualOperation.ChargeName.Should().Be(chargeInformation.ChargeName);
            actualOperation.ChargeOwner.Should().Be(senderMarketParticipant.MarketParticipantId);
            actualOperation.StartDateTime.Should().Be(chargeInformation.EffectiveDate.UtcDateTime.ToInstant());
            actualOperation.SenderProvidedChargeId.Should().Be(chargeInformation.SenderProvidedChargeId);
            actualOperation.EndDateTime.Should().BeNull();
            actualOperation.TaxIndicator.Should()
                .Be(chargeInformation.TaxIndicator ? TaxIndicator.Tax : TaxIndicator.NoTax);
            actualOperation.TransparentInvoicing.Should()
                .Be(chargeInformation.TransparentInvoicing ?
                    TransparentInvoicing.Transparent : TransparentInvoicing.NonTransparent);
        }
    }
}
