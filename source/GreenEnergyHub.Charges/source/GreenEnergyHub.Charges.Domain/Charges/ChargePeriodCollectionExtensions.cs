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
using NodaTime;

namespace GreenEnergyHub.Charges.Domain.Charges
{
    public static class ChargePeriodCollectionExtensions
    {
        public static IEnumerable<Charge> OrderedByReceivedDateTimeAndOrder(this IEnumerable<Charge> source)
        {
            return source
                .OrderByDescending(p => p.ReceivedDateTime)
                .ThenByDescending(p => p.ReceivedOrder);
        }

        public static Charge? GetValidChargePeriodAsOf(this IEnumerable<Charge> source, Instant instant)
        {
            var result = source.Where(charge => charge.StartDateTime <= instant).OrderedByReceivedDateTimeAndOrder();
            return result.FirstOrDefault();
        }
    }
}
