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
using System.Linq;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Energinet.DataHub.Core.FunctionApp.TestCommon;
using FluentAssertions;
using GreenEnergyHub.Charges.Application.Charges.Events;
using GreenEnergyHub.Charges.Domain.Dtos.ChargePriceCommandReceivedEvents;
using GreenEnergyHub.Charges.Domain.Dtos.Messages.Events;
using GreenEnergyHub.Charges.FunctionHost.Charges;
using GreenEnergyHub.Charges.Infrastructure.Core.MessageMetaData;
using GreenEnergyHub.Charges.IntegrationTest.Core.Fixtures.FunctionApp;
using GreenEnergyHub.Charges.IntegrationTest.Core.TestCommon;
using GreenEnergyHub.Charges.IntegrationTest.Core.TestHelpers;
using GreenEnergyHub.Charges.IntegrationTests.Fixtures;
using GreenEnergyHub.Charges.TestCore.Attributes;
using GreenEnergyHub.Charges.Tests.Builders.Command;
using NodaTime;
using Xunit;
using Xunit.Abstractions;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.IntegrationTests.IntegrationTests.EndpointTests
{
    [IntegrationTest]
    public class ChargePriceCommandReceiverEndpointTests
    {
        [Collection(nameof(ChargesFunctionAppCollectionFixture))]
        public class RunAsync : FunctionAppTestBase<ChargesFunctionAppFixture>, IAsyncLifetime
        {
            private static readonly string _operationsRejectedEventType = typeof(ChargePriceOperationsRejectedEvent).FullName!;

            public RunAsync(ChargesFunctionAppFixture fixture, ITestOutputHelper testOutputHelper)
                : base(fixture, testOutputHelper)
            {
            }

            public Task DisposeAsync()
            {
                Fixture.MessageHubMock.Clear();
                return Task.CompletedTask;
            }

            public Task InitializeAsync()
            {
                return Task.CompletedTask;
            }

            [Theory]
            [InlineAutoMoqData]
            public async Task RunAsync_WhenValidationFails_PersistsRejectedEvent(
                ChargePriceCommandBuilder commandBuilder,
                ChargePriceOperationDtoBuilder operationDtoBuilder)
            {
                // Arrange
                await using var context = Fixture.ChargesDatabaseManager.CreateDbContext();
                var invalidChargePriceCommandReceivedEvent = CreateInvalidChargePriceCommandReceivedEvent(
                    commandBuilder, operationDtoBuilder);
                var correlationId = CorrelationIdGenerator.Create();
                var message = CreateServiceBusMessage(invalidChargePriceCommandReceivedEvent, correlationId);

                // Act
                await MockTelemetryClient.WrappedOperationWithTelemetryDependencyInformationAsync(
                    () => Fixture.ChargePriceCommandReceivedTopic.SenderClient.SendMessageAsync(message), correlationId);

                await FunctionAsserts.AssertHasExecutedAsync(
                    Fixture.HostManager, nameof(ChargePriceCommandReceiverEndpoint));

                // Assert
                var actualOutboxMessage = context.OutboxMessages.Single(x => x.CorrelationId.Contains(correlationId));
                actualOutboxMessage.Type.Should().Be(_operationsRejectedEventType);
            }

            private static ChargePriceCommandReceivedEvent CreateInvalidChargePriceCommandReceivedEvent(
                ChargePriceCommandBuilder commandBuilder, ChargePriceOperationDtoBuilder operationDtoBuilder)
            {
                var invalidOperation = operationDtoBuilder.WithPoint(123456789m).Build();
                var priceCommand = commandBuilder.WithChargeOperation(invalidOperation).Build();
                var chargePriceReceivedEvent = new ChargePriceCommandReceivedEvent(
                    Instant.FromDateTimeUtc(DateTime.UtcNow), priceCommand);
                return chargePriceReceivedEvent;
            }

            private static ServiceBusMessage CreateServiceBusMessage(IInternalEvent internalEvent, string correlationId)
            {
                var applicationProperties = new Dictionary<string, string>
                {
                    { MessageMetaDataConstants.CorrelationId, correlationId },
                };
                var message = ServiceBusMessageGenerator.CreateWithJsonContent(
                    internalEvent, applicationProperties, correlationId);

                return message;
            }
        }
    }
}
