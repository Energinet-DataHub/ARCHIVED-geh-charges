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
using GreenEnergyHub.Charges.Domain.Events.Local;
using GreenEnergyHub.Charges.Infrastructure.Internal.ChargeCommandReceived;
using GreenEnergyHub.Charges.Infrastructure.Internal.Mappers;
using GreenEnergyHub.Charges.TestCore;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Infrastructure.Internal.Mappers
{
    [UnitTest]
    public class ChargeCommandReceivedInboundMapperTests
    {
        [Theory]
        [InlineAutoMoqData]
        public void Convert_WhenCalled_ShouldMapToDomainObjectWithCorrectValues(
            [NotNull] ChargeCommandReceivedContract acceptedCommand,
            [NotNull] ChargeCommandReceivedInboundMapper sut)
        {
            // Arrange

            // Act
            var converted = (ChargeCommandReceivedEvent)sut.Convert(acceptedCommand);

            // Assert
            converted.CorrelationId.Should().BeEquivalentTo(acceptedCommand.CorrelationId);
            var commandChargeOperation = converted.Command.ChargeOperation;
            commandChargeOperation.Id.Should().BeEquivalentTo(acceptedCommand.ChargeOperation.Id);
            commandChargeOperation.Resolution.Should().BeEquivalentTo(acceptedCommand.ChargeOperation.Resolution);
            commandChargeOperation.Type.Should().BeEquivalentTo(acceptedCommand.ChargeOperation.ChargeType);
            commandChargeOperation.ChargeDescription.Should().BeEquivalentTo(acceptedCommand.ChargeOperation.ChargeDescription);
            commandChargeOperation.ChargeId.Should().BeEquivalentTo(acceptedCommand.ChargeOperation.ChargeId);
            commandChargeOperation.ChargeName.Should().BeEquivalentTo(acceptedCommand.ChargeOperation.ChargeName);
            commandChargeOperation.ChargeOwner.Should().BeEquivalentTo(acceptedCommand.ChargeOperation.ChargeOwner);
            commandChargeOperation.OperationType.Should().BeEquivalentTo(acceptedCommand.ChargeOperation.OperationType);
            commandChargeOperation.TaxIndicator.Should().Be(acceptedCommand.ChargeOperation.TaxIndicator);
            commandChargeOperation.TransparentInvoicing.Should().Be(acceptedCommand.ChargeOperation.TransparentInvoicing);
            commandChargeOperation.VatClassification.Should().BeEquivalentTo(acceptedCommand.ChargeOperation.VatClassification);
            commandChargeOperation.EndDateTime!.Value.ToUnixTimeSeconds().Should().Be(acceptedCommand.ChargeOperation.EndDateTime.Seconds);
            commandChargeOperation.StartDateTime.ToUnixTimeSeconds().Should().Be(acceptedCommand.ChargeOperation.StartDateTime.Seconds);
            commandChargeOperation.Points.Should().BeEquivalentTo(acceptedCommand.ChargeOperation.Points);

            var commandDocument = converted.Command.Document;
            commandDocument.Id.Should().BeEquivalentTo(acceptedCommand.Document.Id);
            commandDocument.Recipient.Id.Should().BeEquivalentTo(acceptedCommand.Document.Recipient.Id);
            commandDocument.Recipient.BusinessProcessRole.Should().BeEquivalentTo(acceptedCommand.Document.Recipient.MarketParticipantRole);
            commandDocument.Sender.Id.Should().BeEquivalentTo(acceptedCommand.Document.Sender.Id);
            commandDocument.Sender.BusinessProcessRole.Should().BeEquivalentTo(acceptedCommand.Document.Sender.MarketParticipantRole);
            commandDocument.Type.Should().BeEquivalentTo(acceptedCommand.Document.Type);
            commandDocument.IndustryClassification.Should().BeEquivalentTo(acceptedCommand.Document.IndustryClassification);
            commandDocument.RequestDate.ToUnixTimeSeconds().Should().Be(acceptedCommand.Document.RequestDate.Seconds);
            commandDocument.BusinessReasonCode.Should().BeEquivalentTo(acceptedCommand.Document.BusinessReasonCode);
            commandDocument.CreatedDateTime.ToUnixTimeSeconds().Should().Be(acceptedCommand.Document.CreatedDateTime.Seconds);
        }
    }
}
