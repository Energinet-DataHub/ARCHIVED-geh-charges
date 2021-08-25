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
using AutoFixture.Xunit2;
using GreenEnergyHub.Charges.Application;
using GreenEnergyHub.Charges.Application.ChangeOfCharges;
using GreenEnergyHub.Charges.ChargeCommandReceiver;
using GreenEnergyHub.Charges.Core.Json;
using GreenEnergyHub.Charges.Domain.ChangeOfCharges.Transaction;
using GreenEnergyHub.Charges.Domain.Events.Local;
using GreenEnergyHub.Charges.Infrastructure.Messaging;
using GreenEnergyHub.Charges.IntegrationTests.TestHelpers;
using GreenEnergyHub.Charges.MessageReceiver;
using GreenEnergyHub.Charges.TestCore;
using GreenEnergyHub.Charges.TestCore.Attributes;
using GreenEnergyHub.Messaging.Transport;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using NodaTime;
using Xunit;
using Xunit.Abstractions;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.IntegrationTests.Application.ChangeOfCharges
{
    /// <summary>
    /// Integration tests in ChangeOfChargeLocalHostTests can executed at localhost
    /// using Service Bus topics in a sandbox environment. Prerequisites for executing
    /// the integration tests as localhost:
    ///  - all required Service Bus topics and subscriptions created in sandbox
    ///  - all topic and subscription-names must be set in local.settings.json
    ///  - all topic sender and listener connectionstrings must be set in local.settings.json
    /// See local.settings.json.sample for an example.
    /// </summary>
    [IntegrationTest]
    public class ChangeOfChargeLocalHostTests : IClassFixture<DbContextRegistrator>
    {
        private readonly bool _runLocalhostTests;
        private readonly ITestOutputHelper _testOutputHelper;
        private readonly ChargeDbQueries _chargeDbQueries;
        private readonly ChargeHttpTrigger? _chargeHttpTrigger;
        private readonly ChargeCommandEndpoint? _chargeCommandEndpoint;
        private readonly string? _commandReceivedTopicName;
        private readonly string? _commandAcceptedTopicName;
        private readonly string? _commandRejectedTopicName;
        private readonly string? _commandReceivedConnectionString;
        private readonly string? _commandAcceptedConnectionString;
        private readonly string? _commandRejectedConnectionString;
        private readonly string? _commandReceivedSubscriptionName;
        private readonly string? _commandAcceptedSubscriptionName;
        private readonly string? _commandRejectedSubscriptionName;

        public ChangeOfChargeLocalHostTests(ITestOutputHelper testOutputHelper, [NotNull] DbContextRegistrator dbContextRegistrator)
        {
            _testOutputHelper = testOutputHelper;
            _chargeDbQueries = new ChargeDbQueries(dbContextRegistrator.ServiceProvider);

            _runLocalhostTests = Environment.GetEnvironmentVariable("RUN_LOCALHOST_TESTS")?.ToUpperInvariant() == "TRUE";

            if (!_runLocalhostTests) return;

            var messageReceiverHost = FunctionHostConfigurationHelper.SetupHost(new MessageReceiver.Startup());
            var chargeCommandReceiverHost = FunctionHostConfigurationHelper.SetupHost(new ChargeCommandReceiver.Startup());

            _chargeHttpTrigger = new ChargeHttpTrigger(
                messageReceiverHost.Services.GetRequiredService<IChangeOfChargesMessageHandler>(),
                messageReceiverHost.Services.GetRequiredService<ICorrelationContext>(),
                messageReceiverHost.Services.GetRequiredService<MessageExtractor<ChargeCommand>>());

            _chargeCommandEndpoint = new ChargeCommandEndpoint(
                chargeCommandReceiverHost.Services.GetRequiredService<IChargeCommandHandler>(),
                chargeCommandReceiverHost.Services.GetRequiredService<ICorrelationContext>(),
                chargeCommandReceiverHost.Services.GetRequiredService<MessageExtractor>());

            _commandReceivedSubscriptionName = Environment.GetEnvironmentVariable("COMMAND_RECEIVED_SUBSCRIPTION_NAME") ?? string.Empty;
            _commandAcceptedSubscriptionName = Environment.GetEnvironmentVariable("COMMAND_ACCEPTED_SUBSCRIPTION_NAME") ?? string.Empty;
            _commandRejectedSubscriptionName = Environment.GetEnvironmentVariable("COMMAND_REJECTED_SUBSCRIPTION_NAME") ?? string.Empty;
            _commandReceivedTopicName = Environment.GetEnvironmentVariable("COMMAND_RECEIVED_TOPIC_NAME") ?? string.Empty;
            _commandAcceptedTopicName = Environment.GetEnvironmentVariable("COMMAND_ACCEPTED_TOPIC_NAME") ?? string.Empty;
            _commandRejectedTopicName = Environment.GetEnvironmentVariable("COMMAND_REJECTED_TOPIC_NAME") ?? string.Empty;
            _commandReceivedConnectionString = Environment.GetEnvironmentVariable("COMMAND_RECEIVED_LISTENER_CONNECTION_STRING") ?? string.Empty;
            _commandAcceptedConnectionString = Environment.GetEnvironmentVariable("COMMAND_ACCEPTED_LISTENER_CONNECTION_STRING") ?? string.Empty;
            _commandRejectedConnectionString = Environment.GetEnvironmentVariable("COMMAND_REJECTED_LISTENER_CONNECTION_STRING") ?? string.Empty;
        }

        [Theory(Timeout = 60000)]
        [Trait(HostingEnvironmentTraitConstants.HostingEnvironment, HostingEnvironmentTraitConstants.LocalHost)]
        [InlineAutoMoqData("TestFiles/ValidCreateTariffCommand.json")]
        public async Task Test_ChargeCommand_is_Accepted(
            string testFilePath,
            [NotNull] [Frozen] Mock<ILogger> logger,
            [NotNull] ExecutionContext executionContext,
            [NotNull] ServiceBusTestHelper serviceBusTestHelper)
        {
            if (!_runLocalhostTests) return;

            // arrange
            IClock clock = SystemClock.Instance;
            var chargeJson = EmbeddedResourceHelper.GetInputJson(testFilePath, clock);
            var req = HttpRequestFactory.CreateHttpRequest(chargeJson);
            var chargeCommand = new JsonSerializer().Deserialize<ChargeCommand>(chargeJson);

            // act
            var messageReceiverResult = await RunMessageReceiver(logger, executionContext, req).ConfigureAwait(false);
            var commandReceivedResult = await serviceBusTestHelper
                .GetMessageFromServiceBusAsync<ChargeCommandAcceptedEvent>(
                    _commandReceivedConnectionString ?? string.Empty,
                    _commandReceivedTopicName ?? string.Empty,
                    _commandReceivedSubscriptionName ?? string.Empty,
                    executionContext.InvocationId.ToString())
                .ConfigureAwait(false);
            _testOutputHelper.WriteLine($"Message to be handled by ChargeCommandEndpoint: {commandReceivedResult.receivedMessage.Body.Length}");

            await _chargeCommandEndpoint!.RunAsync(commandReceivedResult.receivedMessage.Body, logger.Object).ConfigureAwait(false);

            var commandAcceptedResult = await serviceBusTestHelper
                .GetMessageFromServiceBusAsync<ChargeCommandAcceptedEvent>(
                    _commandAcceptedConnectionString ?? string.Empty,
                    _commandAcceptedTopicName ?? string.Empty,
                    _commandAcceptedSubscriptionName ?? string.Empty,
                    commandReceivedResult.receivedEvent.CorrelationId)
                .ConfigureAwait(false);
            _testOutputHelper.WriteLine($"Message accepted by ChargeCommandEndpoint: {commandAcceptedResult.receivedMessage.CorrelationId}");

            var chargeExists = await _chargeDbQueries
                .ChargeExistsAsync(
                    chargeCommand.ChargeOperation.ChargeId,
                    chargeCommand.ChargeOperation.ChargeOwner,
                    chargeCommand.ChargeOperation.Type)
                .ConfigureAwait(false);

            // assert
            Assert.Equal(200, messageReceiverResult!.StatusCode!.Value);
            Assert.Equal(executionContext.InvocationId.ToString(), commandReceivedResult.receivedMessage.CorrelationId);
            Assert.Equal(executionContext.InvocationId.ToString(), commandAcceptedResult.receivedMessage.CorrelationId);
            Assert.True(commandReceivedResult.receivedMessage.Body.Length > 0);
            Assert.True(commandAcceptedResult.receivedMessage.Body.Length > 0);
            Assert.True(chargeExists);
        }

        [Theory(Timeout = 60000)]
        [Trait(HostingEnvironmentTraitConstants.HostingEnvironment, HostingEnvironmentTraitConstants.LocalHost)]
        [InlineAutoMoqData("TestFiles/InvalidCreateTariffCommand.json")]
        public async Task Test_ChargeCommand_is_Rejected(
            string testFilePath,
            [NotNull] [Frozen] Mock<ILogger> logger,
            [NotNull] ExecutionContext executionContext,
            [NotNull] ServiceBusTestHelper serviceBusTestHelper)
        {
            if (!_runLocalhostTests) return;

            // arrange
            IClock clock = SystemClock.Instance;
            var chargeJson = EmbeddedResourceHelper.GetInputJson(testFilePath, clock);
            var req = HttpRequestFactory.CreateHttpRequest(chargeJson);
            var chargeCommand = new JsonSerializer().Deserialize<ChargeCommand>(chargeJson);

            // act
            var messageReceiverResult = await RunMessageReceiver(logger, executionContext, req).ConfigureAwait(false);
            var commandReceivedResult = await serviceBusTestHelper.GetMessageFromServiceBusAsync<ChargeCommand>(
                    _commandReceivedConnectionString ?? string.Empty,
                    _commandReceivedTopicName ?? string.Empty,
                    _commandReceivedSubscriptionName ?? string.Empty,
                    executionContext.InvocationId.ToString())
                .ConfigureAwait(false);
            _testOutputHelper.WriteLine($"Message to be handled by ChargeCommandEndpoint: {commandReceivedResult.receivedMessage.Body.Length}");

            await _chargeCommandEndpoint!.RunAsync(commandReceivedResult.receivedMessage.Body, logger.Object).ConfigureAwait(false);

            var commandRejectedResult = await serviceBusTestHelper.GetMessageFromServiceBusAsync<ChargeCommandRejectedEvent>(
                    _commandRejectedConnectionString ?? string.Empty,
                    _commandRejectedTopicName ?? string.Empty,
                    _commandRejectedSubscriptionName ?? string.Empty,
                    executionContext.InvocationId.ToString())
                .ConfigureAwait(false);
            _testOutputHelper.WriteLine($"Message accepted by ChargeCommandEndpoint: {commandRejectedResult.receivedMessage.Body.Length}");

            var chargeExists = await _chargeDbQueries
                .ChargeExistsAsync(
                    chargeCommand.ChargeOperation.ChargeId,
                    chargeCommand.ChargeOperation.ChargeOwner,
                    chargeCommand.ChargeOperation.Type)
                .ConfigureAwait(false);

            // assert
            Assert.Equal(200, messageReceiverResult!.StatusCode!.Value);
            Assert.Equal(executionContext.InvocationId.ToString(), commandReceivedResult.receivedEvent.CorrelationId);
            Assert.Equal(executionContext.InvocationId.ToString(), commandRejectedResult.receivedEvent.CorrelationId);
            Assert.True(commandReceivedResult.receivedMessage.Body.Length > 0);
            Assert.True(commandRejectedResult.receivedMessage.Body.Length > 0);
            Assert.False(chargeExists);
        }

        private async Task<OkObjectResult> RunMessageReceiver(Mock<ILogger> logger, ExecutionContext executionContext, DefaultHttpRequest req)
        {
            return (OkObjectResult)await _chargeHttpTrigger!
                .RunAsync(req, executionContext, logger.Object).ConfigureAwait(false);
        }
    }
}
