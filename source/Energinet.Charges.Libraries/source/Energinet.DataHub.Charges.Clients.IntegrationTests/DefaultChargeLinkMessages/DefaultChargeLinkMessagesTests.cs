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
using Energinet.DataHub.Charges.Clients.IntegrationTests.Fixtures;
using Energinet.DataHub.Charges.Libraries.Common;
using Energinet.DataHub.Charges.Libraries.DefaultChargeLinkMessages;
using Energinet.DataHub.Charges.Libraries.Factories;
using Energinet.DataHub.Charges.Libraries.Models;
using Energinet.DataHub.Core.FunctionApp.TestCommon.ServiceBus.ListenerMock;
using FluentAssertions;
using GreenEnergyHub.TestHelpers;
using Xunit;
using Xunit.Abstractions;

namespace Energinet.DataHub.Charges.Clients.IntegrationTests.DefaultChargeLinkMessages
{
    public static class DefaultChargeLinkMessagesTests
    {
        [Collection(nameof(ChargesClientsCollectionFixture))]
        [SuppressMessage("ReSharper", "CA1034", Justification = "done")] // TODO: Why is this necessary here, but not in Charges.IntegrationTests.Health.HealthStatusTests?
        public class CreateDefaultChargeLinkMessagesAsync : LibraryTestBase<ChargesClientsFixture>, IAsyncLifetime
        {
            private readonly string _serviceBusConnectionString;
            private readonly string _replyToQueueName;
            private readonly string _requestQueueName;

            public CreateDefaultChargeLinkMessagesAsync(ChargesClientsFixture fixture, ITestOutputHelper testOutputHelper)
                : base(fixture, testOutputHelper)
            {
                _serviceBusConnectionString = EnvironmentVariableReader.GetEnvironmentVariable(
                    EnvironmentSettingNames.IntegrationEventSenderConnectionString, string.Empty);
                _replyToQueueName = EnvironmentVariableReader.GetEnvironmentVariable(
                    EnvironmentSettingNames.CreateLinkReplyQueueName, string.Empty);
                _requestQueueName = EnvironmentVariableReader.GetEnvironmentVariable(
                    EnvironmentSettingNames.CreateLinkRequestQueueName, string.Empty);
            }

            public Task InitializeAsync()
            {
                return Task.CompletedTask;
            }

            public Task DisposeAsync()
            {
                Fixture.ServiceBusListenerMock.ResetMessageHandlersAndReceivedMessages();
                return Task.CompletedTask;
            }

            [Theory]
            [AutoDomainData]
            public async Task When_CreateDefaultChargeLinkMessagesAsync_Then_RequestIsSendToCharges(
                CreateDefaultChargeLinkMessagesDto createDefaultChargeLinkMessagesDto, string correlationId)
            {
                // Arrange
                var body = new BinaryData(Array.Empty<byte>());
                string actualCorrelationId = string.Empty;
                using var isMessageReceivedEvent = await Fixture.ServiceBusListenerMock
                    .WhenAny()
                    .VerifyOnceAsync(receivedMessage =>
                    {
                        body = receivedMessage.Body;
                        actualCorrelationId = receivedMessage.CorrelationId;
                        return Task.CompletedTask;
                    }).ConfigureAwait(false);

                var serviceBusRequestSenderFactory = new ServiceBusRequestSenderFactory();
                await using var serviceBusClient = new ServiceBusClient(_serviceBusConnectionString);
                await using var sut = new DefaultChargeLinkMessagesRequestClient(
                    serviceBusClient, serviceBusRequestSenderFactory, _replyToQueueName, _requestQueueName);

                // Act
                await sut.CreateDefaultChargeLinkMessagesRequestAsync(createDefaultChargeLinkMessagesDto, correlationId).ConfigureAwait(false);

                // Assert
                // => Service Bus (timeout should not be more than 5 secs).
                var isMessageReceived = isMessageReceivedEvent.Wait(TimeSpan.FromSeconds(5));
                isMessageReceived.Should().BeTrue();
                actualCorrelationId.Should().Be(correlationId);

                body.Should().NotBeNull();
            }
        }
    }
}
