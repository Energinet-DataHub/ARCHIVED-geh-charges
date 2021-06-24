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
using GreenEnergyHub.Charges.Domain.Messages;
using Microsoft.Azure.ServiceBus;
using Newtonsoft.Json;
using Xunit.Abstractions;
using JsonSerializer = GreenEnergyHub.Charges.Core.Json.JsonSerializer;

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

        public async Task<(T receivedEvent, Message receivedMessage)> GetMessageFromServiceBusAsync<T>(
            string serviceBusConnectionString,
            string serviceBusTopic,
            string serviceBusSubscription,
            string correlationId)
        {
            Message receivedMessage = null!;
            var completion = new TaskCompletionSource<T>();
            var subscriptionClient = GetSubscriptionClient(serviceBusConnectionString, serviceBusTopic, serviceBusSubscription);

            subscriptionClient.RegisterMessageHandler(
                async (serviceBusMessage, _) =>
                {
                    var jsonMessage = Encoding.UTF8.GetString(serviceBusMessage.Body);
                    var deserializedMessage = new JsonSerializer().Deserialize<T>(jsonMessage);

                    if (deserializedMessage is IMessage message && message.CorrelationId == correlationId)
                    {
                        receivedMessage = serviceBusMessage;

                        _testOutputHelper.WriteLine($"Message received with body: {serviceBusMessage.Body.Length}");
                        await subscriptionClient.CompleteAsync(serviceBusMessage.SystemProperties.LockToken).ConfigureAwait(false);

                        var inflightMessageHandlerTasksWaitTimeout = new TimeSpan(0, 0, 0, 0, 1);
                        await subscriptionClient.UnregisterMessageHandlerAsync(inflightMessageHandlerTasksWaitTimeout).ConfigureAwait(false);

                        completion.SetResult(deserializedMessage);
                    }
                }, new MessageHandlerOptions(async args =>
                    {
                        await Task.Run(() => _testOutputHelper.WriteLine(args.Exception.ToString())).ConfigureAwait(false);
                    })
                    { MaxConcurrentCalls = 1, AutoComplete = false });

            var receivedEvent = await completion.Task.ConfigureAwait(false);

            return (receivedEvent, receivedMessage);
        }

        public static void RegisterSubscriptionClientMessageHandler<T>(
            [NotNull] ISubscriptionClient subscriptionClient,
            [NotNull] TaskCompletionSource<T> completion)
        {
            subscriptionClient.RegisterMessageHandler(
                (message, _) =>
                {
                    try
                    {
                        var json = Encoding.UTF8.GetString(message.Body);
                        var ev = JsonConvert.DeserializeObject<T>(json);
                        completion.SetResult(ev!);
                    }
#pragma warning disable CA1031 // allow catch of exception
                    catch (Exception exception)
                    {
                        completion.SetException(exception);
                    }
#pragma warning restore CA1031
                    return Task.CompletedTask;
                }, new MessageHandlerOptions(ExceptionReceivedHandlerAsync));
        }

        private static SubscriptionClient GetSubscriptionClient(
            string serviceBusConnectionString,
            string topicPath,
            string subscriptionName)
        {
            var subscriptionClient = new SubscriptionClient(serviceBusConnectionString, topicPath, subscriptionName);
            return subscriptionClient;
        }

        private static Task ExceptionReceivedHandlerAsync(ExceptionReceivedEventArgs arg)
        {
            throw new MessageNotFoundException(arg.ToString(), arg.Exception);
        }
    }
}
