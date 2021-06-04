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
using GreenEnergyHub.Charges.Application.ChangeOfCharges;
using GreenEnergyHub.Charges.Domain.ChangeOfCharges.Transaction;
using GreenEnergyHub.Charges.Infrastructure.Messaging;
using GreenEnergyHub.Charges.IntegrationTests.TestHelpers;
using GreenEnergyHub.Charges.MessageReceiver;
using GreenEnergyHub.Charges.TestCore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using NodaTime;
using Squadron;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.IntegrationTests.Application.ChangeOfCharges
{
    [IntegrationTest]
    public class ChangeOfChargeSquadronTests
        : IClassFixture<AzureCloudServiceBusResource<ChargesAzureCloudServiceBusOptions>>
    {
        private readonly AzureCloudServiceBusResource<ChargesAzureCloudServiceBusOptions> _serviceBusResource;

        public ChangeOfChargeSquadronTests(AzureCloudServiceBusResource<ChargesAzureCloudServiceBusOptions> serviceBusResource)
        {
            _serviceBusResource = serviceBusResource;
        }

        [Theory]
        [InlineAutoMoqData("TestFiles/ValidCreateTariffCommand.json")]
        [InlineAutoMoqData("TestFiles/InvalidCreateTariffCommand.json")]
        public async Task MessageReceiver_receives_message(
            string testFilePath,
            [NotNull] [Frozen] Mock<ILogger> logger,
            [NotNull] ExecutionContext executionContext)
        {
            // arrange
            var subscriptionClient = _serviceBusResource.GetSubscriptionClient(
                ChargesAzureCloudServiceBusOptions.ReceivedTopicName,
                ChargesAzureCloudServiceBusOptions.SubscriptionName);

            var completion = new TaskCompletionSource<ChargeCommand?>();
            RegisterSubscriptionClientMessageHandler(subscriptionClient, completion);

            IClock clock = SystemClock.Instance;
            var httpRequest = HttpRequestFactory.CreateHttpRequest(testFilePath, clock);

            // act
            await CallMessageReceiver(logger, executionContext, httpRequest).ConfigureAwait(false);
            var receivedEvent = await completion.Task.ConfigureAwait(false);

            // assert
            Assert.NotNull(receivedEvent);
        }

        private void RegisterSubscriptionClientMessageHandler(
            [NotNull] ISubscriptionClient subscriptionClient,
            [NotNull] TaskCompletionSource<ChargeCommand?> completion)
        {
            subscriptionClient.RegisterMessageHandler(
                (message, ct) =>
                {
                    try
                    {
                        var json = Encoding.UTF8.GetString(message.Body);
                        var ev = JsonConvert.DeserializeObject<ChargeCommand>(json);
                        completion.SetResult(ev);
                    }
#pragma warning disable CA1031 // allow catch of exception
                    catch (Exception exception)
                    {
                        completion.SetException(exception);
                    }
#pragma warning restore CA1031
                    return Task.CompletedTask;
                }, new MessageHandlerOptions(ExceptionReceivedHandler));
        }

        private Task ExceptionReceivedHandler(ExceptionReceivedEventArgs arg)
        {
            throw new NotImplementedException();
        }

        private async Task<OkObjectResult> CallMessageReceiver(IMock<ILogger> logger, ExecutionContext executionContext, HttpRequest req)
        {
            var topicClient = _serviceBusResource.GetTopicClient(ChargesAzureCloudServiceBusOptions.ReceivedTopicName);
            var messageReceiverHost = FunctionHostConfigurationHelper.SetupHost(new MessageReceiverConfiguration(topicClient));

            var chargeHttpTrigger = new ChargeHttpTrigger(
                messageReceiverHost.Services.GetRequiredService<IChangeOfChargesMessageHandler>(),
                messageReceiverHost.Services.GetRequiredService<ICorrelationContext>(),
                messageReceiverHost.Services.GetRequiredService<MessageExtractor<ChargeCommand>>());

            return (OkObjectResult)await chargeHttpTrigger
                .RunAsync(req, executionContext, logger.Object)
                .ConfigureAwait(false);
        }
    }
}
