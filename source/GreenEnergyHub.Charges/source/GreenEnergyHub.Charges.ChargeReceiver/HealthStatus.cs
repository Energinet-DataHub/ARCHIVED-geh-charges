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

using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace GreenEnergyHub.Charges.ChargeReceiver
{
    public static class HealthStatus
    {
        /// <summary>
        /// HTTP GET endpoint that can be used to monitor the health of the function app.
        /// </summary>
        [FunctionName(nameof(HealthStatus))]
        public static Task<IActionResult> RunAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("Health Status API invoked");
            log.LogDebug("Workaround for unused method argument", req);

            /* Consider checking access to used Service Bus topics and other health checks */

            var status = new
            {
                FunctionAppIsAlive = true,
            };

            return Task.FromResult<IActionResult>(new JsonResult(status));
        }
    }
}
