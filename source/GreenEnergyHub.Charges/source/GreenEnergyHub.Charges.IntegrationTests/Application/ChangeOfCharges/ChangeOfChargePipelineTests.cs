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
using GreenEnergyHub.Charges.Application.ChangeOfCharges;
using GreenEnergyHub.Charges.Domain.ChangeOfCharges.Transaction;
using GreenEnergyHub.Charges.Infrastructure.Messaging;
using GreenEnergyHub.Charges.IntegrationTests.TestHelpers;
using GreenEnergyHub.Charges.MessageReceiver;
using GreenEnergyHub.Charges.TestCore;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using NodaTime;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.IntegrationTests.Application.ChangeOfCharges
{
    [IntegrationTest]
    public class ChangeOfChargePipelineTests : IClassFixture<DbFixture>
    {
        private readonly ChargeHttpTrigger _chargeHttpTrigger;
        private readonly string _commandAcceptedSubscriptionName;
        private readonly string _commandRejectedSubscriptionName;
        private readonly string _commandAcceptedTopicName;
        private readonly string _commandRejectedTopicName;
        private readonly string _commandAcceptedConnectionString;
        private readonly string _commandRejectedConnectionString;
        private readonly TestDbHelper _testDbHelper;

        public ChangeOfChargePipelineTests([NotNull] DbFixture dbFixture)
        {
            _testDbHelper = new TestDbHelper(dbFixture.ServiceProvider);

            var messageReceiverHost = TestConfigurationHelper.SetupHost(new MessageReceiver.Startup());

            _chargeHttpTrigger = new ChargeHttpTrigger(
                messageReceiverHost.Services.GetRequiredService<IChangeOfChargesMessageHandler>(),
                messageReceiverHost.Services.GetRequiredService<ICorrelationContext>(),
                messageReceiverHost.Services.GetRequiredService<MessageExtractor<ChargeCommand>>());

            _commandAcceptedSubscriptionName = Environment.GetEnvironmentVariable("COMMAND_ACCEPTED_INTEGRATION_TEST_SUBSCRIPTION_NAME") !;
            _commandRejectedSubscriptionName = Environment.GetEnvironmentVariable("COMMAND_REJECTED_INTEGRATION_TEST_SUBSCRIPTION_NAME") !;
            _commandAcceptedTopicName = Environment.GetEnvironmentVariable("COMMAND_ACCEPTED_TOPIC_NAME") !;
            _commandRejectedTopicName = Environment.GetEnvironmentVariable("COMMAND_REJECTED_TOPIC_NAME") !;
            _commandAcceptedConnectionString = Environment.GetEnvironmentVariable("COMMAND_ACCEPTED_LISTENER_CONNECTION_STRING") !;
            _commandRejectedConnectionString = Environment.GetEnvironmentVariable("COMMAND_REJECTED_LISTENER_CONNECTION_STRING") !;
        }

        [Theory]
        [InlineAutoMoqData("TestFiles\\ValidTariffAddition.json")]
        public async Task Test_ChargeCommandCompleteFlow_is_Accepted(
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

            var commandAcceptedMessage = serviceBusMessageTestHelper
                .GetMessageFromServiceBus(_commandAcceptedConnectionString, _commandAcceptedTopicName, _commandAcceptedSubscriptionName);

            var chargeExistsByCorrelationId = await _testDbHelper
                .ChargeExistsByCorrelationIdAsync(executionContext.InvocationId.ToString())
                .ConfigureAwait(false);

            // assert
            Assert.Equal(200, messageReceiverResult!.StatusCode!.Value);
            Assert.Equal(executionContext.InvocationId.ToString(), commandAcceptedMessage.CorrelationId);
            Assert.True(commandAcceptedMessage.Body.Length > 0);
            Assert.True(chargeExistsByCorrelationId);
        }

        [Theory]
        [InlineAutoMoqData("TestFiles\\InValidTariffAddition.json")]
        public async Task Test_ChargeCommandCompleteFlow_is_Rejected(
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

            var commandRejectedMessage = serviceBusMessageTestHelper
                .GetMessageFromServiceBus(_commandRejectedConnectionString, _commandRejectedTopicName, _commandRejectedSubscriptionName);

            var chargeExistsByCorrelationId = await _testDbHelper
                .ChargeExistsByCorrelationIdAsync(executionContext.InvocationId.ToString())
                .ConfigureAwait(false);

            // assert
            Assert.Equal(200, messageReceiverResult!.StatusCode!.Value);
            Assert.Equal(executionContext.InvocationId.ToString(), commandRejectedMessage.CorrelationId);
            Assert.True(commandRejectedMessage.Body.Length > 0);
            Assert.False(chargeExistsByCorrelationId);
        }

        private async Task<OkObjectResult> RunMessageReceiver(Mock<ILogger> logger, ExecutionContext executionContext, DefaultHttpRequest req)
        {
            return (OkObjectResult)await _chargeHttpTrigger
                .RunAsync(req, executionContext, logger.Object).ConfigureAwait(false);
        }
    }
}
