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

using Energinet.DataHub.Core.App.FunctionApp.Middleware.CorrelationId;
using FluentAssertions;
using GreenEnergyHub.Charges.Infrastructure.Outbox;
using GreenEnergyHub.Charges.Tests.Builders.Command;
using GreenEnergyHub.Json;
using GreenEnergyHub.TestHelpers;
using NodaTime;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Infrastructure.Outbox
{
    [UnitTest]
    public class OutboxMessageFactoryTests
    {
        private const string OperationsRejectedEventType = "GreenEnergyHub.Charges.Application.Charges.Events.OperationsRejectedEvent";

        [Theory]
        [AutoDomainData]
        public void WhenOperationsRejectedEvent_ReturnsOutboxMessage_WithSerializedData(
            JsonSerializer jsonSerializer,
            IClock clock,
            ICorrelationContext context,
            OperationsRejectedEventBuilder operationBuilder)
        {
            // Arrange
            var rejectedEvent = operationBuilder.Build();
            var factory = new OutboxMessageFactory(jsonSerializer, clock, context);

            // Act
            var message = factory.CreateFrom(rejectedEvent);

            // Assert
            message.Id.Should().NotBeEmpty();
            message.Type.Should().Be(OperationsRejectedEventType);
            message.Data.Should().NotBeNullOrEmpty();
            message.CorrelationTraceContext.Should().Be(context.AsTraceContext());
            message.CreationDate.Should().NotBeNull();
            message.ProcessedDate.Should().BeNull();
        }
    }
}
