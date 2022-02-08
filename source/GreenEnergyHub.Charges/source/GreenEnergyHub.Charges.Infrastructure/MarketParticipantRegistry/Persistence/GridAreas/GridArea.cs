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

namespace GreenEnergyHub.Charges.Infrastructure.MarketParticipantRegistry.Persistence.GridAreas
{
    /// <summary>
    /// Immutable grid area.
    /// </summary>
    public class GridArea
    {
        /// <summary>
        /// Solely used by persistence infrastructure.
        /// </summary>
        private GridArea()
        {
            Code = null!;
            Name = null!;
            PriceAreaCode = null!;
        }

        public Guid Id { get; }

        /// <summary>
        /// Database record ID.
        /// Should probably not be used.
        /// </summary>
        public int RecordId { get; }

        /// <summary>
        /// E.g "870".
        /// </summary>
        public string Code { get; }

        /// <summary>
        /// E.g. "alfa".
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// E.g. "DK1".
        /// </summary>
        public string PriceAreaCode { get; }

        /// <summary>
        /// True if the area is active.
        /// </summary>
        public bool Active { get; }

        /// <summary>
        /// The ID of the responsible grid access provider.
        /// </summary>
        public Guid ActorId { get; }
    }
}
