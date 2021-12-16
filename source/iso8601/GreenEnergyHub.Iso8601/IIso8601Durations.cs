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

        /// <summary>
        /// Retrieves the time which occur by adding the required number of durations
        /// to the start time, making sure that durations higher than 0 is at the nth start
        /// of the duration based on the local timezone
        ///
        /// If numberOfDurations is zero then the start is simply returned even though
        /// it might not match a fixed duration start. This will allows us to support
        /// irregular time/price series points where the first position might be off from the
        /// otherwise fixed intervals
        ///
        /// Example: P1M (monthly) in Europe/Copenhagen
        /// Provided startInstant 2021-01-18T23:00:00Z
        /// numberOfDurations 0 = 2021-01-18T23:00:00Z (provided start as is)
        /// numberOfDurations 1 = 2021-01-31T23:00:00Z (first start of month after startInstant)
        /// numberOfDurations 2 = 2021-02-28T23:00:00Z (second start of month after startInstant)
        ///
        /// Example: PT15M (quarter of hour) in Europe/Copenhagen
        /// Provided startInstant 2021-01-18T23:11:00Z
        /// numberOfDurations 0 = 2021-01-18T23:11:00Z (provided start as is)
        /// numberOfDurations 1 = 2021-01-18T23:15:00Z (first start of quarter of hour after startInstant)
        /// numberOfDurations 2 = 2021-01-18T23:30:00Z (second start of quarter of hour after startInstant)
        /// </summary>
        /// <param name="startInstant">The start instant for the time interval</param>
        /// <param name="duration">ISO 8601 duration</param>
        /// <param name="numberOfDurations">Number of durations to add</param>
        /// <returns>The time going forward to the fixed start of the interval by the
        /// requested number of durations</returns>
        Instant GetTimeFixedToDuration(Instant startInstant, string duration, int numberOfDurations);
    }
}
