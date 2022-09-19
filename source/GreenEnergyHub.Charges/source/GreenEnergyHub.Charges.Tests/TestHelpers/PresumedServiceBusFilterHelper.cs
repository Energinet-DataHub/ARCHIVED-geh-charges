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

namespace GreenEnergyHub.Charges.Tests.TestHelpers
{
    public class PresumedServiceBusFilterHelper
    {
        /// <summary>
        /// This method returns a list of names of all domain event filters we presume to to use in infrastructure-as-code:
        /// - func-functionhost.tf
        /// - sbt-charges-domain-events.tf
        /// </summary>
        /// <returns>List of all domain event filter names</returns>
        public static IOrderedEnumerable<string> GetOrderedServiceBusFiltersPresumedToBeUsedInInfrastructureAsCode()
        {
            // Names are split to prevent Resharper from automatically renaming when class is renamed
            var presumedServiceBusFilters = new List<string>
            {
                "ChargePriceCommandReceived" + "Event",
                "ChargeLinksRejected" + "Event",
                "ChargeLinksReceived" + "Event",
                "ChargeLinksDataAvailableNotified" + "Event",
                "ChargeLinksAccepted" + "Event",
                "ChargeInformationCommandReceived" + "Event",
                "ChargeInformationCommandRejected" + "Event",
                "ChargeInformationCommandAccepted" + "Event",
            };
            return presumedServiceBusFilters.OrderBy(x => x);
        }
    }
}
