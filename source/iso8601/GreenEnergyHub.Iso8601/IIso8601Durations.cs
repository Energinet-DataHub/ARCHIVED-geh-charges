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

using NodaTime;

namespace GreenEnergyHub.Iso8601
{
    /// <summary>
    /// Util functions performing  time duration calculations in accordance to ISO 8601 Durations
    /// </summary>
    public interface IIso8601Durations
    {
        /// <summary>
        /// Add a time interval consisting of a number of durations
        /// </summary>
        /// <param name="startInstant">The start instant for the time interval</param>
        /// <param name="duration">ISO 8601 duration</param>
        /// <param name="numberOfDurations">Number of durations to add</param>
        /// <returns>End time for time interval</returns>
        Instant AddDuration(Instant startInstant, string duration, int numberOfDurations);
    }
}
