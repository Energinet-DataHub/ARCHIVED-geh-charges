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
using FluentAssertions;
using GreenEnergyHub.Charges.Core.DateTime;
using GreenEnergyHub.Charges.Domain.ChangeOfCharges.Transaction;
using GreenEnergyHub.Charges.Domain.Events.Local;
using GreenEnergyHub.Charges.Infrastructure.Internal.ChargeCommandRejected;
using GreenEnergyHub.Charges.Infrastructure.Internal.Mappers;
using GreenEnergyHub.Charges.TestCore;
using GreenEnergyHub.TestHelpers.FluentAssertionsExtensions;
using NodaTime;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Infrastructure.Internal.Mappers
{
    [UnitTest]
    public class ChargeCommandRejectedOutboundMapperTests
    {
        [Theory]
        [InlineAutoMoqData]
        public void Convert_WhenCalled_ShouldMapToProtobufWithCorrectValues([NotNull]ChargeCommand chargeCommand)
        {
            // Arrange
            var reasons = new List<string> { "reason 1", "reason 2" };
            ChargeCommandRejectedEvent chargeCommandRejectedEvent = new (SystemClock.Instance.GetCurrentInstant(), chargeCommand, reasons);

            var mapper = new ChargeCommandRejectedOutboundMapper();

            UpdateInstantsToValidTimes(chargeCommandRejectedEvent);

            // Act
            var converted = (ChargeCommandRejectedContract)mapper.Convert(chargeCommandRejectedEvent);

            // Assert
            var chargeLinkDocument = chargeCommandRejectedEvent.Command.Document;
            var convertedDocument = converted.ChargeCommand.Document;
            convertedDocument.Id.Should().BeEquivalentTo(chargeLinkDocument.Id);
            convertedDocument.Sender.Id.Should().BeEquivalentTo(chargeLinkDocument.Sender.Id);
            convertedDocument.Sender.BusinessProcessRole.Should().BeEquivalentTo(chargeLinkDocument.Sender.BusinessProcessRole);
            convertedDocument.Recipient.Id.Should().BeEquivalentTo(chargeLinkDocument.Recipient.Id);
            convertedDocument.Recipient.BusinessProcessRole.Should().BeEquivalentTo(chargeLinkDocument.Recipient.BusinessProcessRole);
            convertedDocument.BusinessReasonCode.Should().BeEquivalentTo(chargeLinkDocument.BusinessReasonCode);
            convertedDocument.CreatedDateTime.Seconds.Should().Be(chargeLinkDocument.CreatedDateTime.ToUnixTimeSeconds());
            convertedDocument.Type.Should().BeEquivalentTo(chargeLinkDocument.Type);
            convertedDocument.RequestDate.Seconds.Should().Be(chargeLinkDocument.RequestDate.ToUnixTimeSeconds());
            convertedDocument.IndustryClassification.Should().BeEquivalentTo(chargeLinkDocument.IndustryClassification);
            convertedDocument.Should().NotContainNullsOrEmptyEnumerables();
            chargeLinkDocument.Should().NotContainNullsOrEmptyEnumerables();
            converted.ChargeCommand.ChargeOperation.Id.Should().Be(chargeCommandRejectedEvent.Command.ChargeOperation.Id);
            converted.ChargeCommand.ChargeOperation.OperationType.Should().Be(chargeCommandRejectedEvent.Command.ChargeOperation.OperationType);
            converted.ChargeCommand.ChargeOperation.ChargeId.Should().Be(chargeCommandRejectedEvent.Command.ChargeOperation.ChargeId);
            converted.ChargeCommand.ChargeOperation.ChargeType.Should().Be(chargeCommandRejectedEvent.Command.ChargeOperation.Type);
            converted.ChargeCommand.ChargeOperation.ChargeName.Should().Be(chargeCommandRejectedEvent.Command.ChargeOperation.ChargeName);
            converted.ChargeCommand.ChargeOperation.ChargeDescription.Should().Be(chargeCommandRejectedEvent.Command.ChargeOperation.ChargeDescription);
            converted.ChargeCommand.ChargeOperation.StartDateTime.Seconds.Should().Be(chargeCommandRejectedEvent.Command.ChargeOperation.StartDateTime.ToUnixTimeSeconds());
            converted.ChargeCommand.ChargeOperation.EndDateTime.Seconds.Should().Be(chargeCommandRejectedEvent.Command.ChargeOperation.EndDateTime.TimeOrEndDefault().ToUnixTimeSeconds());
            converted.ChargeCommand.ChargeOperation.VatClassification.Should().Be(chargeCommandRejectedEvent.Command.ChargeOperation.VatClassification);
            converted.ChargeCommand.ChargeOperation.TransparentInvoicing.Should().Be(chargeCommandRejectedEvent.Command.ChargeOperation.TransparentInvoicing);
            converted.ChargeCommand.ChargeOperation.TaxIndicator.Should().Be(chargeCommandRejectedEvent.Command.ChargeOperation.TaxIndicator);
            converted.ChargeCommand.ChargeOperation.ChargeOwner.Should().Be(chargeCommandRejectedEvent.Command.ChargeOperation.ChargeOwner);
            converted.ChargeCommand.ChargeOperation.Resolution.Should().Be(chargeCommandRejectedEvent.Command.ChargeOperation.Resolution);
            foreach (var point in converted.ChargeCommand.ChargeOperation.Points)
            {
                var matchingPoint = chargeCommandRejectedEvent.Command.ChargeOperation.Points[converted.ChargeCommand.ChargeOperation.Points.IndexOf(point)];
                point.Position.Should().Be(matchingPoint.Position);
                point.Price.Should().Be((double)matchingPoint.Price);
                point.Time.Seconds.Should().Be(matchingPoint.Time.ToUnixTimeSeconds());
            }
        }

        [Fact]
        public void Convert_WhenCalledWithNull_ShouldThrow()
        {
            var mapper = new ChargeCommandRejectedOutboundMapper();
            ChargeCommandRejectedEvent? chargeCommandRejectedEvent = null;
            Assert.Throws<InvalidOperationException>(() => mapper.Convert(chargeCommandRejectedEvent!));
        }

        private static void UpdateInstantsToValidTimes([NotNull] ChargeCommandRejectedEvent chargeCommandRejectedEvent)
        {
            chargeCommandRejectedEvent.Command.Document.RequestDate = Instant.FromUtc(2021, 7, 21, 11, 42, 25);
            chargeCommandRejectedEvent.Command.Document.CreatedDateTime = Instant.FromUtc(2021, 7, 21, 12, 14, 43);
            chargeCommandRejectedEvent.Command.ChargeOperation.StartDateTime = Instant.FromUtc(2021, 8, 31, 22, 0);
            chargeCommandRejectedEvent.Command.ChargeOperation.EndDateTime = Instant.FromUtc(2021, 9, 30, 22, 0);

            foreach (var point in chargeCommandRejectedEvent.Command.ChargeOperation.Points)
            {
                point.Time = SystemClock.Instance.GetCurrentInstant();
            }
        }
    }
}
