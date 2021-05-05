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
using System.IO;
using GreenEnergyHub.Charges.MessageReceiver;
using Microsoft.Extensions.Hosting;

namespace GreenEnergyHub.Charges.IntegrationTests.TestHelpers
{
    public static class TestConfigurationHelper
    {
        public static void ConfigureEnvironmentVariablesFromLocalSettings()
        {
            var path = Path.GetDirectoryName(typeof(ChargeHttpTrigger).Assembly.Location);
            var json = File.ReadAllText(Path.Join(path, "local.settings.json"));
            var parsed = Newtonsoft.Json.Linq.JObject.Parse(json).Value<Newtonsoft.Json.Linq.JObject>("Values");

            foreach (var item in parsed)
            {
                Environment.SetEnvironmentVariable(item.Key, item.Value.ToString());
            }
        }

        public static IHost SetupHost()
        {
            var startup = new Startup();
            return new HostBuilder().ConfigureWebJobs(startup.Configure).Build();
        }
    }
}
