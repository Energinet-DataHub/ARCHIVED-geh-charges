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
using System.Text;
using System.Threading.Tasks;
using AutoFixture.Xunit2;
using GreenEnergyHub.Charges.Application.ChangeOfCharges;
using GreenEnergyHub.Charges.Domain.ChangeOfCharges.Transaction;
using GreenEnergyHub.Charges.Infrastructure.Messaging;
using GreenEnergyHub.Charges.IntegrationTests.TestHelpers;
using GreenEnergyHub.Charges.MessageReceiver;
using GreenEnergyHub.Charges.TestCore;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http.Internal;
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
        : IClassFixture<AzureCloudServiceBusResource<ChargeAzureCloudServiceBusOptions>>
    {
        private readonly AzureCloudServiceBusResource<ChargeAzureCloudServiceBusOptions> _serviceBusResource;

        public ChangeOfChargeSquadronTests(AzureCloudServiceBusResource<ChargeAzureCloudServiceBusOptions> serviceBusResource)
        {
            _serviceBusResource = serviceBusResource;
        }

        [Theory]
        [InlineAutoMoqData("TestFiles/ValidCreateTariffCommand.json")]
        public async Task M1(
            string testFilePath,
            [NotNull] [Frozen] Mock<ILogger> logger,
            [NotNull] ExecutionContext executionContext)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));
            if (executionContext == null) throw new ArgumentNullException(nameof(executionContext));

            // Arrange
            var topicClient = _serviceBusResource.GetTopicClient(ChargeAzureCloudServiceBusOptions.ReceivedTopicName);
            var messageReceiverHost = FunctionHostConfigurationHelper.SetupHost(new MessageReceiverConfiguration(topicClient));

            var chargeHttpTrigger = new ChargeHttpTrigger(
                messageReceiverHost.Services.GetRequiredService<IChangeOfChargesMessageHandler>(),
                messageReceiverHost.Services.GetRequiredService<ICorrelationContext>(),
                messageReceiverHost.Services.GetRequiredService<MessageExtractor<ChargeCommand>>());

            IClock clock = SystemClock.Instance;
            var req = HttpRequestFactory.CreateHttpRequest(testFilePath, clock);

            await RunMessageReceiver(logger, executionContext, req, chargeHttpTrigger).ConfigureAwait(false);

            var subscriptionClient = _serviceBusResource.GetSubscriptionClient(
                ChargeAzureCloudServiceBusOptions.ReceivedTopicName,
                ChargeAzureCloudServiceBusOptions.SubscriptionName001);

            var completion = new TaskCompletionSource<ChargeCommand?>();

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

            var receivedEvent = await completion.Task.ConfigureAwait(false);

            Assert.NotNull(receivedEvent);
        }

        private Task ExceptionReceivedHandler(ExceptionReceivedEventArgs arg)
        {
            throw new NotImplementedException();
        }

        private static async Task<OkObjectResult> RunMessageReceiver(Mock<ILogger> logger, ExecutionContext executionContext, DefaultHttpRequest req, ChargeHttpTrigger chargeHttpTrigger)
        {
            return (OkObjectResult)await chargeHttpTrigger
                .RunAsync(req, executionContext, logger.Object)
                .ConfigureAwait(false);
        }
    }
}
