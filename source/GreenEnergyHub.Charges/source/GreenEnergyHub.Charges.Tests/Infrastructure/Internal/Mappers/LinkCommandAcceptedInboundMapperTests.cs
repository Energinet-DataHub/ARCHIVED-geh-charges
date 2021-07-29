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
using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using GreenEnergyHub.Charges.Domain.ChargeLinks;
using GreenEnergyHub.Charges.Infrastructure.Internal.ChargeLinkCommandAccepted;
using GreenEnergyHub.Charges.Infrastructure.Internal.Mappers;
using GreenEnergyHub.Charges.TestCore;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Infrastructure.Internal.Mappers
{
    [UnitTest]
    public class LinkCommandAcceptedInboundMapperTests
    {
        [Theory]
        [InlineAutoMoqData]
        public void Convert_WhenCalled_ShouldMapToDomainObjectWithCorrectValues(
            [NotNull] ChargeLinkCommandAcceptedContract acceptedCommand,
            [NotNull] LinkCommandAcceptedInboundMapper sut)
        {
            // Arrange

            // Act
            var converted = (ChargeLinkCommandAcceptedEvent)sut.Convert(acceptedCommand);

            // Assert
            converted.Document.Id.Should().BeEquivalentTo(acceptedCommand.Document.Id);
            converted.Document.RequestDate.ToUnixTimeSeconds().Should()
                .Be(acceptedCommand.Document.RequestDate.Seconds);
            converted.Document.Type.Should().BeEquivalentTo(acceptedCommand.Document.Type);
            converted.Document.CreatedDateTime.ToUnixTimeSeconds().Should()
                .Be(acceptedCommand.Document.CreatedDateTime.Seconds);
            converted.Document.Sender.Id.Should().BeEquivalentTo(acceptedCommand.Document.Sender.Id);
            converted.Document.Sender.BusinessProcessRole.Should().BeEquivalentTo(acceptedCommand.Document.Sender.MarketParticipantRole);
            converted.Document.Recipient.Id.Should().BeEquivalentTo(acceptedCommand.Document.Recipient.Id);
            converted.Document.Recipient.BusinessProcessRole.Should().BeEquivalentTo(acceptedCommand.Document.Recipient.MarketParticipantRole);
            converted.Document.IndustryClassification.Should().BeEquivalentTo(acceptedCommand.Document.IndustryClassification);
            converted.Document.BusinessReasonCode.Should().BeEquivalentTo(acceptedCommand.Document.BusinessReasonCode);
            converted.ChargeLink.Id.Should().BeEquivalentTo(acceptedCommand.ChargeLink.Id);
            converted.ChargeLink.MeteringPointId.Should().BeEquivalentTo(acceptedCommand.ChargeLink.MeteringPointId);
            converted.ChargeLink.StartDateTime.ToUnixTimeSeconds().Should()
                .Be(acceptedCommand.ChargeLink.StartDateTime.Seconds);
            Assert.NotNull(converted.ChargeLink.EndDateTime);
            converted.ChargeLink.EndDateTime!.Value.ToUnixTimeSeconds().Should()
                .Be(acceptedCommand.ChargeLink.EndDateTime.Seconds);
            converted.ChargeLink.ChargeId.Should().BeEquivalentTo(acceptedCommand.ChargeLink.ChargeId);
            converted.ChargeLink.Factor.Should().Be(acceptedCommand.ChargeLink.Factor);
            converted.ChargeLink.ChargeOwner.Should().BeEquivalentTo(acceptedCommand.ChargeLink.ChargeOwner);
            converted.ChargeLink.ChargeType.Should().BeEquivalentTo(acceptedCommand.ChargeLink.ChargeType);
        }

        [Fact]
        public void Convert_WhenCalledWithNull_ShouldThrow()
        {
            var mapper = new LinkCommandAcceptedInboundMapper();
            ChargeLinkCommandAcceptedContract? chargeLinkCommandAcceptedContract = null;
            Assert.Throws<InvalidOperationException>(() => mapper.Convert(chargeLinkCommandAcceptedContract!));
        }
    }
}
