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
    public class ChargeInformationHistoryPersisterEndpointTests
    {
        [Collection(nameof(ChargesFunctionAppCollectionFixture))]
        public class RunAsync : FunctionAppTestBase<ChargesFunctionAppFixture>, IAsyncLifetime
        {
            private readonly ChargesQueryDatabaseManager _databaseManager;

            public RunAsync(ChargesFunctionAppFixture fixture, ITestOutputHelper testOutputHelper)
                : base(fixture, testOutputHelper)
            {
                _databaseManager = fixture.ChargesQueryDatabaseManager;
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
            public async Task RunAsync_WhenChargeInformationOperationsAcceptedEvent_ChargeHistoryIsPersisted(
                ChargeInformationOperationsAcceptedEventBuilder chargeInformationOperationsAcceptedEventBuilder)
            {
                // Arrange
                await using var chargesQueryDatabaseReadContext = _databaseManager.CreateDbContext();

                var operationsAcceptedEvent = chargeInformationOperationsAcceptedEventBuilder.Build();
                var operationDto = operationsAcceptedEvent.Operations.Last();
                var senderProvidedChargeId = operationDto.SenderProvidedChargeId;
                var chargeType = operationDto.ChargeType;
                var chargeOwner = operationDto.ChargeOwner;

                var correlationId = CorrelationIdGenerator.Create();
                var message = ServiceBusMessageGenerator.CreateServiceBusMessage(operationsAcceptedEvent, correlationId);

                // Act
                await MockTelemetryClient.WrappedOperationWithTelemetryDependencyInformationAsync(
                    () => Fixture.ChargesDomainEventTopic.SenderClient.SendMessageAsync(message), correlationId);

                // Assert
                await FunctionAsserts.AssertHasExecutedAsync(
                    Fixture.HostManager, nameof(ChargeInformationHistoryPersisterEndpoint));

                var actual = chargesQueryDatabaseReadContext.ChargeInformationHistories
                    .Single(x => x.SenderProvidedChargeId == senderProvidedChargeId && x.Type == chargeType && x.Owner == chargeOwner);
                actual.Name.Should().Be(operationDto.ChargeName);
                actual.Description.Should().Be(operationDto.ChargeDescription);
                actual.Resolution.Should().Be(operationDto.Resolution);
                actual.TaxIndicator.Should().Be(operationDto.TaxIndicator);
                actual.TransparentInvoicing.Should().Be(operationDto.TransparentInvoicing);
                actual.VatClassification.Should().Be(operationDto.VatClassification);
                actual.StartDateTime.Should().Be(operationDto.StartDateTime);
                actual.EndDateTime.Should().Be(operationDto.EndDateTime);
                actual.AcceptedDateTime.Should().Be(operationsAcceptedEvent.PublishedTime);
            }
        }
    }
}
