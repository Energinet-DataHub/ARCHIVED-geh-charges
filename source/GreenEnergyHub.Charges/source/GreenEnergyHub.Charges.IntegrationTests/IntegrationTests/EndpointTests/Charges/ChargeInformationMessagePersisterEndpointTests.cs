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

using System.Linq;
using System.Threading.Tasks;
using Energinet.DataHub.Core.FunctionApp.TestCommon;
using Energinet.DataHub.Core.TestCommon.AutoFixture.Attributes;
using FluentAssertions;
using GreenEnergyHub.Charges.FunctionHost.Charges;
using GreenEnergyHub.Charges.IntegrationTest.Core.Fixtures.Database;
using GreenEnergyHub.Charges.IntegrationTest.Core.Fixtures.FunctionApp;
using GreenEnergyHub.Charges.IntegrationTest.Core.TestCommon;
using GreenEnergyHub.Charges.IntegrationTest.Core.TestHelpers;
using GreenEnergyHub.Charges.IntegrationTests.Fixtures;
using GreenEnergyHub.Charges.TestCore.Builders.Command;
using Xunit;
using Xunit.Abstractions;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.IntegrationTests.IntegrationTests.EndpointTests.Charges
{
    [IntegrationTest]
    public class ChargeInformationMessagePersisterEndpointTests
    {
        [Collection(nameof(ChargesFunctionAppCollectionFixture))]
        public class RunAsync : FunctionAppTestBase<ChargesFunctionAppFixture>, IAsyncLifetime
        {
            private readonly ChargesDatabaseManager _databaseManager;

            public RunAsync(ChargesFunctionAppFixture fixture, ITestOutputHelper testOutputHelper)
                : base(fixture, testOutputHelper)
            {
                _databaseManager = fixture.ChargesDatabaseManager;
            }

            public Task DisposeAsync()
            {
                // We need to clear host log after each test is done to ensure that we can assert
                // on function executed on each test run because we only check on function name.
                Fixture.HostManager.ClearHostLog();
                return Task.CompletedTask;
            }

            public Task InitializeAsync()
            {
                return Task.CompletedTask;
            }

            [Theory]
            [InlineAutoMoqData]
            public async Task RunAsync_WhenChargeInformationOperationsAcceptedEvent_ChargeMessageIsPersisted(
                ChargeInformationOperationsAcceptedEventBuilder chargeInformationOperationsAcceptedEventBuilder)
            {
                // Arrange
                await using var chargesDatabaseReadContext = _databaseManager.CreateDbContext();

                var operationsConfirmedEvent = chargeInformationOperationsAcceptedEventBuilder.Build();
                var documentId = operationsConfirmedEvent.Document.Id;
                var operationDto = operationsConfirmedEvent.Operations.Last();

                var correlationId = CorrelationIdGenerator.Create();
                var message = ServiceBusMessageGenerator.CreateServiceBusMessage(operationsConfirmedEvent, correlationId);

                // Act
                await MockTelemetryClient.WrappedOperationWithTelemetryDependencyInformationAsync(
                    () => Fixture.ChargesDomainEventTopic.SenderClient.SendMessageAsync(message), correlationId);

                // Assert
                await FunctionAsserts.AssertHasExecutedAsync(
                    Fixture.HostManager, nameof(ChargeInformationMessagePersisterEndpoint));

                var actual = chargesDatabaseReadContext.ChargeMessages.Single(x => x.MessageId == documentId);
                actual.SenderProvidedChargeId.Should().Be(operationDto.SenderProvidedChargeId);
                actual.Type.Should().Be(operationDto.ChargeType);
                actual.MarketParticipantId.Should().Be(operationDto.ChargeOwner);
            }
        }
    }
}
