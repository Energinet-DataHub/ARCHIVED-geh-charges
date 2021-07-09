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
using System.Globalization;
using Energinet.DataHub.ChargeLinks.InternalContracts;
using FluentAssertions;
using GreenEnergyHub.Charges.Domain.ChargeLinks;
using GreenEnergyHub.Charges.Domain.MarketDocument;
using GreenEnergyHub.Charges.Infrastructure.Internal.Mappers;
using GreenEnergyHub.Messaging.MessageTypes.Common;
using NodaTime;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Infrastructure.Internal.Mappers
{
    [UnitTest]
    public class ChargeLinkCommandOutboundMapperTests
    {
        [Fact]
        public void Convert_WhenCalled_ShouldMapToProtobufWithCorrectValues()
        {
            // Arrange
            var correlationId = Guid.NewGuid();
            var chargeLink = new ChargeLinkCommand(correlationId.ToString())
            {
                Document = new Document
                {
                    Id = "1",
                    Sender = new Domain.MarketDocument.MarketParticipant
                    {
                        Id = "sender",
                        Name = "senderName",
                        BusinessProcessRole = MarketParticipantRole.EnergyAgency,
                    },
                    Recipient = new Domain.MarketDocument.MarketParticipant
                    {
                        Id = "recipient",
                        Name = "recipientName",
                        BusinessProcessRole = MarketParticipantRole.EnergyAgency,
                    },
                    BusinessReasonCode = BusinessReasonCode.UpdateChargeInformation,
                    CreatedDateTime = SystemClock.Instance.GetCurrentInstant(),
                    Type = DocumentType.RequestChangeBillingMasterData,
                    RequestDate = SystemClock.Instance.GetCurrentInstant(),
                    IndustryClassification = IndustryClassification.Electricity,
                },
                ChargeLink = new ChargeLink
                {
                    Id = "11",
                    ChargeId = "12",
                    ChargeOwner = "13",
                    ChargeType = Domain.ChangeOfCharges.Transaction.ChargeType.Tariff,
                    Factor = 15,
                    MeteringPointId = "16",
                    EndDateTime = null,
                    StartDateTime = SystemClock.Instance.GetCurrentInstant(),
                    Transaction = Transaction.NewTransaction(),
                },
                Transaction = Transaction.NewTransaction(),
            };
            var mapper = new ChargeLinkDomainOutboundMapper();

            // Act
            var converted = (ChargeLinkCommandDomain)mapper.Convert(chargeLink);

            // Assert
            var chargeLinkDocument = chargeLink.Document;
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

            converted.ChargeLink.Id.Should().Be(chargeLink.ChargeLink.Id);
            converted.ChargeLink.ChargeId.Should().Be(chargeLink.ChargeLink.ChargeId);
            converted.ChargeLink.ChargeOwner.Should().Be(chargeLink.ChargeLink.ChargeOwner);
            converted.ChargeLink.ChargeType.Should().Be(chargeLink.ChargeLink.ChargeType.ToString());
            converted.ChargeLink.Factor.Should().Be(chargeLink.ChargeLink.Factor.ToString(CultureInfo.InvariantCulture));
            converted.ChargeLink.MeteringPointId.Should().Be(chargeLink.ChargeLink.MeteringPointId);
            converted.ChargeLink.StartDateTime.Should().BeEquivalentTo(converted.ChargeLink.StartDateTime);
            converted.ChargeLink.EndDateTime.Should().BeEquivalentTo(converted.ChargeLink.EndDateTime);
        }
    }
}
