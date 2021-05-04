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

using System.Threading;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using GreenEnergyHub.Messaging.Transport;

namespace GreenEnergyHub.Charges.Infrastructure.Messaging
{
    /// <summary>
    /// Implementation of a channel sending information over the Azure Service Bus
    /// A new channel should be used for each queue or topic you wish to send to
    /// Each channel should be used as scoped context for dependency injection
    /// while ServiceBusSender and the ServiceBusClient used to make the sender should be singletons
    /// </summary>
    public class ServiceBusChannel : Channel
    {
        private readonly ServiceBusSender _serviceBusSender;
        private readonly ICorrelationContext _correlationContext;

        public ServiceBusChannel(ServiceBusSender serviceBusSender, ICorrelationContext correlationContext)
        {
            _serviceBusSender = serviceBusSender;
            _correlationContext = correlationContext;
        }

        /// <summary>
        /// Write the <paramref name="data"/> to the channel
        /// </summary>
        /// <param name="data">data to write</param>
        /// <param name="cancellationToken">cancellation token</param>
        protected override async Task WriteAsync(byte[] data, CancellationToken cancellationToken = default)
        {
            var message = GetServiceBusMessage(data);

            await _serviceBusSender.SendMessageAsync(message, cancellationToken).ConfigureAwait(false);
        }

        private ServiceBusMessage GetServiceBusMessage(byte[] data)
        {
            return new ServiceBusMessage(data)
            {
                CorrelationId = _correlationContext.CorrelationId,
            };
        }
    }
}
