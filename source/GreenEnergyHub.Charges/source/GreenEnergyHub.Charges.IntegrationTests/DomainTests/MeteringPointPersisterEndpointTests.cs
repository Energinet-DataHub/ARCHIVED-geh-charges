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
using Energinet.DataHub.Core.FunctionApp.TestCommon.FunctionAppHost;
using Energinet.DataHub.Core.FunctionApp.TestCommon.ServiceBus.ListenerMock;
using Energinet.DataHub.Core.TestCommon;
using Energinet.DataHub.MeteringPoints.IntegrationEventContracts;
using FluentAssertions;
using Google.Protobuf;
using GreenEnergyHub.Charges.Domain.MeteringPoints;
using GreenEnergyHub.Charges.FunctionHost.MeteringPoint;
using GreenEnergyHub.Charges.Infrastructure.Contracts.External.MeteringPointCreated;
using GreenEnergyHub.Charges.IntegrationTests.Fixtures;
using GreenEnergyHub.Charges.IntegrationTests.TestHelpers;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Xunit.Abstractions;
using Xunit.Categories;
using static Energinet.DataHub.MeteringPoints.IntegrationEventContracts.MeteringPointCreated.Types;

namespace GreenEnergyHub.Charges.IntegrationTests.DomainTests
{
    [IntegrationTest]
    public class MeteringPointPersisterEndpointTests
    {
        [Collection(nameof(ChargesFunctionAppCollectionFixture))]
        public class RunAsync : FunctionAppTestBase<ChargesFunctionAppFixture>, IAsyncLifetime
        {
            private TimeSpan DefaultTimeout { get; } = TimeSpan.FromSeconds(10);

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
            [InlineData(MeteringPointCreated.Types.MeteringPointType.MptProduction, MeteringMethod.MmPhysical, MeteringPointCreated.Types.SettlementMethod.SmUnknown)]
            [InlineData(MeteringPointCreated.Types.MeteringPointType.MptProduction, MeteringMethod.MmVirtual, MeteringPointCreated.Types.SettlementMethod.SmUnknown)]
            [InlineData(MeteringPointCreated.Types.MeteringPointType.MptAnalysis, MeteringMethod.MmPhysical, MeteringPointCreated.Types.SettlementMethod.SmUnknown)]
            [InlineData(MeteringPointCreated.Types.MeteringPointType.MptAnalysis, MeteringMethod.MmVirtual, MeteringPointCreated.Types.SettlementMethod.SmUnknown)]
            [InlineData(MeteringPointCreated.Types.MeteringPointType.MptConsumption, MeteringMethod.MmPhysical, MeteringPointCreated.Types.SettlementMethod.SmFlex)]
            [InlineData(MeteringPointCreated.Types.MeteringPointType.MptConsumption, MeteringMethod.MmPhysical, MeteringPointCreated.Types.SettlementMethod.SmProfiled)]
            [InlineData(MeteringPointCreated.Types.MeteringPointType.MptConsumption, MeteringMethod.MmPhysical, MeteringPointCreated.Types.SettlementMethod.SmNonprofiled)]
            [InlineData(MeteringPointCreated.Types.MeteringPointType.MptConsumption, MeteringMethod.MmVirtual, MeteringPointCreated.Types.SettlementMethod.SmFlex)]
            [InlineData(MeteringPointCreated.Types.MeteringPointType.MptConsumption, MeteringMethod.MmVirtual, MeteringPointCreated.Types.SettlementMethod.SmProfiled)]
            [InlineData(MeteringPointCreated.Types.MeteringPointType.MptConsumption, MeteringMethod.MmVirtual, MeteringPointCreated.Types.SettlementMethod.SmNonprofiled)]
            [InlineData(MeteringPointCreated.Types.MeteringPointType.MptConsumptionFromGrid, MeteringMethod.MmPhysical, MeteringPointCreated.Types.SettlementMethod.SmUnknown)]
            [InlineData(MeteringPointCreated.Types.MeteringPointType.MptConsumptionFromGrid, MeteringMethod.MmVirtual, MeteringPointCreated.Types.SettlementMethod.SmUnknown)]
            [InlineData(MeteringPointCreated.Types.MeteringPointType.MptElectricalHeating, MeteringMethod.MmVirtual, MeteringPointCreated.Types.SettlementMethod.SmUnknown)]
            [InlineData(MeteringPointCreated.Types.MeteringPointType.MptExchange, MeteringMethod.MmPhysical, MeteringPointCreated.Types.SettlementMethod.SmUnknown)]
            [InlineData(MeteringPointCreated.Types.MeteringPointType.MptExchange, MeteringMethod.MmVirtual, MeteringPointCreated.Types.SettlementMethod.SmUnknown)]
            [InlineData(MeteringPointCreated.Types.MeteringPointType.MptExchangeReactiveEnergy, MeteringMethod.MmPhysical, MeteringPointCreated.Types.SettlementMethod.SmUnknown)]
            [InlineData(MeteringPointCreated.Types.MeteringPointType.MptExchangeReactiveEnergy, MeteringMethod.MmVirtual, MeteringPointCreated.Types.SettlementMethod.SmUnknown)]
            [InlineData(MeteringPointCreated.Types.MeteringPointType.MptInternalUse, MeteringMethod.MmPhysical, MeteringPointCreated.Types.SettlementMethod.SmUnknown)]
            [InlineData(MeteringPointCreated.Types.MeteringPointType.MptInternalUse, MeteringMethod.MmVirtual, MeteringPointCreated.Types.SettlementMethod.SmUnknown)]
            [InlineData(MeteringPointCreated.Types.MeteringPointType.MptNetConsumption, MeteringMethod.MmPhysical, MeteringPointCreated.Types.SettlementMethod.SmUnknown)]
            [InlineData(MeteringPointCreated.Types.MeteringPointType.MptNetConsumption, MeteringMethod.MmVirtual, MeteringPointCreated.Types.SettlementMethod.SmUnknown)]
            [InlineData(MeteringPointCreated.Types.MeteringPointType.MptNetFromGrid, MeteringMethod.MmPhysical, MeteringPointCreated.Types.SettlementMethod.SmUnknown)]
            [InlineData(MeteringPointCreated.Types.MeteringPointType.MptNetFromGrid, MeteringMethod.MmVirtual, MeteringPointCreated.Types.SettlementMethod.SmUnknown)]
            [InlineData(MeteringPointCreated.Types.MeteringPointType.MptNetProduction, MeteringMethod.MmPhysical, MeteringPointCreated.Types.SettlementMethod.SmUnknown)]
            [InlineData(MeteringPointCreated.Types.MeteringPointType.MptNetProduction, MeteringMethod.MmVirtual, MeteringPointCreated.Types.SettlementMethod.SmUnknown)]
            [InlineData(MeteringPointCreated.Types.MeteringPointType.MptNetToGrid, MeteringMethod.MmPhysical, MeteringPointCreated.Types.SettlementMethod.SmUnknown)]
            [InlineData(MeteringPointCreated.Types.MeteringPointType.MptNetToGrid, MeteringMethod.MmVirtual, MeteringPointCreated.Types.SettlementMethod.SmUnknown)]
            [InlineData(MeteringPointCreated.Types.MeteringPointType.MptOtherConsumption, MeteringMethod.MmPhysical, MeteringPointCreated.Types.SettlementMethod.SmUnknown)]
            [InlineData(MeteringPointCreated.Types.MeteringPointType.MptOtherConsumption, MeteringMethod.MmVirtual, MeteringPointCreated.Types.SettlementMethod.SmUnknown)]
            [InlineData(MeteringPointCreated.Types.MeteringPointType.MptOtherProduction, MeteringMethod.MmPhysical, MeteringPointCreated.Types.SettlementMethod.SmUnknown)]
            [InlineData(MeteringPointCreated.Types.MeteringPointType.MptOtherProduction, MeteringMethod.MmVirtual, MeteringPointCreated.Types.SettlementMethod.SmUnknown)]
            [InlineData(MeteringPointCreated.Types.MeteringPointType.MptSupplyToGrid, MeteringMethod.MmPhysical, MeteringPointCreated.Types.SettlementMethod.SmUnknown)]
            [InlineData(MeteringPointCreated.Types.MeteringPointType.MptSupplyToGrid, MeteringMethod.MmVirtual, MeteringPointCreated.Types.SettlementMethod.SmUnknown)]
            [InlineData(MeteringPointCreated.Types.MeteringPointType.MptSurplusProductionGroup, MeteringMethod.MmPhysical, MeteringPointCreated.Types.SettlementMethod.SmUnknown)]
            [InlineData(MeteringPointCreated.Types.MeteringPointType.MptSurplusProductionGroup, MeteringMethod.MmVirtual, MeteringPointCreated.Types.SettlementMethod.SmUnknown)]
            [InlineData(MeteringPointCreated.Types.MeteringPointType.MptVeproduction, MeteringMethod.MmPhysical, MeteringPointCreated.Types.SettlementMethod.SmUnknown)]
            [InlineData(MeteringPointCreated.Types.MeteringPointType.MptVeproduction, MeteringMethod.MmVirtual, MeteringPointCreated.Types.SettlementMethod.SmUnknown)]
            [InlineData(MeteringPointCreated.Types.MeteringPointType.MptTotalConsumption, MeteringMethod.MmPhysical, MeteringPointCreated.Types.SettlementMethod.SmUnknown)]
            [InlineData(MeteringPointCreated.Types.MeteringPointType.MptTotalConsumption, MeteringMethod.MmVirtual, MeteringPointCreated.Types.SettlementMethod.SmUnknown)]
            [InlineData(MeteringPointCreated.Types.MeteringPointType.MptWholesaleServices, MeteringMethod.MmPhysical, MeteringPointCreated.Types.SettlementMethod.SmUnknown)]
            [InlineData(MeteringPointCreated.Types.MeteringPointType.MptWholesaleServices, MeteringMethod.MmVirtual, MeteringPointCreated.Types.SettlementMethod.SmUnknown)]
            public async Task When_ReceivingMeteringPointCreatedMessage_MeteringPointIsSavedToDatabase(
                MeteringPointCreated.Types.MeteringPointType meteringPointType,
                MeteringMethod meteringMethod,
                MeteringPointCreated.Types.SettlementMethod settlementMethod)
            {
                // Arrange
                var meteringPointId = RandomString(20);
                var message = CreateServiceBusMessage(
                    meteringPointId,
                    meteringPointType,
                    meteringMethod,
                    settlementMethod,
                    out var correlationId,
                    out var parentId);

                // Act
                await MockTelemetryClient.WrappedOperationWithTelemetryDependencyInformationAsync(
                    () => Fixture.MeteringPointCreatedTopic.SenderClient.SendMessageAsync(message), correlationId, parentId);

                // Assert
                await AssertFunctionExecuted(Fixture.HostManager, nameof(MeteringPointPersisterEndpoint)).ConfigureAwait(false);
                await using var context = Fixture.DatabaseManager.CreateDbContext();
                var meteringPoint = context.MeteringPoints.SingleOrDefault(x => x.MeteringPointId == meteringPointId);
                meteringPoint.Should().NotBeNull();

                // We need to clear host log after each test is done to ensure that we can assert on function executed on each test run because we only check on function name.
                Fixture.HostManager.ClearHostLog();
            }

            private static Random random = new Random();

            public static string RandomString(int length)
            {
                const string chars = "0123456789";
                return new string(Enumerable.Repeat(chars, length)
                    .Select(s => s[random.Next(s.Length)]).ToArray());
            }

            private static async Task AssertFunctionExecuted(FunctionAppHostManager hostManager, string functionName)
            {
                var waitTimespan = TimeSpan.FromSeconds(10);

                var functionExecuted = await Awaiter
                    .TryWaitUntilConditionAsync(
                        () => hostManager.CheckIfFunctionWasExecuted(
                            $"Functions.{functionName}"),
                        waitTimespan)
                    .ConfigureAwait(false);
                functionExecuted.Should().BeTrue($"{functionName} was expected to run.");
            }

            private ServiceBusMessage CreateServiceBusMessage(
                string meteringPointId,
                MeteringPointCreated.Types.MeteringPointType meteringPointType,
                MeteringMethod meteringMethod,
                MeteringPointCreated.Types.SettlementMethod settlementMethod,
                out string correlationId,
                out string parentId)
            {
                var date = new DateTime(2021, 1, 2, 3, 4, 5, DateTimeKind.Utc);
                correlationId = CorrelationIdGenerator.Create();
                var message = new MeteringPointCreated
                {
                    MeteringPointId = meteringPointId,
                    ConnectionState = MeteringPointCreated.Types.ConnectionState.CsNew,
                    EffectiveDate = new DateTime(2020, 01, 01, 0, 0, 0).ToProtoBufTimestamp(),
                    GridAreaCode = "001",
                    GsrnNumber = meteringPointId,
                    MeteringPointType = meteringPointType,
                    MeteringMethod = meteringMethod,
                    SettlementMethod = settlementMethod,
                };
                parentId = $"00-{correlationId}-b7ad6b7169203331-01";

                var byteArray = message.ToByteArray();
                var serviceBusMessage = new ServiceBusMessage(byteArray)
                {
                    CorrelationId = (string)correlationId,
                };
                serviceBusMessage.ApplicationProperties.Add("OperationTimestamp", date.ToUniversalTime());
                serviceBusMessage.ApplicationProperties.Add("OperationCorrelationId", "1bf1b76337f14b78badc248a3289d021");
                serviceBusMessage.ApplicationProperties.Add("MessageVersion", 1);
                serviceBusMessage.ApplicationProperties.Add("MessageType", "MeteringPointCreated");
                serviceBusMessage.ApplicationProperties.Add("EventIdentification", "2542ed0d242e46b68b8b803e93ffbf7b");
                return serviceBusMessage;
            }
        }
    }
}
