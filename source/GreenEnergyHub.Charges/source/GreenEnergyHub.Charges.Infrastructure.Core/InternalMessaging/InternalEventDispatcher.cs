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
using System.Threading;
using System.Threading.Tasks;
using Energinet.DataHub.Core.JsonSerialization;
using GreenEnergyHub.Charges.Application.Messaging;
using GreenEnergyHub.Charges.Infrastructure.Core.MessagingExtensions;
using GreenEnergyHub.Charges.Infrastructure.Core.MessagingExtensions.Factories;

namespace GreenEnergyHub.Charges.Infrastructure.Core.InternalMessaging
{
    public class InternalEventDispatcher : IInternalEventDispatcher
    {
        private readonly IJsonSerializer _jsonSerializer;
        private readonly IServiceBusDispatcher _serviceBusDispatcher;
        private readonly IServiceBusMessageFactory _serviceBusMessageFactory;

        public InternalEventDispatcher(
            IJsonSerializer jsonSerializer,
            IServiceBusDispatcher serviceBusDispatcher,
            IServiceBusMessageFactory serviceBusMessageFactory)
        {
            _jsonSerializer = jsonSerializer;
            _serviceBusDispatcher = serviceBusDispatcher;
            _serviceBusMessageFactory = serviceBusMessageFactory;
        }

        public async Task DispatchAsync<T>(T internalEvent, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(internalEvent);
            var data = _jsonSerializer.Serialize(internalEvent);
            var serviceBusMessage = _serviceBusMessageFactory.CreateInternalMessage(data, internalEvent.GetType().Name);
            await _serviceBusDispatcher.DispatchAsync(serviceBusMessage, internalEvent.GetType(), cancellationToken).ConfigureAwait(false);
        }
    }
}
