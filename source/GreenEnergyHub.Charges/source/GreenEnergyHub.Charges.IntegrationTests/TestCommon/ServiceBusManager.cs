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
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;

namespace GreenEnergyHub.Charges.IntegrationTests.TestCommon
{
    /// <summary>
    /// Encapsulates creating/deleting resources in an existing Azure Service Bus namespace.
    ///
    /// The queue/topic names are build using a combination of the given name as well as an
    /// manager instance name. This ensures we can easily identity resources from a certain test run;
    /// and avoid name clashing if the test suite is executed by two identities at the same time.
    ///
    /// Also contains factory methods for easily creation of related sender clients.
    /// </summary>
    public class ServiceBusManager : IAsyncDisposable
    {
        private static readonly TimeSpan AutoDeleteOnIdleTimeout = TimeSpan.FromMinutes(5);

        public ServiceBusManager(string connectionString)
        {
            ConnectionString = string.IsNullOrWhiteSpace(connectionString)
                ? throw new ArgumentException("Value cannot be null or whitespace.", nameof(connectionString))
                : connectionString;

            InstanceName = $"{DateTimeOffset.UtcNow.Ticks}-{Guid.NewGuid()}";

            AdministrationClient = new ServiceBusAdministrationClient(ConnectionString);
            Client = new ServiceBusClient(ConnectionString);
        }

        public string ConnectionString { get; }

        /// <summary>
        /// Is used as part of the resource names.
        /// Allows us to identify resources created using the same instance (e.g. for debugging).
        /// </summary>
        public string InstanceName { get; }

        private ServiceBusAdministrationClient AdministrationClient { get; }

        /// <summary>
        /// The client share its underlying connection with any senders/receivers created
        /// from it. Disposing the client will close the connection for all senders/receivers.
        /// See also: https://github.com/Azure/azure-sdk-for-net/blob/main/sdk/servicebus/Azure.Messaging.ServiceBus/MigrationGuide.md#connection-pooling
        /// </summary>
        private ServiceBusClient Client { get; set; }

        public async ValueTask DisposeAsync()
        {
            await DisposeAsyncCore().ConfigureAwait(false);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Create a queue with a name based on <paramref name="queueNamePrefix"/> and <see cref="InstanceName"/>.
        /// Tests should preferable cleanup the queue, but otherwise the queue will automatically be deleted after a period of idle time.
        /// </summary>
        /// <param name="queueNamePrefix">The queue name will start with this name.</param>
        /// <param name="maxDeliveryCount"></param>
        /// <param name="lockDuration"></param>
        /// <param name="requireSession"></param>
        /// <returns>Queue properties for the created queue.</returns>
        public async Task<QueueProperties> CreateQueueAsync(string queueNamePrefix, int maxDeliveryCount = 1, TimeSpan? lockDuration = null, bool requireSession = false)
        {
            var queueName = BuildResourceName(queueNamePrefix);
            var createQueueProperties = new CreateQueueOptions(queueName)
            {
                AutoDeleteOnIdle = AutoDeleteOnIdleTimeout,
                MaxDeliveryCount = maxDeliveryCount,
                LockDuration = lockDuration ?? TimeSpan.FromMinutes(1),
                RequiresSession = requireSession,
            };

            var response = await AdministrationClient.CreateQueueAsync(createQueueProperties)
                .ConfigureAwait(false);

            return response.Value;
        }

        public async Task DeleteQueueAsync(string queueName)
        {
            await AdministrationClient.DeleteQueueAsync(queueName)
                .ConfigureAwait(false);
        }

        public ServiceBusSender CreateSenderClient(string queueName)
        {
            return Client.CreateSender(queueName);
        }

        /// <summary>
        /// Build resource name.
        /// Can be overriden to change the final name format (e.g. if we want to make it completely random).
        /// </summary>
        protected virtual string BuildResourceName(string namePrefix)
        {
            return string.IsNullOrWhiteSpace(namePrefix)
                ? throw new ArgumentException("Value cannot be null or whitespace.", nameof(namePrefix))
                : $"{namePrefix}-{InstanceName}";
        }

#pragma warning disable VSTHRD200 // Use "Async" suffix for async methods; Recommendation for async dispose pattern is to use the method name "DisposeAsyncCore": https://docs.microsoft.com/en-us/dotnet/standard/garbage-collection/implementing-disposeasync#the-disposeasynccore-method
        private async ValueTask DisposeAsyncCore()
#pragma warning restore VSTHRD200 // Use "Async" suffix for async methods
        {
            // Disposing the client closes the underlying connection, which is also
            // used by any senders/receivers created with this client.
            await Client.DisposeAsync().ConfigureAwait(false);
        }
    }
}
