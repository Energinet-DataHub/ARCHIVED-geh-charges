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

namespace GreenEnergyHub.Iso8601
{
    /// <summary>
    /// Interface for time conversion configuration to use within the ISO8601 handling
    /// </summary>
    public interface IIso8601ConversionConfiguration
    {
        /// <summary>
        /// Retrieves the name of the time zone to use in conversions (see https://en.wikipedia.org/wiki/List_of_tz_database_time_zones for list of possibilities)
        /// </summary>
        /// <returns>The name of the time zone to use when converting time, as named by the tz database standard</returns>
        string GetTZDatabaseName();
    }
}
