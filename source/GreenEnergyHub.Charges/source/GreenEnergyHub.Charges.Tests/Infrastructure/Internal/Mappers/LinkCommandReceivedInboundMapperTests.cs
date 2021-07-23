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
using FluentAssertions;
using GreenEnergyHub.Charges.Domain.ChargeLinks;
using GreenEnergyHub.Charges.Infrastructure.Internal.ChargeLinkCommandReceived;
using GreenEnergyHub.Charges.Infrastructure.Internal.Mappers;
using GreenEnergyHub.Charges.TestCore;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Infrastructure.Internal.Mappers
{
    [UnitTest]
    public class LinkCommandReceivedInboundMapperTests
    {
        [Theory]
        [InlineAutoMoqData]
        public void Convert_WhenCalled_ShouldMapToProtobufWithCorrectValues([NotNull] ChargeLinkCommandReceivedContract chargeLinkCommand)
        {
            // Arrange
            var mapper = new LinkCommandReceivedInboundMapper();

            // Act
            var converted = (ChargeLinkCommandReceivedEvent)mapper.Convert(chargeLinkCommand);

            // Assert
            var chargeLinkDocument = chargeLinkCommand.Document;
            var convertedDocument = converted.Document;
            convertedDocument.Id.Should().BeEquivalentTo(chargeLinkDocument.Id);
            convertedDocument.Sender.Id.Should().BeEquivalentTo(chargeLinkDocument.Sender.Id);
            convertedDocument.Sender.BusinessProcessRole.Should().BeEquivalentTo(chargeLinkDocument.Sender.MarketParticipantRole);
            convertedDocument.Recipient.Id.Should().BeEquivalentTo(chargeLinkDocument.Recipient.Id);
            convertedDocument.Recipient.BusinessProcessRole.Should().BeEquivalentTo(chargeLinkDocument.Recipient.MarketParticipantRole);
            convertedDocument.BusinessReasonCode.Should().BeEquivalentTo(chargeLinkDocument.BusinessReasonCode);
            convertedDocument.CreatedDateTime.ToUnixTimeSeconds().Should().Be(chargeLinkDocument.CreatedDateTime.Seconds);
            convertedDocument.Type.Should().BeEquivalentTo(chargeLinkDocument.Type);
            convertedDocument.RequestDate.ToUnixTimeSeconds().Should().Be(chargeLinkDocument.RequestDate.Seconds);
            convertedDocument.IndustryClassification.Should().BeEquivalentTo(chargeLinkDocument.IndustryClassification);
            converted.ChargeLink.Id.Should().Be(chargeLinkCommand.ChargeLink.Id);
            converted.ChargeLink.ChargeId.Should().Be(chargeLinkCommand.ChargeLink.ChargeId);
            converted.ChargeLink.ChargeOwner.Should().Be(chargeLinkCommand.ChargeLink.ChargeOwner);
            converted.ChargeLink.ChargeType.Should().Be(chargeLinkCommand.ChargeLink.ChargeType);
            converted.ChargeLink.Factor.Should().Be(chargeLinkCommand.ChargeLink.Factor);
            converted.ChargeLink.MeteringPointId.Should().Be(chargeLinkCommand.ChargeLink.MeteringPointId);
            converted.ChargeLink.StartDateTime.ToUnixTimeSeconds().Should().Be(chargeLinkCommand.ChargeLink.StartDateTime.Seconds);
            converted.ChargeLink.EndDateTime!.Value.ToUnixTimeSeconds().Should().Be(chargeLinkCommand.ChargeLink.EndDateTime.Seconds);
        }
    }
}
