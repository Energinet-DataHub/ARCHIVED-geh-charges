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
using System.Threading.Tasks;
using Energinet.DataHub.Core.FunctionApp.TestCommon;
using FluentAssertions;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeLinksAcceptedEvents;
using GreenEnergyHub.Charges.FunctionHost.ChargeLinks;
using GreenEnergyHub.Charges.FunctionHost.ChargeLinks.MessageHub;
using GreenEnergyHub.Charges.IntegrationTests.Fixtures.FunctionApp;
using GreenEnergyHub.Charges.IntegrationTests.TestCommon;
using GreenEnergyHub.Charges.IntegrationTests.TestHelpers;
using GreenEnergyHub.Charges.Tests.Builders;
using NodaTime;
using Xunit;
using Xunit.Abstractions;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.IntegrationTests.IntegrationTests
{
    [IntegrationTest]
    public class CreateDefaultChargeLinksReplierEndpointTests
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
            public async Task When_ChargeLinksAcceptedEvent_Then_CreateLinkReply()
            {
                // Arrange
                var command = new ChargeLinksCommandBuilder().Build("571313180000000005");
                var chargeLinksAcceptedEvent = new ChargeLinksAcceptedEvent(command, Instant.FromDateTimeUtc(DateTime.UtcNow));

                var jsonSerializer = new Json.JsonSerializer();
                var body = jsonSerializer.Serialize(chargeLinksAcceptedEvent);

                var applicationProperties = new Dictionary<string, string>();
                applicationProperties.Add("ReplyTo", Fixture.CreateLinkReplyQueue.Name);

                var correlationId = CorrelationIdGenerator.Create();
                var parentId = $"00-{correlationId}-b7ad6b7169203331-02";

                var message = ServiceBusMessageGenerator.CreateWithJsonContent(body, applicationProperties, correlationId);

                using var isMessageReceived = await Fixture.CreateLinkReplyQueueListener
                    .ListenForMessageAsync(correlationId)
                    .ConfigureAwait(false);

                // Act
                await MockTelemetryClient.WrappedOperationWithTelemetryDependencyInformationAsync(
                    () => Fixture.ChargeLinksAcceptedTopic.SenderClient.SendMessageAsync(message), correlationId, parentId);

                // Assert
                await FunctionAsserts.AssertHasExecutedAsync(Fixture.HostManager, nameof(ChargeLinkDataAvailableNotifierEndpoint)).ConfigureAwait(false);
                await FunctionAsserts.AssertHasExecutedAsync(Fixture.HostManager, nameof(CreateDefaultChargeLinksReplierEndpoint)).ConfigureAwait(false);

                var isMessageReceivedByQueue = isMessageReceived.MessageAwaiter!.Wait(TimeSpan.FromSeconds(10));
                isMessageReceivedByQueue.Should().BeTrue();

                // We need to clear host log after each test is done to ensure that we can assert on function executed on each test run because we only check on function name.
                Fixture.HostManager.ClearHostLog();
            }
        }
    }
}
