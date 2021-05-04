﻿// Copyright 2020 Energinet DataHub A/S
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
using GreenEnergyHub.Charges.Domain.PostOffice;
using GreenEnergyHub.Json;
using Microsoft.Extensions.Options;

namespace GreenEnergyHub.Charges.Infrastructure.PostOffice
{
    public class PostOfficeService : IPostOfficeService
    {
        private readonly IJsonSerializer _jsonSerializer;
        private readonly IOptions<PostOfficeConfiguration> _options;

        public PostOfficeService(IJsonSerializer jsonSerializer, IOptions<PostOfficeConfiguration> options)
        {
            _jsonSerializer = jsonSerializer;
            _options = options;
        }

        public async Task SendAsync([NotNull] ChargeCommandAcceptedAcknowledgement acknowledgement)
        {
            var connectionString = _options.Value.ConnectionString;
            await using ServiceBusClient client = new (connectionString);

            var queueOrTopicName = _options.Value.TopicName;
            ServiceBusSender sender = client.CreateSender(queueOrTopicName);
            var serializedMessage = _jsonSerializer.Serialize(acknowledgement);
            var message = new ServiceBusMessage(serializedMessage)
            {
                CorrelationId = acknowledgement!.CorrelationId,

                // We set 'Subject' at the moment for a better overview in the AZ portal.
                Subject = acknowledgement.GetType().Name,
            };

            await sender.SendMessageAsync(message).ConfigureAwait(false);
        }
    }
}
