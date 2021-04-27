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

using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using GreenEnergyHub.Charges.Domain.Events.Local;
using GreenEnergyHub.Json;

namespace GreenEnergyHub.Charges.Application.ChangeOfCharges
{
    public class InternalEventPublisher : IInternalEventPublisher
    {
        private readonly IJsonSerializer _jsonSerializer;
        private readonly IInternalEventCommunicationConfiguration _communicationConfiguration;

        public InternalEventPublisher(
            IJsonSerializer jsonSerializer,
            IInternalEventCommunicationConfiguration communicationConfiguration)
        {
            _jsonSerializer = jsonSerializer;
            _communicationConfiguration = communicationConfiguration;
        }

        public async Task PublishAsync([NotNull] IInternalEvent internalEvent)
        {
            var connectionString = _communicationConfiguration.GetConnectionString(internalEvent);
            await using ServiceBusClient client = new (connectionString);

            var queueOrTopicName = _communicationConfiguration.GetTopic(internalEvent);
            ServiceBusSender sender = client.CreateSender(queueOrTopicName);
            var serializedMessage = _jsonSerializer.Serialize(internalEvent);
            var message = new ServiceBusMessage(serializedMessage)
            {
                CorrelationId = internalEvent.CorrelationId,
                Subject = internalEvent.Filter, // We set 'Subject' at the moment for a better overview in the AZ portal.
            };

            await sender.SendMessageAsync(message).ConfigureAwait(false);
        }
    }
}
