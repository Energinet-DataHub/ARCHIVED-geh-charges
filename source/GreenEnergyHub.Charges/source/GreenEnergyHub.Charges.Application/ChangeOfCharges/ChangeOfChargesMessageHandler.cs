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
using GreenEnergyHub.Charges.Domain.ChangeOfCharges.Message;
using GreenEnergyHub.Charges.Domain.ChangeOfCharges.Result;
using GreenEnergyHub.Charges.Domain.ChangeOfCharges.Transaction;
using GreenEnergyHub.Charges.Domain.Events.Local;
using GreenEnergyHub.Json;
using NodaTime;

namespace GreenEnergyHub.Charges.Application.ChangeOfCharges
{
    public class ChangeOfChargesMessageHandler : IChangeOfChargesMessageHandler
    {
        private const string ChargesServiceBusReceiverTransactionQueueName =
            "CHARGES_SERVICE_BUS_RECEIVER_TRANSACTION_QUEUE_NAME";

        private const string ChargesServiceBusConnectionString =
            "CHARGES_SERVICE_BUS_CONNECTION_STRING";

        private readonly IJsonSerializer _jsonSerializer;

        public ChangeOfChargesMessageHandler(IJsonSerializer jsonSerializer)
        {
            _jsonSerializer = jsonSerializer;
        }

        public async Task<ChangeOfChargesMessageResult> HandleAsync(ChangeOfChargesMessage message)
        {
            if (message != null)
            {
                foreach (ChangeOfChargesTransaction transaction in message.Transactions)
                {
                    await PublishLocalEventAsync(transaction).ConfigureAwait(false);
                }
            }

            return new ChangeOfChargesMessageResult();
        }

        private static string GetEnvironmentVariable(string name)
        {
            var environmentVariable = Environment.GetEnvironmentVariable(name) ??
                            throw new ArgumentNullException(name, "does not exist in configuration settings");
            return environmentVariable;
        }

        private async Task PublishLocalEventAsync(ChangeOfChargesTransaction changeOfChargesTransaction)
        {
            var connectionString = GetEnvironmentVariable(ChargesServiceBusConnectionString);
            // create a Service Bus client
            await using ServiceBusClient client = new (connectionString);

            // create a sender for the queue
            var queueOrTopicName = GetEnvironmentVariable(ChargesServiceBusReceiverTransactionQueueName);
            ServiceBusSender sender = client.CreateSender(queueOrTopicName);

            var chargeTransactionReceivedEvent =
                new ChargeTransactionReceived(
                    SystemClock.Instance.GetCurrentInstant(),
                    Guid.NewGuid().ToString(),
                    changeOfChargesTransaction);

            var serializedMessage = _jsonSerializer.Serialize(chargeTransactionReceivedEvent);

            // create a message that we can send
            var message = new ServiceBusMessage(serializedMessage);

            // send the message
            await sender.SendMessageAsync(message).ConfigureAwait(false);
        }
    }
}
