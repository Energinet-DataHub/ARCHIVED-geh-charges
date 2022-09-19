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

using Energinet.DataHub.Core.JsonSerialization;
using FluentAssertions;
using GreenEnergyHub.Charges.Application.Charges.Events;
using GreenEnergyHub.Charges.Infrastructure.Outbox;
using GreenEnergyHub.Charges.TestCore.Builders.Command;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Infrastructure.Outbox
{
    [UnitTest]
    public class OutboxMessageParserTests
    {
        [Fact]
        public void Parse_ChargePriceOperationsRejectedEventType_ChargePriceOperationsRejectedEventReturned()
        {
            // Arrange
            var jsonSerializer = new JsonSerializer();
            var rejectedEvent = new ChargePriceOperationsRejectedEventBuilder().Build();
            var messageTypeString = typeof(ChargePriceOperationsRejectedEvent).FullName;
            var serializedEvent = jsonSerializer.Serialize(rejectedEvent);
            var sut = new OutboxMessageParser(jsonSerializer);

            // Act
            var actual = sut.Parse(messageTypeString!, serializedEvent);

            // Assert
            actual.Should().BeOfType(typeof(ChargePriceOperationsRejectedEvent));
        }

        [Fact]
        public void Parse_ChargePriceOperationsConfirmedEventType_ChargePriceOperationsConfirmedEventReturned()
        {
            // Arrange
            var jsonSerializer = new JsonSerializer();
            var confirmedEvent = new ChargePriceOperationsConfirmedEventBuilder().Build();
            var messageTypeString = typeof(ChargePriceOperationsConfirmedEvent).FullName;
            var serializedEvent = jsonSerializer.Serialize(confirmedEvent);
            var sut = new OutboxMessageParser(jsonSerializer);

            // Act
            var actual = sut.Parse(messageTypeString!, serializedEvent);

            // Assert
            actual.Should().BeOfType(typeof(ChargePriceOperationsConfirmedEvent));
        }
    }
}
