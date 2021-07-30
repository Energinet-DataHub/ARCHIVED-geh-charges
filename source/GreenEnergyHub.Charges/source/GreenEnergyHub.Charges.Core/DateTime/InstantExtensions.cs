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

using System.Diagnostics.CodeAnalysis;
using Google.Protobuf.WellKnownTypes;
using NodaTime;

namespace GreenEnergyHub.Charges.Core.DateTime
{
    public static class InstantExtensions
    {
        public static Instant TimeOrEndDefault(this Instant? instant)
        {
            return instant ?? Instant.FromUtc(9999, 12, 31, 23, 59, 59);
        }

        public static Timestamp ToTimestamp([NotNull] this Instant instant)
        {
            return Timestamp.FromDateTimeOffset(instant.ToDateTimeOffset());
        }
    }
}
