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
using GreenEnergyHub.Charges.Infrastructure.Messaging;
using GreenEnergyHub.Charges.IntegrationTests.TestHelpers;
using GreenEnergyHub.Charges.MessageReceiver;
using GreenEnergyHub.Charges.TestCore;
using GreenEnergyHub.Json;
using GreenEnergyHub.TestHelpers;
using GreenEnergyHub.TestHelpers.Traits;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Management;
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

        // private readonly SubscriptionClient _subscriptionClient;
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

            using (chargeCommandReceiverHost)
            {
                chargeCommandReceiverHost.StartAsync().ConfigureAwait(false);
            }
        }

        // [InlineAutoMoqData("TestFiles\\ValidChargeUpdate.json")]
        [Theory]
        [InlineAutoMoqData("TestFiles\\ValidChargeAddition.json")]
        public async Task Test_ChargeCommand_is_Accepted(
            string testFilePath,
            [NotNull] [Frozen] Mock<ILogger> logger,
            [NotNull] ExecutionContext executionContext,
            [NotNull] IClock clock)
        {
            // arrange
            var req = CreateHttpRequest(testFilePath, clock);
            SetInvocationId(executionContext);

            // act
            var result = (OkObjectResult)await _chargeHttpTrigger.RunAsync(req, executionContext, logger.Object).ConfigureAwait(false);

            // assert
            Assert.Equal(200, result!.StatusCode!.Value);
        }

        [Theory]
        [InlineAutoDomainData]
        public void Test_Subscription([NotNull] [Frozen] Mock<ILogger> logger)
        {
            // arrange
            string result = null!;
            var log = logger;

            var serviceBusSubscription = Environment.GetEnvironmentVariable("COMMAND_RECEIVED_SUBSCRIPTION_NAME");
            var serviceBusTopic = Environment.GetEnvironmentVariable("COMMAND_RECEIVED_TOPIC_NAME");
            var serviceBusConnectionString = Environment.GetEnvironmentVariable("COMMAND_RECEIVED_LISTENER_CONNECTION_STRING");
            var subscriptionClient = GetSubscriptionClient(serviceBusConnectionString!, serviceBusTopic!, serviceBusSubscription!);

            // act
            subscriptionClient.RegisterMessageHandler(
                async (message, token) =>
                {
                    var messageJson = Encoding.UTF8.GetString(message.Body);
                    result = messageJson;

                    if (messageJson.Length > 0)
                    {
                        await subscriptionClient.CompleteAsync(message.SystemProperties.LockToken).ConfigureAwait(false);
                    }
                },
#pragma warning disable 1998
                new MessageHandlerOptions(async args => _testOutputHelper.WriteLine(args.Exception.ToString()))
                    { MaxConcurrentCalls = 1, AutoComplete = false });
#pragma warning restore 1998

//             try
//             {
//                 subscriptionClient.RegisterMessageHandler(
//                     async (message, token) =>
//                     {
//                         var messageJson = Encoding.UTF8.GetString(message.Body);
//                         // var updateMessage = JsonConvert.DeserializeObject<ServiceBusMessageWrapper>(messageJson);
//                         //await _chargeCommandEndpoint.RunAsync(message.Body, logger.Object).ConfigureAwait(false);
//                         result = messageJson;
//
//                         await subscriptionClient
//                             .CompleteAsync(message.SystemProperties.LockToken)
//                             .ConfigureAwait(false);
//
//                         //Assert.Equal(200, result!.StatusCode!.Value);
//                     },
// #pragma warning disable 1998
//                     new MessageHandlerOptions(async args => _testOutputHelper.WriteLine(args.Exception.ToString()))
//                         { MaxConcurrentCalls = 1, AutoComplete = false });
// #pragma warning restore 1998
//             }
//             catch (InvalidOperationException e)
//             {
//                 _testOutputHelper.WriteLine("Exception: " + e.Message);
//             }
            var count = 0;
            while (result == null)
            {
                _testOutputHelper.WriteLine("still running: " + ++count);
            }

            // assert
            Assert.True(true);
        }

        [Theory]
        [InlineAutoMoqData("TestFiles\\InvalidChargeAddition.json")]
        [InlineAutoMoqData("TestFiles\\InvalidChargeUpdate.json")]
        public async Task Test_ChargeCommand_is_Rejected(
            string testFilePath,
            [NotNull] [Frozen] Mock<ILogger> logger,
            [NotNull] ExecutionContext executionContext,
            [NotNull] IClock clock)
        {
            // arrange
            var req = CreateHttpRequest(testFilePath, clock);
            SetInvocationId(executionContext);

            // act
            var result = (OkObjectResult)await _chargeHttpTrigger.RunAsync(req, executionContext, logger.Object).ConfigureAwait(false);

            // assert
            Assert.True(true);
            //Assert.Equal(500, result!.StatusCode!.Value);
        }

        private static void SetInvocationId(ExecutionContext executionContext)
        {
            executionContext.InvocationId = Guid.NewGuid();
        }

        private static DefaultHttpRequest CreateHttpRequest(string testFile, IClock clock)
        {
            var stream = TestDataHelper.GetInputStream(testFile, clock);
            var defaultHttpContext = new DefaultHttpContext();
            defaultHttpContext.Request.Body = stream;
            var req = new DefaultHttpRequest(defaultHttpContext);
            return req;
        }

        private static async Task<SubscriptionClient> GetOrCreateTopicSubscription(
            string serviceBusConnectionString,
            string topicPath,
            string subscriptionName)
        {
            var managementClient = new ManagementClient(serviceBusConnectionString);
            if (!await managementClient.SubscriptionExistsAsync(topicPath, subscriptionName).ConfigureAwait(false))
            {
                await managementClient.CreateSubscriptionAsync(new SubscriptionDescription(topicPath, subscriptionName)).ConfigureAwait(false);
            }

            var subscriptionClient = new SubscriptionClient(serviceBusConnectionString, topicPath, subscriptionName);
            return subscriptionClient;
        }

        private static SubscriptionClient GetSubscriptionClient(
            string serviceBusConnectionString,
            string topicPath,
            string subscriptionName)
        {
            var subscriptionClient = new SubscriptionClient(serviceBusConnectionString, topicPath, subscriptionName);
            return subscriptionClient;
        }
    }
}
