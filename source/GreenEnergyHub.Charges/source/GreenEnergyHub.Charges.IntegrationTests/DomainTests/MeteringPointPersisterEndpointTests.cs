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
using System.Linq;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Energinet.DataHub.Core.FunctionApp.TestCommon;
using Energinet.DataHub.Core.FunctionApp.TestCommon.FunctionAppHost;
using Energinet.DataHub.Core.TestCommon;
using Energinet.DataHub.MeteringPoints.IntegrationEventContracts;
using FluentAssertions;
using Google.Protobuf;
using GreenEnergyHub.Charges.FunctionHost.MeteringPoint;
using GreenEnergyHub.Charges.Infrastructure.Contracts.External.MeteringPointCreated;
using GreenEnergyHub.Charges.IntegrationTests.Fixtures;
using GreenEnergyHub.Charges.IntegrationTests.TestHelpers;
using Xunit;
using Xunit.Abstractions;
using Xunit.Categories;
using static Energinet.DataHub.MeteringPoints.IntegrationEventContracts.MeteringPointCreated.Types;
using ConnectionState = GreenEnergyHub.Charges.Domain.MeteringPoints.ConnectionState;

namespace GreenEnergyHub.Charges.IntegrationTests.DomainTests
{
    [IntegrationTest]
    public class MeteringPointPersisterEndpointTests
    {
        [Collection(nameof(ChargesFunctionAppCollectionFixture))]
        public class RunAsync : FunctionAppTestBase<ChargesFunctionAppFixture>, IAsyncLifetime
        {
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
            [InlineData(MeteringPointType.MptProduction, SettlementMethod.SmUnknown)]
            [InlineData(MeteringPointType.MptAnalysis, SettlementMethod.SmUnknown)]
            [InlineData(MeteringPointType.MptConsumption, SettlementMethod.SmFlex)]
            [InlineData(MeteringPointType.MptConsumption, SettlementMethod.SmProfiled)]
            [InlineData(MeteringPointType.MptConsumption, SettlementMethod.SmNonprofiled)]
            [InlineData(MeteringPointType.MptConsumptionFromGrid, SettlementMethod.SmUnknown)]
            [InlineData(MeteringPointType.MptElectricalHeating, SettlementMethod.SmUnknown)]
            [InlineData(MeteringPointType.MptExchange, SettlementMethod.SmUnknown)]
            [InlineData(MeteringPointType.MptExchangeReactiveEnergy, SettlementMethod.SmUnknown)]
            [InlineData(MeteringPointType.MptInternalUse, SettlementMethod.SmUnknown)]
            [InlineData(MeteringPointType.MptNetConsumption, SettlementMethod.SmUnknown)]
            [InlineData(MeteringPointType.MptNetFromGrid, SettlementMethod.SmUnknown)]
            [InlineData(MeteringPointType.MptNetProduction, SettlementMethod.SmUnknown)]
            [InlineData(MeteringPointType.MptNetToGrid, SettlementMethod.SmUnknown)]
            [InlineData(MeteringPointType.MptOtherConsumption, SettlementMethod.SmUnknown)]
            [InlineData(MeteringPointType.MptOtherProduction, SettlementMethod.SmUnknown)]
            [InlineData(MeteringPointType.MptSupplyToGrid, SettlementMethod.SmUnknown)]
            [InlineData(MeteringPointType.MptSurplusProductionGroup, SettlementMethod.SmUnknown)]
            [InlineData(MeteringPointType.MptVeproduction, SettlementMethod.SmUnknown)]
            [InlineData(MeteringPointType.MptTotalConsumption, SettlementMethod.SmUnknown)]
            [InlineData(MeteringPointType.MptWholesaleServices, SettlementMethod.SmUnknown)]
            public async Task When_ReceivingMeteringPointCreatedMessage_MeteringPointIsSavedToDatabase(
                MeteringPointType meteringPointType,
                SettlementMethod settlementMethod)
            {
                // Arrange
                var meteringPointId = RandomString(20);
                var message = CreateServiceBusMessage(
                    meteringPointId,
                    meteringPointType,
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
                meteringPoint!.MeteringPointType.Should()
                    .Be(MeteringPointCreatedInboundMapper.MapMeteringPointType(meteringPointType));
                meteringPoint!.ConnectionState.Should().Be(ConnectionState.New);
                meteringPoint!.SettlementMethod.Should()
                    .Be(MeteringPointCreatedInboundMapper.MapSettlementMethod(settlementMethod));

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
                MeteringPointType meteringPointType,
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
                    MeteringMethod = MeteringMethod.MmPhysical,
                    SettlementMethod = settlementMethod,
                };
                parentId = $"00-{correlationId}-b7ad6b7169203331-01";

                var byteArray = message.ToByteArray();
                var serviceBusMessage = new ServiceBusMessage(byteArray)
                {
                    CorrelationId = correlationId,
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
