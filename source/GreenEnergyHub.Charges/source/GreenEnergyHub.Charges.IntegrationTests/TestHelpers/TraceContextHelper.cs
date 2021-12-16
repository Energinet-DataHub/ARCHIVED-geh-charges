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

using System.Net.Http;

namespace GreenEnergyHub.Charges.IntegrationTests.TestHelpers
{
    public static class TraceContextHelper
    {
        public static void ConfigureTraceContext(this HttpRequestMessage request, string correlationId)
        {
            // See https://tsuyoshiushio.medium.com/correlation-with-activity-with-application-insights-3-w3c-tracecontext-d9fb143c0ce2
            var traceParent = CreateTraceParentHttpHeaderValue(correlationId);
            request.Headers.Add("traceparent", traceParent);
        }

        /// <summary>
        /// Creates a trace parent value that can be used to track correlation ID across HTTP requests.
        /// </summary>
        private static string CreateTraceParentHttpHeaderValue(string correlationId)
        {
            return $"00-{correlationId}-b7ad6b7169203331-01";
        }
    }
}
