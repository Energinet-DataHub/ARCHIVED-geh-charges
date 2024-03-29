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

using GreenEnergyHub.Charges.Core.DateTime;
using GreenEnergyHub.Iso8601;
using NodaTime;
using NodaTime.Testing;

namespace GreenEnergyHub.Charges.TestCore.TestHelpers
{
    public static class ZonedDateTimeServiceHelper
    {
        public static ZonedDateTimeService GetZonedDateTimeService(Instant instant)
        {
            var clock = new FakeClock(instant);
            return new ZonedDateTimeService(clock, new Iso8601ConversionConfiguration(DateTimeZoneProviders.Tzdb.GetSystemDefault().Id));
        }

        public static ZonedDateTimeService GetZonedDateTimeService(
            Iso8601ConversionConfiguration iso8601ConversionConfiguration,
            Instant instant)
        {
            var clock = new FakeClock(instant);
            return new ZonedDateTimeService(clock, iso8601ConversionConfiguration);
        }
    }
}
