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
using GreenEnergyHub.Charges.Application.ChargeLinks.Mapping;
using GreenEnergyHub.Charges.Domain.ChargeLinks.Events.Local;
using GreenEnergyHub.Charges.TestCore.Attributes;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Application.ChargeLinks.Mapping
{
    [UnitTest]
    public class ChargeLinkCommandMapperTests
    {
        [Theory]
        [InlineAutoMoqData]
        public void Map_WhenCalled_ShouldMapFromReceivedToAcceptedWithCorrectValues(
            [NotNull] ChargeLinkCommandReceivedEvent chargeLinkCommandReceivedEvent)
        {
            // Arrange
            var mapper = new ChargeLinkCommandMapper();

            // Act
            var chargeLinkCommandAcceptedEvent = mapper.Map(chargeLinkCommandReceivedEvent);

            // Assert
            chargeLinkCommandAcceptedEvent.ChargeLinkCommand.Document.Should().BeEquivalentTo(chargeLinkCommandReceivedEvent.ChargeLinkCommand.Document);
            chargeLinkCommandAcceptedEvent.ChargeLinkCommand.ChargeLink.Should().BeEquivalentTo(chargeLinkCommandReceivedEvent.ChargeLinkCommand.ChargeLink);

            // TODO: LRN Transaction is newed when "mapping", after chargeLinkCommandAcceptedEvent Inheritance from InternalEventBase this will be sorted.
            // chargeLinkCommandAcceptedEvent.ChargeLink.Should().BeEquivalentTo(chargeLinkCommandReceivedEvent.ChargeLinkCommand.Transaction);
        }
    }
}
