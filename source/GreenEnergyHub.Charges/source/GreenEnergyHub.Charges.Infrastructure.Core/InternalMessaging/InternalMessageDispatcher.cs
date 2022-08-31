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

using System;
using System.Threading;
using System.Threading.Tasks;
using Energinet.DataHub.Core.JsonSerialization;
using Energinet.DataHub.Core.Messaging.Transport;
using GreenEnergyHub.Charges.Application.Messaging;
using GreenEnergyHub.Charges.Infrastructure.Core.MessagingExtensions;
using GreenEnergyHub.Charges.Infrastructure.Core.MessagingExtensions.Factories;

namespace GreenEnergyHub.Charges.Infrastructure.Core.InternalMessaging
{
    public class InternalMessageDispatcher<TOutboundMessage> : IInternalMessageDispatcher<TOutboundMessage>
    where TOutboundMessage : IOutboundMessage
    {
        private readonly IJsonSerializer _jsonSerializer;
        private readonly IServiceBusSender<TOutboundMessage> _serviceBusSender;
        private readonly IServiceBusMessageFactory _serviceBusMessageFactory;

        public InternalMessageDispatcher(
            IJsonSerializer jsonSerializer,
            IServiceBusSender<TOutboundMessage> serviceBusSender,
            IServiceBusMessageFactory serviceBusMessageFactory)
        {
            _jsonSerializer = jsonSerializer;
            _serviceBusSender = serviceBusSender;
            _serviceBusMessageFactory = serviceBusMessageFactory;
        }

        public async Task DispatchAsync(TOutboundMessage message, string subject, CancellationToken cancellationToken = default)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));
            var data = _jsonSerializer.Serialize(message);
            var serviceBusMessage = _serviceBusMessageFactory.CreateInternalMessage(data, subject);
            await _serviceBusSender.Instance.SendMessageAsync(serviceBusMessage, cancellationToken).ConfigureAwait(false);
        }
    }
}
