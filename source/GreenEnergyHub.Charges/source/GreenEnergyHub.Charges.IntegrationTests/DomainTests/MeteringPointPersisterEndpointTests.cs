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
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Energinet.DataHub.Core.FunctionApp.TestCommon;
using Energinet.DataHub.MeteringPoints.IntegrationEventContracts;
using Google.Protobuf;
using GreenEnergyHub.Charges.IntegrationTests.Fixtures;
using GreenEnergyHub.Charges.IntegrationTests.TestHelpers;
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

            [Fact]
            public async Task When_ReceivingMeteringPointCreatedMessage_MeteringPointIsSavedToDatabase()
            {
                // Arrange
                var meteringPointId = "701313180000000005";
                var request = CreateServiceBusMessage(
                    meteringPointId,
                    MeteringPointType.MptProduction,
                    MeteringMethod.MmPhysical,
                    SettlementMethod.SmUnknown,
                    out var correlationId,
                    out var parentId);
            }

            private ServiceBusMessage CreateServiceBusMessage(
                string meteringPointId,
                MeteringPointType meteringPointType,
                MeteringMethod meteringMethod,
                SettlementMethod settlementMethod,
                out object correlationId,
                out object parentId)
            {
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
                return serviceBusMessage;
            }
        }
    }
}
