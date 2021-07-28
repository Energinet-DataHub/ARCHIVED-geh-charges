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
using GreenEnergyHub.Charges.Core.DateTime;
using GreenEnergyHub.Charges.Domain.ChangeOfCharges.Transaction;
using GreenEnergyHub.Charges.Domain.Events.Local;
using GreenEnergyHub.Charges.Infrastructure.Internal.ChargeCommandAccepted;
using GreenEnergyHub.Charges.Infrastructure.Internal.Mappers;
using GreenEnergyHub.Charges.TestCore;
using GreenEnergyHub.TestHelpers.FluentAssertionsExtensions;
using NodaTime;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Infrastructure.Internal.Mappers
{
    [UnitTest]
    public class ChargeCommandAcceptedOutboundMapperTests
    {
        [Theory]
        [InlineAutoMoqData]
        public void Convert_WhenCalled_ShouldMapToProtobufWithCorrectValues([NotNull]ChargeCommand chargeCommand)
        {
            // Arrange
            ChargeCommandAcceptedEvent chargeCommandAcceptedEvent = new (SystemClock.Instance.GetCurrentInstant(), chargeCommand);

            var mapper = new ChargeCommandAcceptedOutboundMapper();

            UpdateInstantsToValidTimes(chargeCommandAcceptedEvent);

            // Act
            var converted = (ChargeCommandAcceptedContract)mapper.Convert(chargeCommandAcceptedEvent);

            // Assert
            var chargeLinkDocument = chargeCommandAcceptedEvent.Command.Document;
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
            converted.ChargeOperation.Id.Should().Be(chargeCommandAcceptedEvent.Command.ChargeOperation.Id);
            converted.ChargeOperation.OperationType.Should().Be(chargeCommandAcceptedEvent.Command.ChargeOperation.OperationType);
            converted.ChargeOperation.ChargeId.Should().Be(chargeCommandAcceptedEvent.Command.ChargeOperation.ChargeId);
            converted.ChargeOperation.ChargeType.Should().Be(chargeCommandAcceptedEvent.Command.ChargeOperation.Type);
            converted.ChargeOperation.ChargeName.Should().Be(chargeCommandAcceptedEvent.Command.ChargeOperation.ChargeName);
            converted.ChargeOperation.ChargeDescription.Should().Be(chargeCommandAcceptedEvent.Command.ChargeOperation.ChargeDescription);
            converted.ChargeOperation.StartDateTime.Seconds.Should().Be(chargeCommandAcceptedEvent.Command.ChargeOperation.StartDateTime.ToUnixTimeSeconds());
            converted.ChargeOperation.EndDateTime.Seconds.Should().Be(chargeCommandAcceptedEvent.Command.ChargeOperation.EndDateTime.TimeOrEndDefault().ToUnixTimeSeconds());
            converted.ChargeOperation.VatClassification.Should().Be(chargeCommandAcceptedEvent.Command.ChargeOperation.VatClassification);
            converted.ChargeOperation.TransparentInvoicing.Should().Be(chargeCommandAcceptedEvent.Command.ChargeOperation.TransparentInvoicing);
            converted.ChargeOperation.TaxIndicator.Should().Be(chargeCommandAcceptedEvent.Command.ChargeOperation.TaxIndicator);
            converted.ChargeOperation.ChargeOwner.Should().Be(chargeCommandAcceptedEvent.Command.ChargeOperation.ChargeOwner);
            converted.ChargeOperation.Resolution.Should().Be(chargeCommandAcceptedEvent.Command.ChargeOperation.Resolution);
            foreach (var point in converted.ChargeOperation.Points)
            {
                var matchingPoint = chargeCommandAcceptedEvent.Command.ChargeOperation.Points[converted.ChargeOperation.Points.IndexOf(point)];
                point.Position.Should().Be(matchingPoint.Position);
                point.Price.Should().Be((double)matchingPoint.Price);
                point.Time.Seconds.Should().Be(matchingPoint.Time.ToUnixTimeSeconds());
            }
        }

        private static void UpdateInstantsToValidTimes([NotNull] ChargeCommandAcceptedEvent chargeCommandAcceptedEvent)
        {
            chargeCommandAcceptedEvent.Command.Document.RequestDate = Instant.FromUtc(2021, 7, 21, 11, 42, 25);
            chargeCommandAcceptedEvent.Command.Document.CreatedDateTime = Instant.FromUtc(2021, 7, 21, 12, 14, 43);
            chargeCommandAcceptedEvent.Command.ChargeOperation.StartDateTime = Instant.FromUtc(2021, 8, 31, 22, 0);
            chargeCommandAcceptedEvent.Command.ChargeOperation.EndDateTime = Instant.FromUtc(2021, 9, 30, 22, 0);

            foreach (var point in chargeCommandAcceptedEvent.Command.ChargeOperation.Points)
            {
                point.Time = SystemClock.Instance.GetCurrentInstant();
            }
        }
    }
}
