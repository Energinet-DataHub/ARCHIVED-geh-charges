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
using NodaTime;

namespace GreenEnergyHub.Charges.Domain.ChargeLinks
{
    public class ChargeLinkOperation
    {
        private const int MaxIdLength = 100;
        private const int RequiredCorrelationIdLength = 36;

        public ChargeLinkOperation(string id, string correlationId)
        {
            if (id == null) throw new ArgumentNullException(nameof(id));
            if (id.Length > MaxIdLength) throw new ArgumentException($"Must not exceed {MaxIdLength} characters.", nameof(id));

            if (correlationId == null) throw new ArgumentNullException(nameof(correlationId));
            if (correlationId.Length > RequiredCorrelationIdLength) throw new ArgumentException($"Must be {RequiredCorrelationIdLength} characters long.", nameof(correlationId));

            Id = id;
            CorrelationId = correlationId;
        }

        /// <summary>
        ///  Used by persistence to hydrate. So don't risc failing hydration by validating here.
        /// </summary>
        private ChargeLinkOperation(string id, string correlationId, Instant writeDateTime)
        {
            Id = id;
            CorrelationId = correlationId;
            WriteDateTime = writeDateTime;
        }

        public string Id { get; }

        public string CorrelationId { get; }

        /// <summary>
        /// Time of persistence. Database generated value.
        /// </summary>
        public Instant WriteDateTime { get; }
    }
}
