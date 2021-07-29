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
using GreenEnergyHub.Charges.Domain.Events.Local;
using GreenEnergyHub.Charges.Infrastructure.Internal.ChargeCommandRejected;
using GreenEnergyHub.Charges.Infrastructure.Internal.Mappers;
using GreenEnergyHub.Charges.TestCore;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Infrastructure.Internal.Mappers
{
    [UnitTest]
    public class ChargeCommandRejectedInboundMapperTests
    {
        [Theory]
        [InlineAutoMoqData]
        public void Convert_WhenCalled_ShouldMapToDomainObjectWithCorrectValues(
            [NotNull] ChargeCommandRejectedContract rejectedContract,
            [NotNull] ChargeCommandRejectedInboundMapper sut)
        {
            // Act
            var converted = (ChargeCommandRejectedEvent)sut.Convert(rejectedContract);

            // Assert
            converted.CorrelationId.Should().BeEquivalentTo(rejectedContract.ChargeCommand.CorrelationId);
            converted.Reason.Should().BeEquivalentTo(rejectedContract.RejectReasons);

            var commandChargeOperation = converted.Command.ChargeOperation;
            commandChargeOperation.Id.Should().BeEquivalentTo(rejectedContract.ChargeCommand.ChargeOperation.Id);
            commandChargeOperation.Resolution.Should().BeEquivalentTo(rejectedContract.ChargeCommand.ChargeOperation.Resolution);
            commandChargeOperation.Type.Should().BeEquivalentTo(rejectedContract.ChargeCommand.ChargeOperation.ChargeType);
            commandChargeOperation.ChargeDescription.Should().BeEquivalentTo(rejectedContract.ChargeCommand.ChargeOperation.ChargeDescription);
            commandChargeOperation.ChargeId.Should().BeEquivalentTo(rejectedContract.ChargeCommand.ChargeOperation.ChargeId);
            commandChargeOperation.ChargeName.Should().BeEquivalentTo(rejectedContract.ChargeCommand.ChargeOperation.ChargeName);
            commandChargeOperation.ChargeOwner.Should().BeEquivalentTo(rejectedContract.ChargeCommand.ChargeOperation.ChargeOwner);
            commandChargeOperation.OperationType.Should().BeEquivalentTo(rejectedContract.ChargeCommand.ChargeOperation.OperationType);
            commandChargeOperation.TaxIndicator.Should().Be(rejectedContract.ChargeCommand.ChargeOperation.TaxIndicator);
            commandChargeOperation.TransparentInvoicing.Should().Be(rejectedContract.ChargeCommand.ChargeOperation.TransparentInvoicing);
            commandChargeOperation.VatClassification.Should().BeEquivalentTo(rejectedContract.ChargeCommand.ChargeOperation.VatClassification);
            commandChargeOperation.EndDateTime!.Value.ToUnixTimeSeconds().Should().Be(rejectedContract.ChargeCommand.ChargeOperation.EndDateTime.Seconds);
            commandChargeOperation.StartDateTime.ToUnixTimeSeconds().Should().Be(rejectedContract.ChargeCommand.ChargeOperation.StartDateTime.Seconds);
            commandChargeOperation.Points.Should().BeEquivalentTo(rejectedContract.ChargeCommand.ChargeOperation.Points);

            var commandDocument = converted.Command.Document;
            commandDocument.Id.Should().BeEquivalentTo(rejectedContract.ChargeCommand.Document.Id);
            commandDocument.Recipient.Id.Should().BeEquivalentTo(rejectedContract.ChargeCommand.Document.Recipient.Id);
            commandDocument.Recipient.BusinessProcessRole.Should().BeEquivalentTo(rejectedContract.ChargeCommand.Document.Recipient.BusinessProcessRole);
            commandDocument.Sender.Id.Should().BeEquivalentTo(rejectedContract.ChargeCommand.Document.Sender.Id);
            commandDocument.Sender.BusinessProcessRole.Should().BeEquivalentTo(rejectedContract.ChargeCommand.Document.Sender.BusinessProcessRole);
            commandDocument.Type.Should().BeEquivalentTo(rejectedContract.ChargeCommand.Document.Type);
            commandDocument.IndustryClassification.Should().BeEquivalentTo(rejectedContract.ChargeCommand.Document.IndustryClassification);
            commandDocument.RequestDate.ToUnixTimeSeconds().Should().Be(rejectedContract.ChargeCommand.Document.RequestDate.Seconds);
            commandDocument.BusinessReasonCode.Should().BeEquivalentTo(rejectedContract.ChargeCommand.Document.BusinessReasonCode);
            commandDocument.CreatedDateTime.ToUnixTimeSeconds().Should().Be(rejectedContract.ChargeCommand.Document.CreatedDateTime.Seconds);
        }

        [Fact]
        public void Convert_WhenCalledWithNull_ShouldThrow()
        {
            var mapper = new ChargeCommandRejectedInboundMapper();
            ChargeCommandRejectedContract? chargeCommandRejectedContract = null;
            Assert.Throws<InvalidOperationException>(() => mapper.Convert(chargeCommandRejectedContract!));
        }
    }
}
