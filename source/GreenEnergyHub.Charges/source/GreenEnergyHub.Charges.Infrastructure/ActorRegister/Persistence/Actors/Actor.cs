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
using System.Collections.Generic;

namespace GreenEnergyHub.Charges.Infrastructure.ActorRegister.Persistence.Actors
{
    /// <summary>
    /// An immutable actor.
    /// </summary>
    public class Actor
    {
        private List<Role> _roles;

        public Actor(
            Guid id,
            string identificationNumber,
            IdentificationType identificationType,
            string name,
            List<Role> roles,
            bool active)
        {
            Id = id;
            _roles = roles;
            IdentificationNumber = identificationNumber;
            IdentificationType = identificationType;
            Name = name;
            Active = active;
        }

        /// <summary>
        /// Solely used by persistence infrastructure.
        /// </summary>
        private Actor()
        {
            IdentificationNumber = null!;
            Name = null!;
            _roles = null!;
        }

        public Guid Id { get; }

        /// <summary>
        /// The GLN or EIC number. E.g. "8200000001409".
        /// </summary>
        public string IdentificationNumber { get; }

        public IdentificationType IdentificationType { get; }

        public string Name { get; }

        public IReadOnlyCollection<Role> Roles => _roles;

        public bool Active { get; }
    }
}
