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

namespace GreenEnergyHub.Iso8601
{
    public class Iso8601Durations : IIso8601Durations
    {
        private readonly IIso8601ConversionConfiguration _configuration;

        public Iso8601Durations(IIso8601ConversionConfiguration configuration)
        {
            _configuration = configuration;
        }

        public Instant AddDuration(Instant startInstant, string duration, int numberOfDurations)
        {
            return duration switch
            {
                "PT1H" => startInstant.Plus(Duration.FromHours(numberOfDurations)),
                "PT15M" => startInstant.Plus(Duration.FromMinutes(15 * numberOfDurations)),
                "P1D" => AddDayDuration(startInstant, numberOfDurations),
                "P1M" => AddMonthDuration(startInstant, numberOfDurations),
                _ => throw new ArgumentException($"Unknown time resolution: {duration}"),
            };
        }

        public Instant GetTimeFixedToDuration(Instant startInstant, string duration, int numberOfDurations)
        {
            if (numberOfDurations == 0)
            {
                return startInstant;
            }

            var fixedStart = GetFixedStartOfInterval(startInstant, duration);
            return AddDuration(fixedStart, duration, numberOfDurations);
        }

        private Instant AddMonthDuration(Instant startInstant, int numberOfDurations)
        {
            /* Month handling in time is tricky, as that requires an understanding of what a month is in local time, instead of UTC as we normally use.
             * As a result, we need to convert into a configurable time zone, use that time zones local time and then add the required number of months.
             * Afterwards we will then have to go back from localtime to the zoned time, and then back from that to UTC to return the result
             * If we do not do it this way, we will be thrown off by daylights savings, number of days in a month etc. */
            var zonedTime = ConvertInstantToZonedTime(startInstant);

            var localResult = zonedTime.LocalDateTime.PlusMonths(numberOfDurations);

            return ConvertLocalTimeToUtc(localResult);
        }

        private Instant AddDayDuration(Instant startInstant, int numberOfDurations)
        {
            /* Day handling in time is tricky, as that requires an understanding of what a day is in local time, instead of UTC as we normally use.
             * As a result, we need to convert into a configurable time zone, use that time zones local time and then add the required number of days.
             * Afterwards we will then have to go back from localtime to the zoned time, and then back from that to UTC to return the result
             * If we do not do it this way, we will be thrown off by daylights savings etc. */
            var zonedTime = ConvertInstantToZonedTime(startInstant);

            var localResult = zonedTime.LocalDateTime.PlusDays(numberOfDurations);

            return ConvertLocalTimeToUtc(localResult);
        }

        private ZonedDateTime ConvertInstantToZonedTime(Instant instant)
        {
            var timeZone = GetTimeZone();

            return instant.InZone(timeZone);
        }

        private Instant ConvertLocalTimeToUtc(LocalDateTime localTime)
        {
            var timeZone = GetTimeZone();

            var zonedResult = localTime.InZoneStrictly(timeZone);
            var utcZonedResult = zonedResult.WithZone(DateTimeZone.Utc);

            return utcZonedResult.ToInstant();
        }

        private DateTimeZone GetTimeZone()
        {
            var timeZoneId = _configuration.GetTZDatabaseName();

            var timeZone = DateTimeZoneProviders.Tzdb.GetZoneOrNull(timeZoneId);
            if (timeZone == null) throw new ArgumentException("{timeZoneId} is not a supported time zone", timeZoneId);

            return timeZone!;
        }

        private Instant GetFixedStartOfInterval(Instant time, string duration)
        {
            // The fixed start will always be equal or earlier than the provided time
            return duration switch
            {
                "P1M" => GetStartOfMonth(time),
                "P1D" => GetStartOfDay(time),
                "PT1H" => GetStartOfHour(time),
                "PT15M" => GetStartOfQuarterOfHour(time),
                _ => throw new ArgumentException($"Unsupported time resolution: {duration}"),
            };
        }

        private Instant GetStartOfMonth(Instant time)
        {
            var zonedTime = ConvertInstantToZonedTime(time);

            var localStartOfMonth = new LocalDateTime(
                zonedTime.LocalDateTime.Year,
                zonedTime.LocalDateTime.Month,
                1,
                0,
                0);

            return ConvertLocalTimeToUtc(localStartOfMonth);
        }

        private Instant GetStartOfDay(Instant time)
        {
            var zonedTime = ConvertInstantToZonedTime(time);

            var localStartOfDay = new LocalDateTime(
                zonedTime.LocalDateTime.Year,
                zonedTime.LocalDateTime.Month,
                zonedTime.LocalDateTime.Day,
                0,
                0);

            return ConvertLocalTimeToUtc(localStartOfDay);
        }

        private Instant GetStartOfHour(Instant time)
        {
            var zonedTime = ConvertInstantToZonedTime(time);

            var localStartOfHour = new LocalDateTime(
                zonedTime.LocalDateTime.Year,
                zonedTime.LocalDateTime.Month,
                zonedTime.LocalDateTime.Day,
                zonedTime.Hour,
                0);

            return ConvertLocalTimeToUtc(localStartOfHour);
        }

        private Instant GetStartOfQuarterOfHour(Instant time)
        {
            var zonedTime = ConvertInstantToZonedTime(time);
            var localStartOfQuarterOfHour = new LocalDateTime(
                zonedTime.LocalDateTime.Year,
                zonedTime.LocalDateTime.Month,
                zonedTime.LocalDateTime.Day,
                zonedTime.Hour,
                zonedTime.Minute / 15 * 15); // Integer division, so division and multiplication does not cancel each other out

            return ConvertLocalTimeToUtc(localStartOfQuarterOfHour);
        }
    }
}
