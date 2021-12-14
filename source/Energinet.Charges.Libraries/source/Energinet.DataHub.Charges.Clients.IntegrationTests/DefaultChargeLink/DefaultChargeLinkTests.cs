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
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Energinet.DataHub.Charges.Clients.DefaultChargeLink;
using Energinet.DataHub.Charges.Clients.DefaultChargeLink.Models;
using Energinet.DataHub.Charges.Clients.IntegrationTests.Fixtures;
using Energinet.DataHub.Charges.Clients.ServiceBus.Providers;
using FluentAssertions;
using GreenEnergyHub.TestHelpers;
using Xunit;
using Xunit.Abstractions;

namespace Energinet.DataHub.Charges.Clients.IntegrationTests.DefaultChargeLink
{
    public static class DefaultChargeLinkTests
    {
        [Collection(nameof(ChargesClientsCollectionFixture))]
        [SuppressMessage("ReSharper", "CA1034", Justification = "Integration test")]
        public class CreateDefaultChargeLinksRequestAsync : LibraryTestBase<ChargesClientsFixture>, IAsyncLifetime
        {
            private readonly ServiceBusClient _serviceBusClient;
            private readonly ServiceBusTestListener _serviceBusTestListener;
            private readonly IServiceBusRequestSenderProvider _serviceBusRequestSenderProvider;

            public CreateDefaultChargeLinksRequestAsync(ChargesClientsFixture fixture, ITestOutputHelper testOutputHelper)
                : base(fixture, testOutputHelper)
            {
                string replyToQueueName = EnvironmentVariableReader.GetEnvironmentVariable(
                    EnvironmentSettingNames.CreateLinkReplyQueueName, string.Empty);
                var requestQueueName = EnvironmentVariableReader.GetEnvironmentVariable(
                    EnvironmentSettingNames.CreateLinkRequestQueueName, string.Empty);

                string serviceBusConnectionString = EnvironmentVariableReader.GetEnvironmentVariable(
                    EnvironmentSettingNames.IntegrationEventSenderConnectionString, string.Empty);
                _serviceBusClient = new ServiceBusClient(serviceBusConnectionString);

                _serviceBusTestListener = new ServiceBusTestListener(Fixture);
                _serviceBusRequestSenderProvider =
                    new ServiceBusRequestSenderProvider(
                        _serviceBusClient,
                        new ServiceBusRequestSenderTestConfiguration(replyToQueueName, requestQueueName));
            }

            public Task InitializeAsync()
            {
                return Task.CompletedTask;
            }

            public async Task DisposeAsync()
            {
                Fixture.ServiceBusListenerMock.ResetMessageHandlersAndReceivedMessages();
                await _serviceBusClient.DisposeAsync().ConfigureAwait(false);
            }

            [Theory]
            [AutoDomainData]
            public async Task When_CreateDefaultChargeLinksRequestAsync_Then_RequestIsSendToCharges(
                RequestDefaultChargeLinksForMeteringPointDto requestDefaultChargeLinksForMeteringPointDto, string correlationId)
            {
                // Arrange
                using var result = await _serviceBusTestListener.ListenForMessageAsync().ConfigureAwait(false);
                var sut = new DefaultChargeLinkClient(_serviceBusRequestSenderProvider);

                // Act
                await sut.CreateDefaultChargeLinksRequestAsync(requestDefaultChargeLinksForMeteringPointDto, correlationId).ConfigureAwait(false);

                // Assert
                // => Service Bus (timeout should not be more than 5 secs).
                var isMessageReceived = result.IsMessageReceivedEvent!.Wait(TimeSpan.FromSeconds(5));

                isMessageReceived.Should().BeTrue();
                result.CorrelationId.Should().Be(correlationId);
                result.Body.Should().NotBeNull();
            }
        }
    }
}
