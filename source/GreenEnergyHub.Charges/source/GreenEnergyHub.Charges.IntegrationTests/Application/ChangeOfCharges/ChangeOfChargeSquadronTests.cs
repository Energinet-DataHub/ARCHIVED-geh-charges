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
using GreenEnergyHub.Charges.Application.Charges.Handlers;
using GreenEnergyHub.Charges.ChargeReceiver;
using GreenEnergyHub.Charges.Core.Json;
using GreenEnergyHub.Charges.Domain.ChargeCommandReceivedEvents;
using GreenEnergyHub.Charges.Domain.ChargeCommands;
using GreenEnergyHub.Charges.Infrastructure.Messaging;
using GreenEnergyHub.Charges.IntegrationTests.TestHelpers;
using GreenEnergyHub.Charges.TestCore.Attributes;
using Microsoft.AspNetCore.Http;
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
    public class ChangeOfChargeSquadronTests : IClassFixture<ChargesAzureCloudServiceBusResource>
    {
        private readonly ChargesAzureCloudServiceBusResource _serviceBusResource;

        public ChangeOfChargeSquadronTests(ChargesAzureCloudServiceBusResource serviceBusResource)
        {
            _serviceBusResource = serviceBusResource;
        }

        [PipelineIntegrationTestTheory(Timeout = 30000)]
        [Trait(HostingEnvironmentTraitConstants.HostingEnvironment, HostingEnvironmentTraitConstants.PullRequestGate)]
        [InlineAutoMoqData("TestFiles/ValidCreateTariffCommand.json")]
        [InlineAutoMoqData("TestFiles/InvalidCreateTariffCommand.json")]
        public async Task ChargeReceiver_receives_message(
            string testFilePath,
            [NotNull] [Frozen] Mock<ILogger> logger,
            [NotNull] ExecutionContext executionContext)
        {
            // arrange
            var subscriptionClient = _serviceBusResource.GetSubscriptionClient(
                ChargesAzureCloudServiceBusOptions.ReceivedTopicName,
                ChargesAzureCloudServiceBusOptions.SubscriptionName);

            var completion = new TaskCompletionSource<ChargeCommandReceivedEvent?>();

            ServiceBusTestHelper.RegisterSubscriptionClientMessageHandler(subscriptionClient, completion);

            IClock clock = SystemClock.Instance;
            var chargeJson = EmbeddedResourceHelper.GetInputJson(testFilePath, clock);
            var chargeCommand = new JsonSerializer().Deserialize<ChargeCommand>(chargeJson);
            var httpRequest = HttpRequestFactory.CreateHttpRequest(chargeJson);

            // act
            await CallChargeReceiver(logger, executionContext, httpRequest).ConfigureAwait(false);
            var receivedEvent = await completion.Task.ConfigureAwait(false);

            // assert
            Assert.NotNull(receivedEvent);
            Assert.NotNull(receivedEvent?.Command.Document);
            Assert.NotNull(receivedEvent?.Command.ChargeOperation);
            Assert.Equal(chargeCommand.ChargeOperation.ChargeId, receivedEvent?.Command.ChargeOperation.ChargeId);
        }

        private async Task CallChargeReceiver(
            IMock<ILogger> logger,
            ExecutionContext executionContext,
            HttpRequest req)
        {
            var topicClient = _serviceBusResource.GetTopicClient(ChargesAzureCloudServiceBusOptions.ReceivedTopicName);
            var chargeReceiverHost = FunctionHostConfigurationHelper.SetupHost(new ChargeReceiverConfiguration(topicClient));

            var chargeHttpTrigger = new ChargeHttpTrigger(
                chargeReceiverHost.Services.GetRequiredService<IChargesMessageHandler>(),
                chargeReceiverHost.Services.GetRequiredService<ICorrelationContext>(),
                chargeReceiverHost.Services.GetRequiredService<MessageExtractor<ChargeCommand>>());

            await chargeHttpTrigger
                .RunAsync(req, executionContext, logger.Object)
                .ConfigureAwait(false);
        }
    }
}
