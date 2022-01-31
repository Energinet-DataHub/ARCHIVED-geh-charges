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
using GreenEnergyHub.Charges.IntegrationTests.Fixtures.FunctionApp;
using GreenEnergyHub.Charges.IntegrationTests.TestCommon;
using GreenEnergyHub.Charges.IntegrationTests.TestHelpers;
using Xunit;
using Xunit.Abstractions;
using Xunit.Categories;
using static Energinet.DataHub.MeteringPoints.IntegrationEventContracts.MeteringPointCreated.Types;

namespace GreenEnergyHub.Charges.IntegrationTests.DomainTests
{
    [IntegrationTest]
    public class CreateDefaultChargeLinksReplierTests
    {
        [Collection(nameof(ChargesFunctionAppCollectionFixture))]
        public class RunAsync : FunctionAppTestBase<ChargesFunctionAppFixture>, IAsyncLifetime
        {
            public RunAsync(ChargesFunctionAppFixture fixture, ITestOutputHelper testOutputHelper)
                : base(fixture, testOutputHelper)
            {
            }

            public Task InitializeAsync()
            {
                return Task.CompletedTask;
            }

            public Task DisposeAsync()
            {
                Fixture.MessageHubMock.Clear();
                return Task.CompletedTask;
            }

            [Fact]
            public async Task When_()
            {
                /* Send Metering Point Created event
                 * Send Create Default Charge Link event
                 * Assert message on CreateDefaultChargeLinksReply queue
                 */

                // Arrange
                var meteringPointId = "571337000000000006";
                var message = CreateMeteringPointCreatedServiceBusMessage(
                    Fixture.DatabaseManager.CreateDbContext(),
                    meteringPointId,
                    MeteringPointType.MptConsumption,
                    SettlementMethod.SmFlex,
                    out var correlationId,
                    out var parentId);

                await MockTelemetryClient.WrappedOperationWithTelemetryDependencyInformationAsync(
                    () => Fixture.MeteringPointCreatedTopic.SenderClient.SendMessageAsync(message), correlationId, parentId);

                await FunctionAsserts.AssertHasExecutedAsync(Fixture.HostManager, nameof(MeteringPointPersisterEndpoint)).ConfigureAwait(false);

                await using var context = Fixture.DatabaseManager.CreateDbContext();
                var meteringPoint = context.MeteringPoints.SingleOrDefault(x => x.MeteringPointId == meteringPointId);
                meteringPoint.Should().NotBeNull();

                // We need to clear host log after each test is done to ensure that we can assert on function executed on each test run because we only check on function name.
                Fixture.HostManager.ClearHostLog();
            }

            private ServiceBusMessage CreateMeteringPointCreatedServiceBusMessage(
                IChargesDatabaseContext chargesDatabaseContext,
                string meteringPointId,
                MeteringPointType meteringPointType,
                SettlementMethod settlementMethod,
                out string correlationId,
                out string parentId)
            {
                var gridAreaLinkId = chargesDatabaseContext.GridAreaLinks.First().Id;
                var date = new DateTime(2022, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                correlationId = CorrelationIdGenerator.Create();
                var message = new MeteringPointCreated
                {
                    MeteringPointId = meteringPointId,
                    ConnectionState = ConnectionState.CsNew,
                    EffectiveDate = date.ToProtoBufTimestamp(),
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
                serviceBusMessage.ApplicationProperties.Add("OperationCorrelationId", "1bf1b76337f14b78badc248a3289d022");
                serviceBusMessage.ApplicationProperties.Add("MessageVersion", 1);
                serviceBusMessage.ApplicationProperties.Add("MessageType", "MeteringPointCreated");
                serviceBusMessage.ApplicationProperties.Add("EventIdentification", "2542ed0d242e46b68b8b803e93ffbf7c");
                return serviceBusMessage;
            }
        }
    }
}
