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
using Microsoft.Extensions.Configuration;
using Xunit;

namespace GreenEnergyHub.Charges.SystemTests.Fixtures
{
    /// <summary>
    /// Use this to mark system tests (facts).
    ///
    /// On developer machines we use the 'systemtest.local.settings.json' to set the 'SYSTEMFACT_SKIP' value.
    /// On hosted agents we must set it using an environment variable.
    /// </summary>
    public sealed class SystemFactAttribute : FactAttribute
    {
        private static readonly Lazy<bool> _shouldSkip = new Lazy<bool>(ShouldSkip);

        public SystemFactAttribute()
        {
            if (_shouldSkip.Value)
            {
                Skip = "System fact was configured to be skipped.";
            }
        }

        private static bool ShouldSkip()
        {
            var configuration = new SystemTestConfiguration();
            return configuration.Root.GetValue<bool>("SYSTEMFACT_SKIP", defaultValue: true);
        }
    }
}
