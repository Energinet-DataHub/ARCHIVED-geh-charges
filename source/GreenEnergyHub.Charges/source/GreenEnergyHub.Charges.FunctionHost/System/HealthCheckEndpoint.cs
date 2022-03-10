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
using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace GreenEnergyHub.Charges.FunctionHost.System
{
    public class HealthCheckEndpoint
    {
        public HealthCheckEndpoint(HealthCheckService healthCheck)
        {
            HealthCheck = healthCheck;
        }

        private HealthCheckService HealthCheck { get; }

        [Function("HealthCheck")]
        public async Task<HttpResponseData> RunAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "monitor/{endpoint}")]
            HttpRequestData httpRequest,
            string endpoint)
        {
            Func<HealthCheckRegistration, bool>? predicate = null;
            if (string.Compare(endpoint, "live", ignoreCase: true) == 0)
            {
                predicate = r => r.Name.Contains("self");
            }

            if (string.Compare(endpoint, "ready", ignoreCase: true) == 0)
            {
                predicate = r => r.Tags.Contains("dependency");
            }

            var httpResponse = httpRequest.CreateResponse();

            if (predicate == null)
            {
                httpResponse.StatusCode = HttpStatusCode.NotFound;
            }
            else
            {
                var result = await HealthCheck.CheckHealthAsync(predicate).ConfigureAwait(false);

                httpResponse.StatusCode = result.Status == Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Healthy
                    ? HttpStatusCode.OK
                    : HttpStatusCode.ServiceUnavailable;

                var healthStatus = Enum.GetName(typeof(Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus), result.Status);
                await httpResponse.WriteStringAsync(healthStatus!).ConfigureAwait(false);
            }

            return httpResponse;
        }
    }
}
