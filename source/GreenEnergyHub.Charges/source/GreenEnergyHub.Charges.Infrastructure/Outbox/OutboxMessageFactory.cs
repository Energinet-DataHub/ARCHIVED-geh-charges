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

using Energinet.DataHub.Core.App.FunctionApp.Middleware.CorrelationId;
using GreenEnergyHub.Charges.Application.Charges.Events;
using GreenEnergyHub.Json;
using NodaTime;

namespace GreenEnergyHub.Charges.Infrastructure.Outbox
{
    public class OutboxMessageFactory : IOutboxMessageFactory
    {
        private readonly IJsonSerializer _jsonSerializer;
        private readonly IClock _clock;
        private readonly ICorrelationContext _correlationContext;

        public OutboxMessageFactory(
            IJsonSerializer jsonSerializer,
            IClock clock,
            ICorrelationContext correlationContext)
        {
            _jsonSerializer = jsonSerializer;
            _clock = clock;
            _correlationContext = correlationContext;
        }

        public OutboxMessage CreateFrom(ChargePriceOperationsRejectedEvent chargePriceOperationsRejectedEvent)
        {
            var data = _jsonSerializer.Serialize(chargePriceOperationsRejectedEvent);
            return OutboxMessage.Create(
                data,
                _correlationContext.Id,
                chargePriceOperationsRejectedEvent.GetType().ToString(),
                _clock.GetCurrentInstant());
        }
    }
}
