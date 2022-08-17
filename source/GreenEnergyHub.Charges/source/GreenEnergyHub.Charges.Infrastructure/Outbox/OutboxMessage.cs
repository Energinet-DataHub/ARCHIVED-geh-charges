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
using NodaTime;

namespace GreenEnergyHub.Charges.Infrastructure.Outbox
{
    public class OutboxMessage
    {
        private OutboxMessage(string data, string correlationTraceContext, string type, Instant creationDate)
        {
            Id = Guid.NewGuid();
            Type = type;
            Data = data;
            CorrelationTraceContext = correlationTraceContext;
            CreationDate = creationDate;
        }

        public Guid Id { get; }

        public string Type { get; }

        public string Data { get; }

        public string CorrelationTraceContext { get; }

        public Instant CreationDate { get; }

        public Instant? ProcessedDate { get; private set; }

        public static OutboxMessage Create(string data, string correlationTraceContext, string type, Instant creationDate)
        {
            return new OutboxMessage(
                data,
                correlationTraceContext,
                type,
                creationDate);
        }

        public void SetProcessed(Instant when)
        {
            ProcessedDate = when;
        }
    }
}
