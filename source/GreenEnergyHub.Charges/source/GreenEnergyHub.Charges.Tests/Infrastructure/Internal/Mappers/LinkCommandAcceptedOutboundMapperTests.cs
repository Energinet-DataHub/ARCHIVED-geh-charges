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
using GreenEnergyHub.Charges.Core.DateTime;
using GreenEnergyHub.Charges.Domain.ChargeLinks;
using GreenEnergyHub.Charges.Infrastructure.Internal.ChargeLinkCommandAccepted;
using GreenEnergyHub.Charges.Infrastructure.Internal.Mappers;
using GreenEnergyHub.Charges.TestCore;
using GreenEnergyHub.TestHelpers.FluentAssertionsExtensions;
using NodaTime;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Infrastructure.Internal.Mappers
{
    [UnitTest]
    public class LinkCommandAcceptedOutboundMapperTests
    {
        [Theory]
        [InlineAutoMoqData]
        public void Convert_WhenCalled_ShouldMapToProtobufWithCorrectValues([NotNull] ChargeLinkCommandAcceptedEvent chargeLinkCommand)
        {
            // Arrange
            var mapper = new LinkCommandAcceptedOutboundMapper();

            UpdateInstantsToValidTimes(chargeLinkCommand);

            // Act
            var converted = (ChargeLinkCommandAcceptedContract)mapper.Convert(chargeLinkCommand);

            // Assert
            var chargeLinkDocument = chargeLinkCommand.Document;
            var convertedDocument = converted.Document;
            convertedDocument.Id.Should().BeEquivalentTo(chargeLinkDocument.Id);
            convertedDocument.Sender.Id.Should().BeEquivalentTo(chargeLinkDocument.Sender.Id);
            convertedDocument.Sender.MarketParticipantRole.Should().BeEquivalentTo(chargeLinkDocument.Sender.BusinessProcessRole);
            convertedDocument.Recipient.Id.Should().BeEquivalentTo(chargeLinkDocument.Recipient.Id);
            convertedDocument.Recipient.MarketParticipantRole.Should().BeEquivalentTo(chargeLinkDocument.Recipient.BusinessProcessRole);
            convertedDocument.BusinessReasonCode.Should().BeEquivalentTo(chargeLinkDocument.BusinessReasonCode);
            convertedDocument.CreatedDateTime.Seconds.Should().Be(chargeLinkDocument.CreatedDateTime.ToUnixTimeSeconds());
            convertedDocument.Type.Should().BeEquivalentTo(chargeLinkDocument.Type);
            convertedDocument.RequestDate.Seconds.Should().Be(chargeLinkDocument.RequestDate.ToUnixTimeSeconds());
            convertedDocument.IndustryClassification.Should().BeEquivalentTo(chargeLinkDocument.IndustryClassification);
            convertedDocument.Should().NotContainNullsOrEmptyEnumerables();
            chargeLinkDocument.Should().NotContainNullsOrEmptyEnumerables();
            converted.ChargeLink.Id.Should().Be(chargeLinkCommand.ChargeLink.Id);
            converted.ChargeLink.ChargeId.Should().Be(chargeLinkCommand.ChargeLink.ChargeId);
            converted.ChargeLink.ChargeOwner.Should().Be(chargeLinkCommand.ChargeLink.ChargeOwner);
            converted.ChargeLink.ChargeType.Should().Be(chargeLinkCommand.ChargeLink.ChargeType);
            converted.ChargeLink.Factor.Should().Be(chargeLinkCommand.ChargeLink.Factor);
            converted.ChargeLink.MeteringPointId.Should().Be(chargeLinkCommand.ChargeLink.MeteringPointId);
            converted.ChargeLink.StartDateTime.Seconds.Should().Be(chargeLinkCommand.ChargeLink.StartDateTime.ToUnixTimeSeconds());
            converted.ChargeLink.EndDateTime.Seconds.Should().Be(chargeLinkCommand.ChargeLink.EndDateTime.TimeOrEndDefault().ToUnixTimeSeconds());
        }

        [Fact]
        public void Convert_WhenCalledWithNull_ShouldThrow()
        {
            var mapper = new LinkCommandAcceptedOutboundMapper();
            ChargeLinkCommandAcceptedEvent? chargeLinkCommandAcceptedEvent = null;
            Assert.Throws<InvalidOperationException>(() => mapper.Convert(chargeLinkCommandAcceptedEvent!));
        }

        private static void UpdateInstantsToValidTimes([NotNull] ChargeLinkCommand chargeLinkCommand)
        {
            chargeLinkCommand.Document.RequestDate = Instant.FromUtc(2021, 7, 21, 11, 42, 25);
            chargeLinkCommand.Document.CreatedDateTime = Instant.FromUtc(2021, 7, 21, 12, 14, 43);
            chargeLinkCommand.ChargeLink.StartDateTime = Instant.FromUtc(2021, 8, 31, 22, 0);
            chargeLinkCommand.ChargeLink.EndDateTime = Instant.FromUtc(2021, 9, 30, 22, 0);
        }
    }
}
