﻿// Copyright 2020 Energinet DataHub A/S
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
using GreenEnergyHub.Iso8601;
using NodaTime;

namespace GreenEnergyHub.Charges.Core.DateTime
{
    public class ZonedDateTimeService : IZonedDateTimeService
    {
        private readonly IClock _clock;
        private readonly IIso8601ConversionConfiguration _configuration;

        public ZonedDateTimeService(IClock clock, IIso8601ConversionConfiguration configuration)
        {
            _clock = clock;
            _configuration = configuration;
        }

        public ZonedDateTime GetZonedDateTimeNow()
        {
            var timeZone = GetTimeZone();
            return _clock
                .GetCurrentInstant()
                .InZone(timeZone);
        }

        public ZonedDateTime AtStartOfDay()
        {
            var timeZone = GetTimeZone();
            var now = _clock.GetCurrentInstant();
            var today = now.InZone(timeZone).Date;
            return timeZone.AtStartOfDay(today);
        }

        public ZonedDateTime AtStartOfMonth()
        {
            var timeZone = GetTimeZone();
            var now = _clock.GetCurrentInstant();
            var today = now.InZone(timeZone).Date;
            var startMonth = today.PlusDays(1 - today.Day);
            return timeZone.AtStartOfDay(startMonth);
        }

        public ZonedDateTime AtStartOfTodayPlusDays(int numberOfDaysToAdd)
        {
            var timeZone = GetTimeZone();
            var now = _clock.GetCurrentInstant();
            var localNow = now.InZone(timeZone);
            var localWithDaysAdded = localNow.Date.PlusDays(numberOfDaysToAdd);
            return timeZone.AtStartOfDay(localWithDaysAdded);
        }

        public ZonedDateTime AtStartOfThisMonthPlusMonths(int numberOfMonthsToAdd)
        {
            var timeZone = GetTimeZone();
            var now = _clock.GetCurrentInstant();
            var localNow = now.InZone(timeZone).Date;
            var localStartOfMonth = localNow.PlusDays(1 - localNow.Day);
            var localWithMonthsAdded = localStartOfMonth.PlusMonths(numberOfMonthsToAdd);
            return timeZone.AtStartOfDay(localWithMonthsAdded);
        }

        public ZonedDateTime GetZonedDateTime(LocalDateTime localDateTime, ResolutionStrategy strategy)
        {
            if (strategy != ResolutionStrategy.Leniently) throw new NotImplementedException();

            var timeZone = GetTimeZone();
            return timeZone.AtLeniently(localDateTime);
        }

        public ZonedDateTime GetZonedDateTime(Instant instant)
        {
            var timeZone = GetTimeZone();
            return instant.InZone(timeZone);
        }

        private DateTimeZone GetTimeZone()
        {
            var timeZoneId = _configuration.GetTZDatabaseName();

            var timeZone = DateTimeZoneProviders.Tzdb.GetZoneOrNull(timeZoneId);
            if (timeZone == null) throw new ArgumentException("{timeZoneId} is not a supported time zone", timeZoneId);

            return timeZone!;
        }
    }
}
