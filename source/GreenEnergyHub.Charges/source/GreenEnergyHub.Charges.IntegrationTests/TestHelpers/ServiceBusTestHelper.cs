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
using Microsoft.Azure.ServiceBus;
using Xunit.Abstractions;

namespace GreenEnergyHub.Charges.IntegrationTests.TestHelpers
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class ServiceBusTestHelper
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public ServiceBusTestHelper(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        public async Task<Message> GetMessageFromServiceBusAsync(
            string serviceBusConnectionString,
            string serviceBusTopic,
            string serviceBusSubscription)
        {
            Message receivedMessage = null!;

            var subscriptionClient = GetSubscriptionClient(serviceBusConnectionString, serviceBusTopic, serviceBusSubscription);

            subscriptionClient.RegisterMessageHandler(
                async (message, token) =>
                {
                    var messageJson = Encoding.UTF8.GetString(message.Body);
                    receivedMessage = message;

                    if (messageJson.Length > 0)
                    {
                        _testOutputHelper.WriteLine($"Message received with body: {message.Body.Length}");
                        await subscriptionClient.CompleteAsync(message.SystemProperties.LockToken).ConfigureAwait(false);

                        var inflightMessageHandlerTasksWaitTimeout = new TimeSpan(0, 0, 0, 0, 1);
                        await subscriptionClient.UnregisterMessageHandlerAsync(inflightMessageHandlerTasksWaitTimeout).ConfigureAwait(false);
                    }
                },
                new MessageHandlerOptions(async args =>
                    {
                        await Task.Run(() => _testOutputHelper.WriteLine(args.Exception.ToString())).ConfigureAwait(false);
                    })
                    { MaxConcurrentCalls = 1, AutoComplete = false });

            // ReSharper disable once NotAccessedVariable
            // - for debugging purposes only
            var count = 0;
            while (receivedMessage == null)
            {
                await Task.Delay(50).ConfigureAwait(false);
                ++count;
            }

            return receivedMessage;
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
