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

using Microsoft.Extensions.Configuration;

namespace Azlogin
{
    public static class IConfigurationExtensions
    {
        /// <summary>
        /// This extension method first try to read the setting value as if it was stored in "local.setting.json";
        /// if the value is empty, it then try to retrieve it directly in the configuration container.
        ///
        /// NOTICE: Azure Function App settings in "local.settings.json" are stored in the section "Values".
        /// </summary>
        public static string GetValue(this IConfiguration configuration, string settingName)
        {
            var value = configuration[$"Values:{settingName}"];
            return string.IsNullOrEmpty(value)
                ? configuration[settingName]
                : value;
        }
    }
}
