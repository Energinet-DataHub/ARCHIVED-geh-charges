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

        private static async Task<Dictionary<string, bool>> GetDeepHealthCheckStatusAsync()
        {
            var integrationConnectionString = EnvironmentHelper.GetEnv(EnvironmentSettingNames.DataHubManagerConnectionString);
            var domainConnectionString = EnvironmentHelper.GetEnv(EnvironmentSettingNames.DomainEventManagerConnectionString);

            var healthStatus = new Dictionary<string, bool>();

            await GetDatabaseConnectionStatusAsync(healthStatus).ConfigureAwait(false);

            await GetIntegrationEventsForChargesStatusAsync(integrationConnectionString, healthStatus).ConfigureAwait(false);
            await GetIntegrationEventsForChargeLinksStatusAsync(integrationConnectionString, healthStatus).ConfigureAwait(false);
            await GetIntegrationEventsMeteringPointDomainStatusAsync(integrationConnectionString, healthStatus).ConfigureAwait(false);
            await GetIntegrationEventsMessageHubStatusAsync(integrationConnectionString, healthStatus).ConfigureAwait(false);

            await GetInternalEventsForDefaultChargeLinksStatusAsync(domainConnectionString, healthStatus).ConfigureAwait(false);
            await GetInternalEventsForChargesStatusAsync(domainConnectionString, healthStatus).ConfigureAwait(false);
            await GetInternalEventsForChargeLinksStatusAsync(domainConnectionString, healthStatus).ConfigureAwait(false);

            return healthStatus;
        }

        private static async Task GetDatabaseConnectionStatusAsync(Dictionary<string, bool> healthStatus)
        {
            var chargesDbConnectionString = EnvironmentHelper.GetEnv(EnvironmentSettingNames.ChargeDbConnectionString);
            var marketParticipantRegistryDbConnectionString = EnvironmentHelper.GetEnv(EnvironmentSettingNames.MarketParticipantRegistryDbConnectionString);

            healthStatus.Add("ChargesDatabaseIsAvailable", await IsDatabaseAvailableAsync(chargesDbConnectionString).ConfigureAwait(false));
            healthStatus.Add("MarketParticipantRegistryDatabaseIsAvailable", await IsDatabaseAvailableAsync(marketParticipantRegistryDbConnectionString).ConfigureAwait(false));
        }

        private static async Task GetIntegrationEventsForChargesStatusAsync(string connectionString, Dictionary<string, bool> healthStatus)
        {
            healthStatus.Add(
                "ChargeCreatedTopicExists",
                await TopicExistsAsync(connectionString, EnvironmentSettingNames.ChargeCreatedTopicName).ConfigureAwait(false));

            healthStatus.Add(
                "ChargePricesUpdatedTopicExists",
                await TopicExistsAsync(connectionString, EnvironmentSettingNames.ChargePricesUpdatedTopicName).ConfigureAwait(false));
        }

        private static async Task GetIntegrationEventsForChargeLinksStatusAsync(string connectionString, Dictionary<string, bool> healthStatus)
        {
            healthStatus.Add(
                "ChargeLinksCreatedTopicExists",
                await TopicExistsAsync(connectionString, EnvironmentSettingNames.ChargeLinksCreatedTopicName).ConfigureAwait(false));
        }

        private static async Task GetIntegrationEventsMeteringPointDomainStatusAsync(string connectionString, Dictionary<string, bool> healthStatus)
        {
            healthStatus.Add(
                "MeteringPointCreatedTopicExists",
                await TopicExistsAsync(connectionString, EnvironmentSettingNames.MeteringPointCreatedTopicName).ConfigureAwait(false));

            healthStatus.Add(
                "MeteringPointCreatedSubscriptionExists",
                await SubscriptionExistsAsync(connectionString, EnvironmentSettingNames.MeteringPointCreatedSubscriptionName, EnvironmentSettingNames.MeteringPointCreatedTopicName).ConfigureAwait(false));

            healthStatus.Add(
                "CreateLinksRequestQueueExists",
                await QueueExistsAsync(connectionString, EnvironmentSettingNames.CreateLinksRequestQueueName).ConfigureAwait(false));
        }

        private static async Task GetIntegrationEventsMessageHubStatusAsync(string connectionString, Dictionary<string, bool> healthStatus)
        {
            healthStatus.Add(
                "MessageHubDataAvailableQueueExists",
                await QueueExistsAsync(connectionString, EnvironmentSettingNames.MessageHubDataAvailableQueue).ConfigureAwait(false));

            healthStatus.Add(
                "MessageHubRequestQueueExists",
                await QueueExistsAsync(connectionString, EnvironmentSettingNames.MessageHubRequestQueue).ConfigureAwait(false));

            healthStatus.Add(
                "MessageHubResponseQueueExists",
                await QueueExistsAsync(connectionString, EnvironmentSettingNames.MessageHubReplyQueue).ConfigureAwait(false));
        }

        private static async Task GetInternalEventsForDefaultChargeLinksStatusAsync(string connectionString, Dictionary<string, bool> healthStatus)
        {
            healthStatus.Add(
                "DefaultChargeLinksDataAvailableNotifiedTopicExists",
                await TopicExistsAsync(connectionString, EnvironmentSettingNames.DefaultChargeLinksDataAvailableNotifiedTopicName).ConfigureAwait(false));

            healthStatus.Add(
                "DefaultChargeLinksDataAvailableNotifiedSubscriptionExists",
                await SubscriptionExistsAsync(connectionString, EnvironmentSettingNames.DefaultChargeLinksDataAvailableNotifiedSubscription, EnvironmentSettingNames.DefaultChargeLinksDataAvailableNotifiedTopicName).ConfigureAwait(false));
        }

        private static async Task GetInternalEventsForChargesStatusAsync(string connectionString, Dictionary<string, bool> healthStatus)
        {
            healthStatus.Add(
                "ChargeCommandReceivedTopicExists",
                await TopicExistsAsync(connectionString, EnvironmentSettingNames.CommandReceivedTopicName).ConfigureAwait(false));

            healthStatus.Add(
                "ChargeCommandReceivedSubscriptionExists",
                await SubscriptionExistsAsync(connectionString, EnvironmentSettingNames.CommandReceivedSubscriptionName, EnvironmentSettingNames.CommandReceivedTopicName).ConfigureAwait(false));

            healthStatus.Add(
                "ChargeCommandRejectedTopicExists",
                await TopicExistsAsync(connectionString, EnvironmentSettingNames.CommandRejectedTopicName).ConfigureAwait(false));

            healthStatus.Add(
                "ChargeCommandRejectedSubscriptionExists",
                await SubscriptionExistsAsync(connectionString, EnvironmentSettingNames.CommandRejectedSubscriptionName, EnvironmentSettingNames.CommandRejectedTopicName).ConfigureAwait(false));

            healthStatus.Add(
                "ChargeCommandAcceptedTopicExists",
                await TopicExistsAsync(connectionString, EnvironmentSettingNames.CommandAcceptedTopicName).ConfigureAwait(false));

            healthStatus.Add(
                "ChargeCommandAcceptedSubscriptionExists",
                await SubscriptionExistsAsync(connectionString, EnvironmentSettingNames.CommandAcceptedSubscriptionName, EnvironmentSettingNames.CommandAcceptedTopicName).ConfigureAwait(false));

            // Used by ChargeDataAvailableNotifierEndpoint
            healthStatus.Add(
                "ChargeCommandDataAvailableNotifierSubscriptionExists",
                await SubscriptionExistsAsync(connectionString, EnvironmentSettingNames.ChargeAcceptedSubDataAvailableNotifier, EnvironmentSettingNames.CommandAcceptedTopicName).ConfigureAwait(false));

            // Used by ChargeIntegrationEventsPublisherEndpoint
            healthStatus.Add(
                "ChargeCommandAcceptedReceiverSubscriptionExists",
                await SubscriptionExistsAsync(connectionString, EnvironmentSettingNames.CommandAcceptedReceiverSubscriptionName, EnvironmentSettingNames.CommandAcceptedTopicName).ConfigureAwait(false));
        }

        private static async Task GetInternalEventsForChargeLinksStatusAsync(string connectionString, Dictionary<string, bool> healthStatus)
        {
            healthStatus.Add(
                "ChargeLinksAcceptedTopicExists",
                await TopicExistsAsync(connectionString, EnvironmentSettingNames.ChargeLinksAcceptedTopicName).ConfigureAwait(false));

            healthStatus.Add(
                "ChargeLinksAcceptedReplierSubscriptionExists",
                await SubscriptionExistsAsync(connectionString, EnvironmentSettingNames.ChargeLinksAcceptedReplier, EnvironmentSettingNames.ChargeLinksAcceptedTopicName).ConfigureAwait(false));

            healthStatus.Add(
                "ChargeLinksAcceptedEventPublisherSubscriptionExists",
                await SubscriptionExistsAsync(connectionString, EnvironmentSettingNames.ChargeLinksAcceptedSubEventPublisher, EnvironmentSettingNames.ChargeLinksAcceptedTopicName).ConfigureAwait(false));

            healthStatus.Add(
                "ChargeLinksAcceptedDataAvailableNotifierSubscriptionExists",
                await SubscriptionExistsAsync(connectionString, EnvironmentSettingNames.ChargeLinksAcceptedSubDataAvailableNotifier, EnvironmentSettingNames.ChargeLinksAcceptedTopicName).ConfigureAwait(false));

            healthStatus.Add(
                "ChargeLinksAcceptedConfirmationNotifierSubscriptionExists",
                await SubscriptionExistsAsync(connectionString, EnvironmentSettingNames.ChargeLinksAcceptedSubConfirmationNotifier, EnvironmentSettingNames.ChargeLinksAcceptedTopicName).ConfigureAwait(false));

            healthStatus.Add(
                "ChargeLinksRejectedTopicExists",
                await TopicExistsAsync(connectionString, EnvironmentSettingNames.ChargeLinksRejectedTopicName).ConfigureAwait(false));

            healthStatus.Add(
                "ChargeLinksRejectedSubscriptionExists",
                await SubscriptionExistsAsync(connectionString, EnvironmentSettingNames.ChargeLinksRejectedSubscriptionName, EnvironmentSettingNames.ChargeLinksRejectedTopicName).ConfigureAwait(false));

            healthStatus.Add(
                "ChargeLinksReceivedTopicExists",
                await TopicExistsAsync(connectionString, EnvironmentSettingNames.ChargeLinksReceivedTopicName).ConfigureAwait(false));

            healthStatus.Add(
                "ChargeLinksReceivedSubscriptionExists",
                await SubscriptionExistsAsync(connectionString, EnvironmentSettingNames.ChargeLinksReceivedSubscriptionName, EnvironmentSettingNames.ChargeLinksReceivedTopicName).ConfigureAwait(false));
        }

        private static async Task<bool> IsDatabaseAvailableAsync(string connectionString)
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

        private static async Task<bool> QueueExistsAsync(string connectionString, string queueNameEnvVariable)
        {
            var client = new ServiceBusAdministrationClient(connectionString);
            var queueName = EnvironmentHelper.GetEnv(queueNameEnvVariable);
            return await client.QueueExistsAsync(queueName).ConfigureAwait(false);
        }

        private static async Task<bool> TopicExistsAsync(string connectionString, string topicNameEnvVariable)
        {
            var client = new ServiceBusAdministrationClient(connectionString);
            var topicName = EnvironmentHelper.GetEnv(topicNameEnvVariable);
            return await client.TopicExistsAsync(topicName).ConfigureAwait(false);
        }

        private static async Task<bool> SubscriptionExistsAsync(string connectionString, string subscriptionNameEnvVariable, string topicNameEnvVariable)
        {
            var client = new ServiceBusAdministrationClient(connectionString);
            var subscriptionName = EnvironmentHelper.GetEnv(subscriptionNameEnvVariable);
            var topicName = EnvironmentHelper.GetEnv(topicNameEnvVariable);
            return await client.SubscriptionExistsAsync(topicName, subscriptionName).ConfigureAwait(false);
        }
    }
}
