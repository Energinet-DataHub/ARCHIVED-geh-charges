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
using GreenEnergyHub.Charges.Infrastructure.Core.Registration;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Data.SqlClient;

namespace GreenEnergyHub.Charges.FunctionHost.System
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
            var healthStatus = await GetDeepHealthCheckStatusAsync().ConfigureAwait(false);

            var isHealthy = healthStatus.All(x => x.Value);
            var httpStatus = isHealthy ? HttpStatusCode.OK : HttpStatusCode.ServiceUnavailable;

            var response = req.CreateResponse();
            await response.WriteAsJsonAsync(healthStatus).ConfigureAwait(false);
            response.StatusCode = httpStatus;
            return response;
        }

        private async Task<Dictionary<string, bool>> GetDeepHealthCheckStatusAsync()
        {
            var connectionString = EnvironmentHelper.GetEnv(EnvironmentSettingNames.DataHubManagerConnectionString);
            var chargesDbConnectionString = EnvironmentHelper.GetEnv(EnvironmentSettingNames.ChargeDbConnectionString);
            var actorRegisterDbConnectionString =
                EnvironmentHelper.GetEnv(EnvironmentSettingNames.ActorRegisterDbConnectionString);

            return new Dictionary<string, bool>
            {
                {
                    "ChargesDatabaseIsAvailable",
                    await IsDatabaseAvailableAsync(chargesDbConnectionString).ConfigureAwait(false)
                },
                {
                    "ActorRegisterDatabaseIsAvailable",
                    await IsDatabaseAvailableAsync(actorRegisterDbConnectionString).ConfigureAwait(false)
                },
                {
                    "MessageHubDataAvailableQueueExists",
                    await QueueExistsAsync(connectionString, EnvironmentSettingNames.MessageHubDataAvailableQueue)
                        .ConfigureAwait(false)
                },
                {
                    "MessageHubRequestQueueExists",
                    await QueueExistsAsync(connectionString, EnvironmentSettingNames.MessageHubRequestQueue)
                        .ConfigureAwait(false)
                },
                {
                    "MessageHubResponseQueueExists",
                    await QueueExistsAsync(connectionString, EnvironmentSettingNames.MessageHubReplyQueue)
                        .ConfigureAwait(false)
                },
            };
        }

        private async Task<bool> IsDatabaseAvailableAsync(string connectionString)
        {
            var connection = new SqlConnection(connectionString);
            try
            {
                await connection.OpenAsync().ConfigureAwait(false);
                await connection.CloseAsync().ConfigureAwait(false);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private async Task<bool> QueueExistsAsync(string connectionString, string queueNameEnvVariable)
        {
            var client = new ServiceBusAdministrationClient(connectionString);
            var queueName = EnvironmentHelper.GetEnv(queueNameEnvVariable);
            return await client.QueueExistsAsync(queueName).ConfigureAwait(false);
        }
    }
}
