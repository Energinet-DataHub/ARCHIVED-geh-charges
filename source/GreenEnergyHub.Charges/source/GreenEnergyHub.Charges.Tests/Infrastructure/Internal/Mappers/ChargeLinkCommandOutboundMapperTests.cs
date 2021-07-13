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
using System.Globalization;
using Energinet.DataHub.ChargeLinks.InternalContracts;
using FluentAssertions;
using GreenEnergyHub.Charges.Domain.ChargeLinks;
using GreenEnergyHub.Charges.Infrastructure.Internal.Mappers;
using GreenEnergyHub.Charges.TestCore;
using GreenEnergyHub.TestHelpers.FluentAssertionsExtensions;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Infrastructure.Internal.Mappers
{
    [UnitTest]
    public class ChargeLinkCommandOutboundMapperTests
    {
        [Theory]
        [InlineAutoMoqData]
        public void Convert_WhenCalled_ShouldMapToProtobufWithCorrectValues([NotNull] ChargeLinkCommand chargeLinkCommand)
        {
            // Arrange
            var mapper = new ChargeLinkDomainOutboundMapper();

            // Act
            var converted = (ChargeLinkCommandDomain)mapper.Convert(chargeLinkCommand);

            // Assert
            var chargeLinkDocument = chargeLinkCommand.Document;
            var convertedDocument = converted.Document;
            convertedDocument.Id.Should().BeEquivalentTo(chargeLinkDocument.Id);
            convertedDocument.Sender.Id.Should().BeEquivalentTo(chargeLinkDocument.Sender.Id);
            convertedDocument.Sender.Name.Should().BeEquivalentTo(chargeLinkDocument.Sender.Name);
            convertedDocument.Sender.MarketParticipantRole.Should().BeEquivalentTo(chargeLinkDocument.Sender.BusinessProcessRole.ToString());
            convertedDocument.Recipient.Id.Should().BeEquivalentTo(chargeLinkDocument.Recipient.Id);
            convertedDocument.Recipient.Name.Should().BeEquivalentTo(chargeLinkDocument.Recipient.Name);
            convertedDocument.Recipient.MarketParticipantRole.Should().BeEquivalentTo(chargeLinkDocument.Recipient.BusinessProcessRole.ToString());
            convertedDocument.BusinessReasonCode.Should().BeEquivalentTo(chargeLinkDocument.BusinessReasonCode.ToString());
            convertedDocument.CreatedDateTime.Should().BeEquivalentTo(chargeLinkDocument.CreatedDateTime.ToString());
            convertedDocument.Type.Should().BeEquivalentTo(chargeLinkDocument.Type.ToString());
            convertedDocument.RequestDate.Should().BeEquivalentTo(chargeLinkDocument.RequestDate.ToString());
            convertedDocument.IndustryClassification.Should().BeEquivalentTo(chargeLinkDocument.IndustryClassification.ToString());
            convertedDocument.Should().NotContainNullsOrEmptyEnumerables();
            chargeLinkDocument.Should().NotContainNullsOrEmptyEnumerables();
            converted.ChargeLink.Id.Should().Be(chargeLinkCommand.ChargeLink.Id);
            converted.ChargeLink.ChargeId.Should().Be(chargeLinkCommand.ChargeLink.ChargeId);
            converted.ChargeLink.ChargeOwner.Should().Be(chargeLinkCommand.ChargeLink.ChargeOwner);
            converted.ChargeLink.ChargeType.Should().Be(chargeLinkCommand.ChargeLink.ChargeType.ToString());
            converted.ChargeLink.Factor.Should().Be(chargeLinkCommand.ChargeLink.Factor.ToString(CultureInfo.InvariantCulture));
            converted.ChargeLink.MeteringPointId.Should().Be(chargeLinkCommand.ChargeLink.MeteringPointId);
            converted.ChargeLink.StartDateTime.Should().BeEquivalentTo(converted.ChargeLink.StartDateTime);
            converted.ChargeLink.EndDateTime.Should().BeEquivalentTo(converted.ChargeLink.EndDateTime);
        }
    }
}
