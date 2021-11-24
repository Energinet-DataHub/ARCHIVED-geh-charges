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

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus.Administration;
using GreenEnergyHub.Charges.FunctionHost.Common;
using GreenEnergyHub.Charges.FunctionHost.Configuration;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace GreenEnergyHub.Charges.FunctionHost.Health
{
    public class HealthStatus
    {
        /// <summary>
        /// HTTP GET endpoint that can be used to monitor the health of the function app.
        /// </summary>
        [Function(nameof(HealthStatus))]
        public async Task<HttpResponseData> RunAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)]
            [NotNull] HttpRequestData req,
            [NotNull] FunctionContext context)
        {
            var healthStatus = await GetDeepHealthCheckStatusAsync();

            var isHealthy = healthStatus.All(x => x.Value);
            var httpStatus = isHealthy ? HttpStatusCode.OK : HttpStatusCode.ServiceUnavailable;

            var response = req.CreateResponse();
            await response.WriteAsJsonAsync(healthStatus);
            response.StatusCode = httpStatus;
            return response;
        }

        private async Task<Dictionary<string, bool>> GetDeepHealthCheckStatusAsync()
        {
            var connectionString = EnvironmentHelper.GetEnv(EnvironmentSettingNames.DataHubManagerConnectionString);

            return new Dictionary<string, bool>
            {
                { "MessageHubDataAvailableQueueExists", await QueueExistsAsync(connectionString, EnvironmentSettingNames.MessageHubDataAvailableQueue) },
                { "MessageHubRequestQueueExists", await QueueExistsAsync(connectionString, EnvironmentSettingNames.MessageHubRequestQueue) },
                { "MessageHubResponseQueueExists", await QueueExistsAsync(connectionString, EnvironmentSettingNames.MessageHubReplyQueue) },
            };
        }

        private async Task<bool> QueueExistsAsync(string connectionString, string queueNameEnvVariable)
        {
            var client = new ServiceBusAdministrationClient(connectionString);
            var queueName = EnvironmentHelper.GetEnv(queueNameEnvVariable);
            return await client.QueueExistsAsync(queueName);
        }
    }
}
