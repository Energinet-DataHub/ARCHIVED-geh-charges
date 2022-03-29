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

using System.Globalization;
using Google.Protobuf.WellKnownTypes;
using NodaTime;

namespace GreenEnergyHub.Charges.Core.DateTime
{
    public static class InstantExtensions
    {
        private const string TimeAndPriceSeriesDateTimeFormat = "yyyy-MM-dd\\THH:mm\\Z";
        private const string CreatedDateTimeFormat = "yyyy-MM-dd\\THH:mm:ss\\Z";

        public static Instant GetEndDefault()
        {
            return Instant.FromUtc(9999, 12, 31, 23, 59, 59);
        }

        public static Instant TimeOrEndDefault(this Instant? instant)
        {
            // This value is decided for the ProtoBuf contracts.
            // It should thus not be replaced by e.g. Instant.MaxValue.
            return instant ?? GetEndDefault();
        }

        public static Timestamp ToTimestamp(this Instant instant)
        {
            return Timestamp.FromDateTimeOffset(instant.ToDateTimeOffset());
        }

        public static bool IsEndDefault(this Instant instant)
        {
            return instant == GetEndDefault();
        }

        public static string GetCreatedDateTimeFormat(this Instant instant)
        {
            return instant.ToString(CreatedDateTimeFormat, CultureInfo.InvariantCulture);
        }

        public static string GetTimeAndPriceSeriesDateTimeFormat(this Instant instant)
        {
            return instant.ToString(TimeAndPriceSeriesDateTimeFormat, CultureInfo.InvariantCulture);
        }
    }
}
