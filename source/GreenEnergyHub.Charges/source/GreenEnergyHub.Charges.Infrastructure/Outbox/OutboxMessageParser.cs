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
using Energinet.DataHub.Core.JsonSerialization;
using GreenEnergyHub.Charges.Application.Charges.Events;
using GreenEnergyHub.Charges.Domain.Dtos.Messages.Events;

namespace GreenEnergyHub.Charges.Infrastructure.Outbox
{
    public class OutboxMessageParser : IOutboxMessageParser
    {
        private readonly IJsonSerializer _jsonSerializer;

        public OutboxMessageParser(IJsonSerializer jsonSerializer)
        {
            _jsonSerializer = jsonSerializer;
        }

        public InternalEvent Parse(string outboxMessageType, string data)
        {
            if (outboxMessageType == typeof(PriceRejectedEvent).FullName)
            {
                var obj = _jsonSerializer.Deserialize(data, typeof(PriceRejectedEvent));
                return (PriceRejectedEvent)obj;
            }

            if (outboxMessageType == typeof(PriceConfirmedEvent).FullName)
            {
                return (PriceConfirmedEvent)_jsonSerializer.Deserialize(data, typeof(PriceConfirmedEvent));
            }

            throw new ArgumentOutOfRangeException($"Could not parse outbox event of type: {outboxMessageType} with data {data}");
        }
    }
}
