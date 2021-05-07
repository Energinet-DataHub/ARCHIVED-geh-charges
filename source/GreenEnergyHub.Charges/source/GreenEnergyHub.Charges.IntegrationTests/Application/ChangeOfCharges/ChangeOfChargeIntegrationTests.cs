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
using System.Text;
using System.Threading.Tasks;
using AutoFixture.Xunit2;
using GreenEnergyHub.Charges.Application;
using GreenEnergyHub.Charges.Application.ChangeOfCharges;
using GreenEnergyHub.Charges.ChargeCommandReceiver;
using GreenEnergyHub.Charges.Domain.Events.Local;
using GreenEnergyHub.Charges.Infrastructure.Messaging;
using GreenEnergyHub.Charges.IntegrationTests.TestHelpers;
using GreenEnergyHub.Charges.MessageReceiver;
using GreenEnergyHub.Charges.TestCore;
using GreenEnergyHub.Json;
using GreenEnergyHub.TestHelpers.Traits;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using NodaTime;
using Xunit;
using Xunit.Abstractions;

namespace GreenEnergyHub.Charges.IntegrationTests.Application.ChangeOfCharges
{
    [Trait(TraitNames.Category, TraitValues.IntegrationTest)]
    public class ChangeOfChargesMessageHandlerTests
    {
        private readonly ITestOutputHelper _testOutputHelper;
        private readonly ChargeHttpTrigger _chargeHttpTrigger;
        private readonly ChargeCommandEndpoint _chargeCommandEndpoint;
        private readonly string _subscriptionName;
        private readonly string _commandReceivedTopicName;
        private readonly string _commandAcceptedTopicName;
        private readonly string _commandRejectedTopicName;
        private readonly string _commandReceivedConnectionString;
        private readonly string _commandAcceptedConnectionString;
        private readonly string _commandRejectedConnectionString;

        public ChangeOfChargesMessageHandlerTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
            TestConfigurationHelper.ConfigureEnvironmentVariablesFromLocalSettings();
            var messageReceiverHost = TestConfigurationHelper.SetupHost(new MessageReceiver.Startup());
            var chargeCommandReceiverHost = TestConfigurationHelper.SetupHost(new ChargeCommandReceiver.Startup());

            _chargeHttpTrigger = new ChargeHttpTrigger(
                messageReceiverHost.Services.GetRequiredService<IJsonSerializer>(),
                messageReceiverHost.Services.GetRequiredService<IChangeOfChargesMessageHandler>(),
                messageReceiverHost.Services.GetRequiredService<ICorrelationContext>());

            _chargeCommandEndpoint = new ChargeCommandEndpoint(
                chargeCommandReceiverHost.Services.GetRequiredService<IJsonSerializer>(),
                chargeCommandReceiverHost.Services.GetRequiredService<IChargeCommandHandler>(),
                chargeCommandReceiverHost.Services.GetRequiredService<ICorrelationContext>());

            _subscriptionName = Environment.GetEnvironmentVariable("COMMAND_INTEGRATIONTEST_SUBSCRIPTION_NAME") !;
            _commandReceivedTopicName = Environment.GetEnvironmentVariable("COMMAND_RECEIVED_TOPIC_NAME") !;
            _commandAcceptedTopicName = Environment.GetEnvironmentVariable("COMMAND_ACCEPTED_TOPIC_NAME") !;
            _commandRejectedTopicName = Environment.GetEnvironmentVariable("COMMAND_REJECTED_TOPIC_NAME") !;
            _commandReceivedConnectionString = Environment.GetEnvironmentVariable("COMMAND_RECEIVED_LISTENER_CONNECTION_STRING") !;
            _commandAcceptedConnectionString = Environment.GetEnvironmentVariable("COMMAND_ACCEPTED_LISTENER_CONNECTION_STRING") !;
            _commandRejectedConnectionString = Environment.GetEnvironmentVariable("COMMAND_REJECTED_LISTENER_CONNECTION_STRING") !;
        }

        [Theory]
        [InlineAutoMoqData("TestFiles\\ValidChargeAddition.json")]
        [InlineAutoMoqData("TestFiles\\ValidChargeUpdate.json")]
        public async Task Test_ChargeCommand_is_Accepted(
            string testFilePath,
            [NotNull] [Frozen] Mock<ILogger> logger,
            [NotNull] ExecutionContext executionContext,
            [NotNull] ServiceBusMessageTestHelper serviceBusMessageTestHelper)
        {
            // arrange
            IClock clock = SystemClock.Instance;
            var req = HttpRequestHelper.CreateHttpRequest(testFilePath, clock);

            // act
            var messageReceiverResult = await RunMessageReceiver(logger, executionContext, req).ConfigureAwait(false);
            var commandReceivedMessage = serviceBusMessageTestHelper
                .GetMessageFromServiceBus(_commandReceivedConnectionString, _commandReceivedTopicName, _subscriptionName);
            _testOutputHelper.WriteLine($"Message to be handled by ChargeCommandEndpoint: {commandReceivedMessage.Body.Length}");

            await _chargeCommandEndpoint.RunAsync(commandReceivedMessage.Body, logger.Object).ConfigureAwait(false);

            var commandAcceptedMessage = serviceBusMessageTestHelper
                .GetMessageFromServiceBus(_commandAcceptedConnectionString, _commandAcceptedTopicName, _subscriptionName);
            _testOutputHelper.WriteLine($"Message accepted by ChargeCommandEndpoint: {commandAcceptedMessage.Body.Length}");

            // assert
            Assert.Equal(200, messageReceiverResult!.StatusCode!.Value);
            Assert.Equal(nameof(ChargeCommandReceivedEvent), commandReceivedMessage.Label);
            Assert.Equal(nameof(ChargeCommandAcceptedEvent), commandAcceptedMessage.Label);
            Assert.True(commandAcceptedMessage.Body.Length > 0);
        }

        [Theory]
        [InlineAutoMoqData("TestFiles\\InvalidChargeAddition.json")]
        [InlineAutoMoqData("TestFiles\\InvalidChargeUpdate.json")]
        public async Task Test_ChargeCommand_is_Rejected(
            string testFilePath,
            [NotNull] [Frozen] Mock<ILogger> logger,
            [NotNull] ExecutionContext executionContext,
            [NotNull] ServiceBusMessageTestHelper serviceBusMessageTestHelper)
        {
            // arrange
            IClock clock = SystemClock.Instance;
            var req = HttpRequestHelper.CreateHttpRequest(testFilePath, clock);

            // act
            var messageReceiverResult = await RunMessageReceiver(logger, executionContext, req).ConfigureAwait(false);
            var commandReceivedMessage = serviceBusMessageTestHelper
                .GetMessageFromServiceBus(_commandReceivedConnectionString, _commandReceivedTopicName, _subscriptionName);
            _testOutputHelper.WriteLine($"Message to be handled by ChargeCommandEndpoint: {commandReceivedMessage.Body.Length}");

            await _chargeCommandEndpoint.RunAsync(commandReceivedMessage.Body, logger.Object).ConfigureAwait(false);

            var commandRejectedMessage = serviceBusMessageTestHelper
                .GetMessageFromServiceBus(_commandRejectedConnectionString, _commandRejectedTopicName, _subscriptionName);
            _testOutputHelper.WriteLine($"Message accepted by ChargeCommandEndpoint: {commandRejectedMessage.Body.Length}");

            // assert
            Assert.Equal(200, messageReceiverResult!.StatusCode!.Value);
            Assert.Equal(nameof(ChargeCommandReceivedEvent), commandReceivedMessage.Label);
            Assert.Equal(nameof(ChargeCommandRejectedEvent), commandRejectedMessage.Label);
            Assert.True(commandRejectedMessage.Body.Length > 0);
        }

        private async Task<OkObjectResult> RunMessageReceiver(Mock<ILogger> logger, ExecutionContext executionContext, DefaultHttpRequest req)
        {
            return (OkObjectResult)await _chargeHttpTrigger.RunAsync(req, executionContext, logger.Object).ConfigureAwait(false);
        }
    }
}
