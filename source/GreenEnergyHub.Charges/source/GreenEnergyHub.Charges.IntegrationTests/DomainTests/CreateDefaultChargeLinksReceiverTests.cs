﻿// Copyright 2020 Energinet DataHub A/S
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
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Energinet.Charges.Contracts;
using Energinet.DataHub.Core.FunctionApp.TestCommon;
using Google.Protobuf;
using GreenEnergyHub.Charges.IntegrationTests.Fixtures;
using GreenEnergyHub.Charges.IntegrationTests.TestHelpers;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Xunit;
using Xunit.Abstractions;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.IntegrationTests.DomainTests
{
    [IntegrationTest]
    public class CreateDefaultChargeLinksReceiverTests
    {
        [Collection(nameof(ChargesFunctionAppCollectionFixture))]
        public class RunAsync : FunctionAppTestBase<ChargesFunctionAppFixture>, IAsyncLifetime
        {
            public RunAsync(ChargesFunctionAppFixture fixture, ITestOutputHelper testOutputHelper)
                : base(fixture, testOutputHelper)
            {
            }

            [Fact]
            public async Task When_ReceivingCreateDefaultChargeLinksRequest_MessageHubIsNotifiedAboutAvailableData_And_Then_When_MessageHubRequestsTheBundle_Then_MessageHubReceivesBundleReply()
            {
                // Arrange
                var meteringPointId = "571313180000000005";
                var request = CreateEvent(meteringPointId, out var correlationId, out var parentId);

                // Act
                await MockTelemetryClient.SendWrappedServiceBusMessageToQueueAsync(Fixture.CreateLinkRequestQueue, request, correlationId, parentId);

                // Assert
                await Fixture.MessageHubMock.AssertPeekReceivesReplyAsync(correlationId);
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

            private CreateDefaultChargeLinks CreateEvent(string meteringPointId, out string correlationId, out string parentId)
            {
                correlationId = CorrelationIdGenerator.Create();
                var message = new CreateDefaultChargeLinks { MeteringPointId = meteringPointId };
                parentId = $"00-{correlationId}-b7ad6b7169203331-01";
                return message;
            }
        }
    }
}
