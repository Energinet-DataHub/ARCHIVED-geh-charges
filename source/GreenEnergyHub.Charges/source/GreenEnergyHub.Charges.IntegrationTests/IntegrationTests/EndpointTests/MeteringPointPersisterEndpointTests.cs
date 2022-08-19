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
using Energinet.DataHub.MeteringPoints.IntegrationEventContracts;
using FluentAssertions;
using Google.Protobuf;
using GreenEnergyHub.Charges.FunctionHost.MeteringPoint;
using GreenEnergyHub.Charges.Infrastructure.Persistence;
using GreenEnergyHub.Charges.IntegrationTest.Core.Fixtures.FunctionApp;
using GreenEnergyHub.Charges.IntegrationTest.Core.TestCommon;
using GreenEnergyHub.Charges.IntegrationTest.Core.TestHelpers;
using GreenEnergyHub.Charges.IntegrationTests.Fixtures;
using Xunit;
using Xunit.Abstractions;
using Xunit.Categories;
using static Energinet.DataHub.MeteringPoints.IntegrationEventContracts.MeteringPointCreated.Types;

namespace GreenEnergyHub.Charges.IntegrationTests.IntegrationTests.EndpointTests
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
            [InlineData(MeteringPointType.MptProduction, SettlementMethod.SmNull)]
            [InlineData(MeteringPointType.MptAnalysis, SettlementMethod.SmNull)]
            [InlineData(MeteringPointType.MptConsumption, SettlementMethod.SmFlex)]
            [InlineData(MeteringPointType.MptConsumption, SettlementMethod.SmProfiled)]
            [InlineData(MeteringPointType.MptConsumption, SettlementMethod.SmNonprofiled)]
            [InlineData(MeteringPointType.MptConsumptionFromGrid, SettlementMethod.SmNull)]
            [InlineData(MeteringPointType.MptElectricalHeating, SettlementMethod.SmNull)]
            [InlineData(MeteringPointType.MptExchange, SettlementMethod.SmNull)]
            [InlineData(MeteringPointType.MptExchangeReactiveEnergy, SettlementMethod.SmNull)]
            [InlineData(MeteringPointType.MptInternalUse, SettlementMethod.SmNull)]
            [InlineData(MeteringPointType.MptNetConsumption, SettlementMethod.SmNull)]
            [InlineData(MeteringPointType.MptNetFromGrid, SettlementMethod.SmNull)]
            [InlineData(MeteringPointType.MptNetProduction, SettlementMethod.SmNull)]
            [InlineData(MeteringPointType.MptNetToGrid, SettlementMethod.SmNull)]
            [InlineData(MeteringPointType.MptOtherConsumption, SettlementMethod.SmNull)]
            [InlineData(MeteringPointType.MptOtherProduction, SettlementMethod.SmNull)]
            [InlineData(MeteringPointType.MptSupplyToGrid, SettlementMethod.SmNull)]
            [InlineData(MeteringPointType.MptSurplusProductionGroup, SettlementMethod.SmNull)]
            [InlineData(MeteringPointType.MptVeproduction, SettlementMethod.SmNull)]
            [InlineData(MeteringPointType.MptTotalConsumption, SettlementMethod.SmNull)]
            [InlineData(MeteringPointType.MptWholesaleServices, SettlementMethod.SmNull)]
            public async Task When_ReceivingMeteringPointCreatedMessage_MeteringPointIsSavedToDatabase(
                MeteringPointType meteringPointType,
                SettlementMethod settlementMethod)
            {
                // Arrange
                var meteringPointId = RandomString(20);
                await using var chargeDbContext = Fixture.ChargesDatabaseManager.CreateDbContext();
                var message = CreateServiceBusMessage(
                    chargeDbContext,
                    meteringPointId,
                    meteringPointType,
                    settlementMethod,
                    out var correlationId,
                    out var parentId);

                // Act
                await MockTelemetryClient.WrappedOperationWithTelemetryDependencyInformationAsync(
                    () => Fixture.MeteringPointCreatedTopic.SenderClient.SendMessageAsync(message), correlationId, parentId);

                // Assert
                await FunctionAsserts.AssertHasExecutedAsync(Fixture.HostManager, nameof(MeteringPointPersisterEndpoint)).ConfigureAwait(false);
                await using var context = Fixture.ChargesDatabaseManager.CreateDbContext();
                var meteringPoint = context.MeteringPoints.SingleOrDefault(x => x.MeteringPointId == meteringPointId);
                meteringPoint.Should().NotBeNull();

                // We need to clear host log after each test is done to ensure that we can assert on function executed on each test run because we only check on function name.
                Fixture.HostManager.ClearHostLog();
            }

            private static readonly Random MyRandom = new Random();

            public static string RandomString(int length)
            {
                const string chars = "0123456789";
                return new string(Enumerable.Repeat(chars, length)
                    .Select(s => s[MyRandom.Next(s.Length)]).ToArray());
            }

            private static ServiceBusMessage CreateServiceBusMessage(
                IChargesDatabaseContext chargesDatabaseContext,
                string meteringPointId,
                MeteringPointType meteringPointType,
                SettlementMethod settlementMethod,
                out string correlationId,
                out string parentId)
            {
                var gridAreaLinkId = chargesDatabaseContext.GridAreaLinks.First().Id;
                var date = new DateTime(2021, 1, 2, 3, 4, 5, DateTimeKind.Utc);
                correlationId = CorrelationIdGenerator.Create();
                var message = new MeteringPointCreated
                {
                    MeteringPointId = meteringPointId,
                    ConnectionState = ConnectionState.CsNew,
                    EffectiveDate = new DateTime(2020, 01, 01, 0, 0, 0).ToProtoBufTimestamp(),
                    GridAreaCode = gridAreaLinkId.ToString(),
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
