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

using System.Collections.Generic;
using Azure.Messaging.ServiceBus;
using Energinet.DataHub.Core.FunctionApp.Common.Abstractions.Actor;
using Energinet.DataHub.Core.FunctionApp.Common.Abstractions.ServiceBus;
using GreenEnergyHub.Charges.Infrastructure.Core.Correlation;
using GreenEnergyHub.Charges.Infrastructure.Core.MessageMetaData;

namespace GreenEnergyHub.Charges.Infrastructure.Core.MessagingExtensions.Factories
{
    public class ServiceBusMessageFactory : IServiceBusMessageFactory
    {
        private readonly ICorrelationContext _correlationContext;
        private readonly IMessageMetaDataContext _messageMetaDataContext;
        private readonly IActorContext _actorContext;

        public ServiceBusMessageFactory(
            ICorrelationContext correlationContext,
            IMessageMetaDataContext messageMetaDataContext,
            IActorContext actorContext)
        {
            _correlationContext = correlationContext;
            _messageMetaDataContext = messageMetaDataContext;
            _actorContext = actorContext;
        }

        public ServiceBusMessage CreateInternalMessage(string data)
        {
            if (_messageMetaDataContext.IsReplyToSet())
            {
                return new ServiceBusMessage(data)
                {
                    CorrelationId = _correlationContext.Id,
                    ApplicationProperties =
                    {
                        new KeyValuePair<string, object>("ReplyTo", _messageMetaDataContext.ReplyTo),
                    },
                };
            }

            return new ServiceBusMessage(data)
            {
                CorrelationId = _correlationContext.Id,
                ApplicationProperties =
                    {
                        // Actor is always set, for internal messages which originates from Charges Http ingestion point.
                        new KeyValuePair<string, object>(Constants.ServiceBusIdentityKey, _actorContext.CurrentActor!.AsString()),
                    },
            };
        }

        public ServiceBusMessage CreateExternalMessage(byte[] data)
        {
            return new ServiceBusMessage(data) { CorrelationId = _correlationContext.Id, };
        }
    }
}
