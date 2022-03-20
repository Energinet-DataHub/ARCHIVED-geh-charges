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

namespace GreenEnergyHub.Charges.TestCore
{
    public static class InstantHelper
    {
        public static Instant GetNowUtc()
        {
            return Instant.FromDateTimeUtc(DateTime.Now.ToUniversalTime());
        }

        public static Instant GetEndDefault()
        {
            return Instant.FromUtc(9999, 12, 31, 23, 59, 59);
        }

        public static Instant GetYesterdayAtMidnightUtc()
        {
            return Instant.FromDateTimeUtc(DateTime.Now.AddDays(-1).Date.ToUniversalTime());
        }

        public static Instant GetTodayAtMidnightUtc()
        {
            return Instant.FromDateTimeUtc(DateTime.Now.Date.ToUniversalTime());
        }

        public static Instant GetTomorrowAtMidnightUtc()
        {
            return Instant.FromDateTimeUtc(DateTime.Now.AddDays(1).Date.ToUniversalTime());
        }

        public static Instant GetTodayPlusDaysAtMidnightUtc(int noOfDaysToAdd)
        {
            return Instant.FromDateTimeUtc(DateTime.Now.AddDays(noOfDaysToAdd).Date.ToUniversalTime());
        }
    }
}
