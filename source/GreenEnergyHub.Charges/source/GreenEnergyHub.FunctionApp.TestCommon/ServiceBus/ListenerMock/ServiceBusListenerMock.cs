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
using GreenEnergyHub.TestCommon.Diagnostics;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using Microsoft.Azure.ServiceBus.Management;

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

            ManagementClient = new ManagementClient(ConnectionString);

            MutableReceivedMessagesLock = new SemaphoreSlim(1, 1);
            MessageReceivers = new Dictionary<string, IReceiverClient>();
            MessageHandlers = new ConcurrentDictionary<Func<Message, bool>, Func<Message, Task>>();

            var mutableReceivedMessages = new BlockingCollection<Message>();
            MutableReceivedMessages = mutableReceivedMessages;
            ReceivedMessages = mutableReceivedMessages;
        }

        public string ConnectionString { get; }

        public IReadOnlyCollection<Message> ReceivedMessages { get; private set; }

        private SemaphoreSlim MutableReceivedMessagesLock { get; }

        private ITestDiagnosticsLogger TestLogger { get; }

        private ManagementClient ManagementClient { get; }

        private IDictionary<string, IReceiverClient> MessageReceivers { get; }

        private IDictionary<Func<Message, bool>, Func<Message, Task>> MessageHandlers { get; }

        private BlockingCollection<Message> MutableReceivedMessages { get; set; }

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

            if (!await ManagementClient.QueueExistsAsync(queueName).ConfigureAwait(false))
            {
                throw new InvalidOperationException($"Queue '{queueName}' does not exists.");
            }

            AddMessageReceiver(queueName);
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

            if (!await ManagementClient.SubscriptionExistsAsync(topicName, subscriptionName).ConfigureAwait(false))
            {
                throw new InvalidOperationException($"Topic '{topicName}' with subscription '{subscriptionName}' does not exists.");
            }

            AddMessageReceiver(EntityNameHelper.FormatSubscriptionPath(topicName, subscriptionName));
        }

        /// <summary>
        /// Add a message handler <paramref name="messageHandler"/> that will be used if message matches <paramref name="messageMatcher"/>.
        /// If multiple message handlers can be matched with a message, then only one will be used (no guarantee for which one).
        /// NOTE: The handler supplied will be invoked for all messages already received and currently present in the ReceivedMessages collection.
        /// </summary>
        public async Task AddMessageHandlerAsync(Func<Message, bool> messageMatcher, Func<Message, Task> messageHandler)
        {
            var alreadyReceivedMessages = new List<Message>();
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
        /// Reset all listener.
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

        private static bool IsDefaultMessageHandler(KeyValuePair<Func<Message, bool>, Func<Message, Task>> messageHandler)
        {
            return messageHandler.Equals(default(KeyValuePair<Func<Message, bool>, Func<Message, Task>>));
        }

        /// <summary>
        /// Default message handler.
        /// </summary>
        private static Func<Message, Task> DefaultMessageHandler()
        {
            return message => Task.CompletedTask;
        }

        private void AddMessageReceiver(string entityPath)
        {
            if (MessageReceivers.Keys.Contains(entityPath))
            {
                throw new InvalidOperationException($"A listener for entity path '{entityPath}' already exists.");
            }

            var messageReceiver = new MessageReceiver(ConnectionString, entityPath, ReceiveMode.PeekLock);
            messageReceiver.RegisterMessageHandler(HandleMessageReceivedAsync, HandleMessagePumpExceptionAsync);

            MessageReceivers.Add(new KeyValuePair<string, IReceiverClient>(entityPath, messageReceiver));
        }

        /// <summary>
        /// All messages received by all message receivers arrives here.
        /// </summary>
        private async Task HandleMessageReceivedAsync(Message message, CancellationToken cancellationToken)
        {
            var messageClone = message.Clone();

            Func<Message, Task> messageHandler;
            await MutableReceivedMessagesLock.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                MutableReceivedMessages.Add(messageClone, cancellationToken);
                messageHandler = GetMessageHandler(messageClone);
            }
            finally
            {
                MutableReceivedMessagesLock.Release();
            }

            try
            {
                await messageHandler(message).ConfigureAwait(false);
            }
            catch
            {
                // Ignore the exception so the message is taking off service bus.
            }
        }

        private Func<Message, Task> GetMessageHandler(Message message)
        {
            var messageHandler = MessageHandlers.FirstOrDefault(x => x.Key(message));
            return IsDefaultMessageHandler(messageHandler)
                ? DefaultMessageHandler()
                : messageHandler.Value;
        }

        /// <summary>
        /// If the underlying message pump throws an exception it will arrive here.
        /// </summary>
        private Task HandleMessagePumpExceptionAsync(ExceptionReceivedEventArgs eventArgs)
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
            await ManagementClient.CloseAsync().ConfigureAwait(false);
        }

        private async Task CleanupMessageReceiversAsync()
        {
            foreach (var registration in MessageReceivers)
            {
                var receiver = registration.Value;
                await receiver.UnregisterMessageHandlerAsync(TimeSpan.FromSeconds(5));
                await receiver.CloseAsync().ConfigureAwait(false);
            }

            MessageReceivers.Clear();
        }

        private void ClearReceivedMessages()
        {
            MutableReceivedMessages.CompleteAdding();
            MutableReceivedMessages.Dispose();

            // As soon as we have called "CompleteAdding" we must create a new instance.
            var mutableReceivedMessages = new BlockingCollection<Message>();
            MutableReceivedMessages = mutableReceivedMessages;
            ReceivedMessages = mutableReceivedMessages;
        }
    }
}
