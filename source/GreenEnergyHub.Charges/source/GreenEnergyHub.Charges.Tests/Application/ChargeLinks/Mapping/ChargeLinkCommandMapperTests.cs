﻿// Copyright 2020 Energinet DataHub A/S
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

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using FluentAssertions;
using GreenEnergyHub.Charges.Domain.ChargeLinkCommandAcceptedEvents;
using GreenEnergyHub.Charges.Domain.ChargeLinkCommands;
using GreenEnergyHub.Charges.TestCore.Attributes;
using NodaTime;
using NodaTime.Testing;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Application.ChargeLinks.Mapping
{
    [UnitTest]
    public class ChargeLinkCommandMapperTests
    {
        [Theory]
        [InlineAutoMoqData]
        public void Map_MapsFrom_ChargeLinkCommand_ToAcceptedEventWithCorrectValues(
            [NotNull] ChargeLinkCommand chargeLinkCommand)
        {
            // Arrange
            var factory = new ChargeLinkCommandAcceptedEventFactory(new FakeClock(Instant.MinValue));

            // Act
            var chargeLinkCommandAcceptedEvent = factory.Create(new List<ChargeLinkCommand> { chargeLinkCommand }, chargeLinkCommand.CorrelationId);
            var actual = chargeLinkCommandAcceptedEvent.ChargeLinkCommands.First();

            // Assert
            actual.Document.Should().BeEquivalentTo(chargeLinkCommand.Document);
            actual.ChargeLink.Should().BeEquivalentTo(chargeLinkCommand.ChargeLink);
        }
    }
}
