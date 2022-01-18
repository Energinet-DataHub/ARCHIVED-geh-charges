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
using GreenEnergyHub.Charges.FunctionHost.MeteringPoint;
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
            [InlineData(MeteringPointType.MptProduction, MeteringMethod.MmPhysical, SettlementMethod.SmUnknown)]
            [InlineData(MeteringPointType.MptProduction, MeteringMethod.MmVirtual, SettlementMethod.SmUnknown)]
            [InlineData(MeteringPointType.MptAnalysis, MeteringMethod.MmPhysical, SettlementMethod.SmUnknown)]
            [InlineData(MeteringPointType.MptAnalysis, MeteringMethod.MmVirtual, SettlementMethod.SmUnknown)]
            [InlineData(MeteringPointType.MptConsumption, MeteringMethod.MmPhysical, SettlementMethod.SmFlex)]
            [InlineData(MeteringPointType.MptConsumption, MeteringMethod.MmPhysical, SettlementMethod.SmProfiled)]
            [InlineData(MeteringPointType.MptConsumption, MeteringMethod.MmPhysical, SettlementMethod.SmNonprofiled)]
            [InlineData(MeteringPointType.MptConsumption, MeteringMethod.MmVirtual, SettlementMethod.SmFlex)]
            [InlineData(MeteringPointType.MptConsumption, MeteringMethod.MmVirtual, SettlementMethod.SmProfiled)]
            [InlineData(MeteringPointType.MptConsumption, MeteringMethod.MmVirtual, SettlementMethod.SmNonprofiled)]
            [InlineData(MeteringPointType.MptConsumptionFromGrid, MeteringMethod.MmPhysical, SettlementMethod.SmUnknown)]
            [InlineData(MeteringPointType.MptConsumptionFromGrid, MeteringMethod.MmVirtual, SettlementMethod.SmUnknown)]
            [InlineData(MeteringPointType.MptElectricalHeating, MeteringMethod.MmVirtual, SettlementMethod.SmUnknown)]
            [InlineData(MeteringPointType.MptExchange, MeteringMethod.MmPhysical, SettlementMethod.SmUnknown)]
            [InlineData(MeteringPointType.MptExchange, MeteringMethod.MmVirtual, SettlementMethod.SmUnknown)]
            [InlineData(MeteringPointType.MptExchangeReactiveEnergy, MeteringMethod.MmPhysical, SettlementMethod.SmUnknown)]
            [InlineData(MeteringPointType.MptExchangeReactiveEnergy, MeteringMethod.MmVirtual, SettlementMethod.SmUnknown)]
            [InlineData(MeteringPointType.MptInternalUse, MeteringMethod.MmPhysical, SettlementMethod.SmUnknown)]
            [InlineData(MeteringPointType.MptInternalUse, MeteringMethod.MmVirtual, SettlementMethod.SmUnknown)]
            [InlineData(MeteringPointType.MptNetConsumption, MeteringMethod.MmPhysical, SettlementMethod.SmUnknown)]
            [InlineData(MeteringPointType.MptNetConsumption, MeteringMethod.MmVirtual, SettlementMethod.SmUnknown)]
            [InlineData(MeteringPointType.MptNetFromGrid, MeteringMethod.MmPhysical, SettlementMethod.SmUnknown)]
            [InlineData(MeteringPointType.MptNetFromGrid, MeteringMethod.MmVirtual, SettlementMethod.SmUnknown)]
            [InlineData(MeteringPointType.MptNetProduction, MeteringMethod.MmPhysical, SettlementMethod.SmUnknown)]
            [InlineData(MeteringPointType.MptNetProduction, MeteringMethod.MmVirtual, SettlementMethod.SmUnknown)]
            [InlineData(MeteringPointType.MptNetToGrid, MeteringMethod.MmPhysical, SettlementMethod.SmUnknown)]
            [InlineData(MeteringPointType.MptNetToGrid, MeteringMethod.MmVirtual, SettlementMethod.SmUnknown)]
            [InlineData(MeteringPointType.MptOtherConsumption, MeteringMethod.MmPhysical, SettlementMethod.SmUnknown)]
            [InlineData(MeteringPointType.MptOtherConsumption, MeteringMethod.MmVirtual, SettlementMethod.SmUnknown)]
            [InlineData(MeteringPointType.MptOtherProduction, MeteringMethod.MmPhysical, SettlementMethod.SmUnknown)]
            [InlineData(MeteringPointType.MptOtherProduction, MeteringMethod.MmVirtual, SettlementMethod.SmUnknown)]
            [InlineData(MeteringPointType.MptSupplyToGrid, MeteringMethod.MmPhysical, SettlementMethod.SmUnknown)]
            [InlineData(MeteringPointType.MptSupplyToGrid, MeteringMethod.MmVirtual, SettlementMethod.SmUnknown)]
            [InlineData(MeteringPointType.MptSurplusProductionGroup, MeteringMethod.MmPhysical, SettlementMethod.SmUnknown)]
            [InlineData(MeteringPointType.MptSurplusProductionGroup, MeteringMethod.MmVirtual, SettlementMethod.SmUnknown)]
            [InlineData(MeteringPointType.MptVeproduction, MeteringMethod.MmPhysical, SettlementMethod.SmUnknown)]
            [InlineData(MeteringPointType.MptVeproduction, MeteringMethod.MmVirtual, SettlementMethod.SmUnknown)]
            [InlineData(MeteringPointType.MptTotalConsumption, MeteringMethod.MmPhysical, SettlementMethod.SmUnknown)]
            [InlineData(MeteringPointType.MptTotalConsumption, MeteringMethod.MmVirtual, SettlementMethod.SmUnknown)]
            [InlineData(MeteringPointType.MptWholesaleServices, MeteringMethod.MmPhysical, SettlementMethod.SmUnknown)]
            [InlineData(MeteringPointType.MptWholesaleServices, MeteringMethod.MmVirtual, SettlementMethod.SmUnknown)]
            public async Task When_ReceivingMeteringPointCreatedMessage_MeteringPointIsSavedToDatabase(
                MeteringPointType meteringPointType,
                MeteringMethod meteringMethod,
                SettlementMethod settlementMethod)
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
                context.MeteringPoints.SingleOrDefaultAsync(x => x.MeteringPointId == meteringPointId).GetAwaiter().Should().NotBeNull();
            }

            public static IEnumerable<object[]> GetMeteringPointScenarios()
            {
                yield return new object[]
                {
                    MeteringPointType.MptProduction,
                    MeteringMethod.MmPhysical,
                    SettlementMethod.SmUnknown,
                    RandomString(20),
                };
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
                MeteringPointType meteringPointType,
                MeteringMethod meteringMethod,
                SettlementMethod settlementMethod,
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
