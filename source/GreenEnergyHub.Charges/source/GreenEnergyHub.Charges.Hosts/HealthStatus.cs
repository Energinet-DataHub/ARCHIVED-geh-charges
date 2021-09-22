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

using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace GreenEnergyHub.Charges.Hosts
{
    public class HealthStatus
    {
        private readonly ILogger _log;

        public HealthStatus([NotNull] ILoggerFactory loggerFactory)
        {
            _log = loggerFactory.CreateLogger(nameof(HealthStatus));
        }

        /// <summary>
        /// HTTP GET endpoint that can be used to monitor the health of the function app.
        /// </summary>
        [Function(nameof(HealthStatus))]
        public Task<IActionResult> RunAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)]
            [NotNull] HttpRequestData req,
            [NotNull] FunctionContext context)
        {
            _log.LogInformation("Health Status API invoked");
            _log.LogDebug("Workaround for unused method arguments", req, context);

            /* Consider checking access to used Service Bus topics and other health checks */

            var status = new
            {
                FunctionAppIsAlive = true,
            };

            return Task.FromResult<IActionResult>(new JsonResult(status));
        }
    }
}
