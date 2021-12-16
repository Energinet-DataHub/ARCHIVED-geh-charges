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
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Reflection;
using NodaTime;

namespace GreenEnergyHub.Charges.IntegrationTests.TestHelpers
{
    public static class EmbeddedResourceHelper
    {
        public static string GetEmbeddedFile(string filePath, [NotNull] IClock clock)
        {
            var basePath = Assembly.GetExecutingAssembly().Location;
            var path = Path.Combine(Directory.GetParent(basePath)!.FullName, filePath);
            var fileText = File.ReadAllText(path);
            return ReplaceMergeFields(clock, fileText);
        }

        private static string ReplaceMergeFields(IClock clock, string file)
        {
            var currentInstant = clock.GetCurrentInstant();
            var now = currentInstant.ToString();

            // cim:timeInterval does not allow seconds.
            var inThirtyoneDays = currentInstant.Plus(Duration.FromDays(31))
                .ToString("yyyy-MM-dd\\THH:mm\\Z", CultureInfo.InvariantCulture);

            return file
                .Replace("{{$randomCharacters}}", Guid.NewGuid().ToString("n")[..10])
                .Replace("{{$isoTimestamp}}", now)
                .Replace("{{$isoTimestampPlusOneMonth}}", inThirtyoneDays);
        }
    }
}
