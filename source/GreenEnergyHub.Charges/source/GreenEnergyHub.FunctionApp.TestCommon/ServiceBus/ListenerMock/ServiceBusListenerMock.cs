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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using GreenEnergyHub.TestCommon.Diagnostics;

namespace GreenEnergyHub.FunctionApp.TestCommon.ServiceBus.ListenerMock
{
    /// <summary>
    /// Simple Service Bus listener mock with fluent API for setup.
    ///
    /// Can listen on one or more queues/topic-subscriptions.
    ///
    /// Reads any message instantly from its source, and keeps
    /// an in memory log of the messages received.
    /// </summary>
    public sealed class ServiceBusListenerMock : IAsyncDisposable
    {
        public ServiceBusListenerMock(string connectionString, ITestDiagnosticsLogger testLogger)
        {
            ConnectionString = string.IsNullOrWhiteSpace(connectionString)
                ? throw new ArgumentException("Value cannot be null or whitespace.", nameof(connectionString))
                : connectionString;
            TestLogger = testLogger
                ?? throw new ArgumentNullException(nameof(testLogger));

            AdministrationClient = new ServiceBusAdministrationClient(ConnectionString);
            Client = new ServiceBusClient(ConnectionString);

            MutableReceivedMessagesLock = new SemaphoreSlim(1, 1);
            MessageReceivers = new Dictionary<string, ServiceBusProcessor>();
            MessageHandlers = new ConcurrentDictionary<Func<ServiceBusReceivedMessage, bool>, Func<ServiceBusReceivedMessage, Task>>();

            var mutableReceivedMessages = new BlockingCollection<ServiceBusReceivedMessage>();
            MutableReceivedMessages = mutableReceivedMessages;
            ReceivedMessages = mutableReceivedMessages;
        }

        public string ConnectionString { get; }

        public IReadOnlyCollection<ServiceBusReceivedMessage> ReceivedMessages { get; private set; }

        private SemaphoreSlim MutableReceivedMessagesLock { get; }

        private ITestDiagnosticsLogger TestLogger { get; }

        private ServiceBusAdministrationClient AdministrationClient { get; }

        private ServiceBusClient Client { get; }

        private IDictionary<string, ServiceBusProcessor> MessageReceivers { get; }

        private IDictionary<Func<ServiceBusReceivedMessage, bool>, Func<ServiceBusReceivedMessage, Task>> MessageHandlers { get; }

        private BlockingCollection<ServiceBusReceivedMessage> MutableReceivedMessages { get; set; }

        /// <summary>
        /// Close listeners and dispose resources.
        /// </summary>
        public async ValueTask DisposeAsync()
        {
            await DisposeAsyncCore().ConfigureAwait(false);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Add a listener for given queue name.
        /// The listener starts reading messages instantly when added.
        /// </summary>
        /// <param name="queueName">Name of the queue to read messages of.</param>
        public async Task AddQueueListenerAsync(string queueName)
        {
            if (string.IsNullOrWhiteSpace(queueName))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(queueName));
            }

            if (!await AdministrationClient.QueueExistsAsync(queueName).ConfigureAwait(false))
            {
                throw new InvalidOperationException($"Queue '{queueName}' does not exists.");
            }

            var options = CreateDefaultProcessorOptions();
            var receiver = Client.CreateProcessor(queueName, options);
            await AddMessageReceiverAsync(queueName, receiver).ConfigureAwait(false);
        }

        /// <summary>
        /// Add a listener for a given topic/subscription path.
        /// The listener starts reading messages instantly when added.
        /// </summary>
        /// <param name="topicName">Name of the topic.</param>
        /// <param name="subscriptionName">Name of the subscription.</param>
        public async Task AddTopicSubscriptionListenerAsync(string topicName, string subscriptionName)
        {
            if (string.IsNullOrWhiteSpace(topicName))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(topicName));
            }

            if (string.IsNullOrWhiteSpace(subscriptionName))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(subscriptionName));
            }

            if (!await AdministrationClient.SubscriptionExistsAsync(topicName, subscriptionName).ConfigureAwait(false))
            {
                throw new InvalidOperationException($"Topic '{topicName}' with subscription '{subscriptionName}' does not exists.");
            }

            var options = CreateDefaultProcessorOptions();
            var receiver = Client.CreateProcessor(topicName, subscriptionName, options);
            await AddMessageReceiverAsync($"{topicName}-{subscriptionName}", receiver).ConfigureAwait(false);
        }

        /// <summary>
        /// Add a message handler <paramref name="messageHandler"/> that will be used if message matches <paramref name="messageMatcher"/>.
        /// If multiple message handlers can be matched with a message, then only one will be used (no guarantee for which one).
        /// NOTE: The handler supplied will be invoked for all messages already received and currently present in the ReceivedMessages collection.
        /// </summary>
        public async Task AddMessageHandlerAsync(Func<ServiceBusReceivedMessage, bool> messageMatcher, Func<ServiceBusReceivedMessage, Task> messageHandler)
        {
            var alreadyReceivedMessages = new List<ServiceBusReceivedMessage>();
            await MutableReceivedMessagesLock.WaitAsync().ConfigureAwait(false);
            try
            {
                MessageHandlers.Add(messageMatcher, messageHandler);
                alreadyReceivedMessages.AddRange(ReceivedMessages);
            }
            finally
            {
                MutableReceivedMessagesLock.Release();
            }

            // Replay already received messages on messageHandler
            foreach (var alreadyReceivedMessage in alreadyReceivedMessages)
            {
                if (messageMatcher(alreadyReceivedMessage))
                {
                    await messageHandler(alreadyReceivedMessage).ConfigureAwait(false);
                }
            }
        }

        /// <summary>
        /// Reset all listeners.
        /// </summary>
        /// <remarks>Avoid using this, unless you are removing the queue or topic</remarks>
        public async Task ResetMessageReceiversAsync()
        {
            await CleanupMessageReceiversAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Reset handlers and received messages.
        /// </summary>
        /// <remarks>Use this between tests.</remarks>
        public void ResetMessageHandlersAndReceivedMessages()
        {
            MessageHandlers.Clear();
            ClearReceivedMessages();
        }

        private static ServiceBusProcessorOptions CreateDefaultProcessorOptions()
        {
            return new ServiceBusProcessorOptions
            {
                // By default or when AutoCompleteMessages is set to true, the processor will complete the message after executing the message handler
                // Set AutoCompleteMessages to false to [settle messages](https://docs.microsoft.com/en-us/azure/service-bus-messaging/message-transfers-locks-settlement#peeklock) on your own.
                // In both cases, if the message handler throws an exception without settling the message, the processor will abandon the message.
                AutoCompleteMessages = true,

                MaxConcurrentCalls = 1,
            };
        }

        private static bool IsDefaultMessageHandler(KeyValuePair<Func<ServiceBusReceivedMessage, bool>, Func<ServiceBusReceivedMessage, Task>> messageHandler)
        {
            return messageHandler.Equals(default(KeyValuePair<Func<ServiceBusReceivedMessage, bool>, Func<ServiceBusReceivedMessage, Task>>));
        }

        /// <summary>
        /// Default message handler.
        /// </summary>
        private static Func<ServiceBusReceivedMessage, Task> DefaultMessageHandler()
        {
            return message => Task.CompletedTask;
        }

        private Task AddMessageReceiverAsync(string entityPath, ServiceBusProcessor receiver)
        {
            if (MessageReceivers.Keys.Contains(entityPath))
            {
                throw new InvalidOperationException($"A listener for entity path '{entityPath}' already exists.");
            }

            MessageReceivers.Add(new KeyValuePair<string, ServiceBusProcessor>(entityPath, receiver));

            receiver.ProcessMessageAsync += HandleMessageReceivedAsync;
            receiver.ProcessErrorAsync += HandleMessagePumpExceptionAsync;

            return receiver.StartProcessingAsync();
        }

        /// <summary>
        /// All messages received by all message receivers arrives here.
        /// </summary>
        private async Task HandleMessageReceivedAsync(ProcessMessageEventArgs arg)
        {
            Func<ServiceBusReceivedMessage, Task> messageHandler;
            await MutableReceivedMessagesLock.WaitAsync(arg.CancellationToken).ConfigureAwait(false);
            try
            {
                MutableReceivedMessages.Add(arg.Message, arg.CancellationToken);
                messageHandler = GetMessageHandler(arg.Message);
            }
            finally
            {
                MutableReceivedMessagesLock.Release();
            }

            try
            {
                await messageHandler(arg.Message).ConfigureAwait(false);
            }
            catch
            {
                // Ignore the exception so the message is taking off service bus.
            }
        }

        private Func<ServiceBusReceivedMessage, Task> GetMessageHandler(ServiceBusReceivedMessage message)
        {
            var messageHandler = MessageHandlers.FirstOrDefault(x => x.Key(message));
            return IsDefaultMessageHandler(messageHandler)
                ? DefaultMessageHandler()
                : messageHandler.Value;
        }

        /// <summary>
        /// If the underlying message pump throws an exception it will arrive here.
        /// </summary>
        private Task HandleMessagePumpExceptionAsync(ProcessErrorEventArgs eventArgs)
        {
            TestLogger.WriteLine($"{nameof(ServiceBusListenerMock)}: {eventArgs.Exception}");
            return Task.CompletedTask;
        }

#pragma warning disable VSTHRD200 // Use "Async" suffix for async methods; Recommendation for async dispose pattern is to use the method name "DisposeAsyncCore": https://docs.microsoft.com/en-us/dotnet/standard/garbage-collection/implementing-disposeasync#the-disposeasynccore-method
        private async ValueTask DisposeAsyncCore()
#pragma warning restore VSTHRD200 // Use "Async" suffix for async methods
        {
            await ResetMessageReceiversAsync().ConfigureAwait(false);
            ResetMessageHandlersAndReceivedMessages();
            await Client.DisposeAsync().ConfigureAwait(false);
        }

        private async Task CleanupMessageReceiversAsync()
        {
            foreach (var registration in MessageReceivers)
            {
                var receiver = registration.Value;
                await receiver.StopProcessingAsync().ConfigureAwait(false);
                await receiver.DisposeAsync().ConfigureAwait(false);
            }

            MessageReceivers.Clear();
        }

        private void ClearReceivedMessages()
        {
            MutableReceivedMessages.CompleteAdding();
            MutableReceivedMessages.Dispose();

            // As soon as we have called "CompleteAdding" we must create a new instance.
            var mutableReceivedMessages = new BlockingCollection<ServiceBusReceivedMessage>();
            MutableReceivedMessages = mutableReceivedMessages;
            ReceivedMessages = mutableReceivedMessages;
        }
    }
}
