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
using System.Linq;
using FluentAssertions;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeLinkCreatedEvents;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeLinksCommands;
using GreenEnergyHub.Charges.TestCore.Attributes;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Application.ChargeLinks.Factories
{
    [UnitTest]
    public class ChargeLinkCreatedEventFactoryTests
    {
        [Theory]
        [InlineAutoMoqData]
        public void CreateEvent_WhenCalled_CreatesEventWithCorrectValues(
            [NotNull] ChargeLinksCommand command,
            [NotNull] ChargeLinkCreatedEventFactory sut)
        {
            // Arrange

            // Act
            var result = sut.CreateEvents(command);
            var chargeLinkCreatedEvents =
                command.ChargeLinks
                    .ToDictionary(chargeLinkDto => chargeLinkDto, chargeLinkDto => result
                        .Single(x => x.ChargeId == chargeLinkDto.SenderProvidedChargeId));

            // Assert
            foreach (var c in chargeLinkCreatedEvents)
            {
                c.Key.Factor.Should().Be(c.Value.ChargeLinkPeriod.Factor);
                c.Key.ChargeOwnerId.Should().Be(c.Value.ChargeOwner);
                c.Key.ChargeType.Should().Be(c.Value.ChargeType);
                c.Key.OperationId.Should().Be(c.Value.ChargeLinkId);
                c.Key.EndDateTime.Should().Be(c.Value.ChargeLinkPeriod.EndDateTime);
                c.Key.StartDateTime.Should().Be(c.Value.ChargeLinkPeriod.StartDateTime);
                c.Key.SenderProvidedChargeId.Should().Be(c.Value.ChargeId);
            }
        }
    }
}
