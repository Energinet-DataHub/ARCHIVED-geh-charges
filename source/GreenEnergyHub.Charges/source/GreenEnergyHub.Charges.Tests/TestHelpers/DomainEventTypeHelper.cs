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
using System.Linq;
using GreenEnergyHub.Charges.Domain.Dtos.Messages.Events;
using GreenEnergyHub.Charges.Tests.TestCore;

namespace GreenEnergyHub.Charges.Tests.TestHelpers
{
    public class DomainEventTypeHelper
    {
        /// <summary>
        /// Return all non-abstract implementations of <see cref="DomainEvent"/> from domain assembly.
        /// </summary>
        public static IEnumerable<string> GetOrderedDomainEventTypeNames()
        {
            var domainAssembly = AssemblyHelper.GetDomainAssembly();
            var messageTypes = domainAssembly
                .GetTypes()
                .Where(t => typeof(DomainEvent).IsAssignableFrom(t) && t.IsClass && !t.IsAbstract)
                .Select(t => t.Name);
            return messageTypes.OrderBy(x => x).ToList();
        }
    }
}
