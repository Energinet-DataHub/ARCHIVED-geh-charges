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

using System.Collections.Concurrent;
using Microsoft.ApplicationInsights.Channel;

namespace GreenEnergyHub.Charges.IntegrationTests.TestHelpers
{
#pragma warning disable CS8618
    public sealed class MockTelemetryChannel : ITelemetryChannel
    {
        private readonly ConcurrentBag<ITelemetry> _sentTelemtries = new();

        public bool IsFlushed { get; private set; }

        public bool? DeveloperMode { get; set; }

        public string EndpointAddress { get; set; }

        public void Send(ITelemetry item)
        {
            _sentTelemtries.Add(item);
        }

        public void Flush()
        {
            IsFlushed = true;
        }

        public void Dispose()
        {
        }
    }
}
