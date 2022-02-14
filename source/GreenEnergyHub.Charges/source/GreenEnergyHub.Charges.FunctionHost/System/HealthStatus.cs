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

        private static async Task<Dictionary<string, bool>> GetDeepHealthCheckStatusAsync()
        {
            var chargesDbConnectionString = EnvironmentHelper.GetEnv(EnvironmentSettingNames.ChargeDbConnectionString);

            var integrationConnectionString = EnvironmentHelper.GetEnv(EnvironmentSettingNames.DataHubManagerConnectionString);
            var domainConnectionString = EnvironmentHelper.GetEnv(EnvironmentSettingNames.DomainEventManagerConnectionString);

            var marketParticipantRegistryDbConnectionString =
                EnvironmentHelper.GetEnv(EnvironmentSettingNames.MarketParticipantRegistryDbConnectionString);

            return new Dictionary<string, bool>
            {
                // Database connections
                {
                    "ChargesDatabaseIsAvailable",
                    await IsDatabaseAvailableAsync(chargesDbConnectionString).ConfigureAwait(false)
                },
                {
                    "MarketParticipantRegistryDatabaseIsAvailable",
                    await IsDatabaseAvailableAsync(marketParticipantRegistryDbConnectionString).ConfigureAwait(false)
                },

                // Integration events, charges
                {
                    "ChargeCreatedTopicExists", await TopicExistsAsync(integrationConnectionString, EnvironmentSettingNames.ChargeCreatedTopicName)
                    .ConfigureAwait(false)
                },
                {
                    "ChargePricesUpdatedTopicExists", await TopicExistsAsync(integrationConnectionString, EnvironmentSettingNames.ChargePricesUpdatedTopicName)
                    .ConfigureAwait(false)
                },

                // Integration events, charge links
                {
                    "ChargeLinksCreatedTopicExists", await TopicExistsAsync(integrationConnectionString, EnvironmentSettingNames.ChargeLinksCreatedTopicName)
                    .ConfigureAwait(false)
                },

                // Integration events, metering point domain
                {
                    "MeteringPointCreatedTopicExists", await TopicExistsAsync(integrationConnectionString, EnvironmentSettingNames.MeteringPointCreatedTopicName)
                    .ConfigureAwait(false)
                },
                {
                    "MeteringPointCreatedSubscriptionExists",
                    await SubscriptionExistsAsync(
                        integrationConnectionString,
                        EnvironmentSettingNames.MeteringPointCreatedSubscriptionName,
                        EnvironmentSettingNames.MeteringPointCreatedTopicName)
                        .ConfigureAwait(false)
                },
                {
                    "CreateLinksRequestQueueExists", await QueueExistsAsync(integrationConnectionString, EnvironmentSettingNames.CreateLinksRequestQueueName)
                    .ConfigureAwait(false)
                },

                // Integration events, MessageHub
                {
                    "MessageHubDataAvailableQueueExists", await QueueExistsAsync(integrationConnectionString, EnvironmentSettingNames.MessageHubDataAvailableQueue)
                    .ConfigureAwait(false)
                },
                {
                    "MessageHubRequestQueueExists", await QueueExistsAsync(integrationConnectionString, EnvironmentSettingNames.MessageHubRequestQueue)
                    .ConfigureAwait(false)
                },
                {
                    "MessageHubResponseQueueExists", await QueueExistsAsync(integrationConnectionString, EnvironmentSettingNames.MessageHubReplyQueue)
                    .ConfigureAwait(false)
                },

                // Internal event, create default charge links
                {
                    "DefaultChargeLinksDataAvailableNotifiedTopicExists",
                    await TopicExistsAsync(
                        domainConnectionString,
                        EnvironmentSettingNames.DefaultChargeLinksDataAvailableNotifiedTopicName)
                        .ConfigureAwait(false)
                },
                {
                    "DefaultChargeLinksDataAvailableNotifiedSubscriptionExists",
                    await SubscriptionExistsAsync(
                        domainConnectionString,
                        EnvironmentSettingNames.DefaultChargeLinksDataAvailableNotifiedSubscription,
                        EnvironmentSettingNames.DefaultChargeLinksDataAvailableNotifiedTopicName).ConfigureAwait(false)
                },

                // Internal events, charge links
                {
                    "ChargeLinksAcceptedTopicExists", await TopicExistsAsync(domainConnectionString, EnvironmentSettingNames.ChargeLinksAcceptedTopicName)
                    .ConfigureAwait(false)
                },
                {
                    "ChargeLinksAcceptedReplierSubscriptionExists",
                    await SubscriptionExistsAsync(
                        domainConnectionString,
                        EnvironmentSettingNames.ChargeLinksAcceptedReplier,
                        EnvironmentSettingNames.ChargeLinksAcceptedTopicName)
                        .ConfigureAwait(false)
                },
                {
                    "ChargeLinksAcceptedEventPublisherSubscriptionExists",
                    await SubscriptionExistsAsync(
                        domainConnectionString,
                        EnvironmentSettingNames.ChargeLinksAcceptedSubEventPublisher,
                        EnvironmentSettingNames.ChargeLinksAcceptedTopicName)
                        .ConfigureAwait(false)
                },
                {
                    "ChargeLinksAcceptedDataAvailableNotifierSubscriptionExists",
                    await SubscriptionExistsAsync(
                        domainConnectionString,
                        EnvironmentSettingNames.ChargeLinksAcceptedSubDataAvailableNotifier,
                        EnvironmentSettingNames.ChargeLinksAcceptedTopicName)
                        .ConfigureAwait(false)
                },
                {
                    "ChargeLinksAcceptedConfirmationNotifierSubscriptionExists",
                    await SubscriptionExistsAsync(
                        domainConnectionString,
                        EnvironmentSettingNames.ChargeLinksAcceptedSubConfirmationNotifier,
                        EnvironmentSettingNames.ChargeLinksAcceptedTopicName)
                        .ConfigureAwait(false)
                },
                {
                    "ChargeLinksRejectedTopicExists", await TopicExistsAsync(domainConnectionString, EnvironmentSettingNames.ChargeLinksRejectedTopicName)
                    .ConfigureAwait(false)
                },
                {
                    "ChargeLinksRejectedSubscriptionExists",
                    await SubscriptionExistsAsync(
                        domainConnectionString,
                        EnvironmentSettingNames.ChargeLinksRejectedSubscriptionName,
                        EnvironmentSettingNames.ChargeLinksRejectedTopicName)
                        .ConfigureAwait(false)
                },
                {
                    "ChargeLinksReceivedTopicExists", await TopicExistsAsync(domainConnectionString, EnvironmentSettingNames.ChargeLinksReceivedTopicName)
                    .ConfigureAwait(false)
                },
                {
                    "ChargeLinksReceivedSubscriptionExists",
                    await SubscriptionExistsAsync(
                        domainConnectionString,
                        EnvironmentSettingNames.ChargeLinksReceivedSubscriptionName,
                        EnvironmentSettingNames.ChargeLinksReceivedTopicName)
                        .ConfigureAwait(false)
                },

                // Internal events, charges
                {
                    "ChargeCommandReceivedTopicExists", await TopicExistsAsync(domainConnectionString, EnvironmentSettingNames.CommandReceivedTopicName)
                    .ConfigureAwait(false)
                },
                {
                    "ChargeCommandReceivedSubscriptionExists",
                    await SubscriptionExistsAsync(
                        domainConnectionString,
                        EnvironmentSettingNames.CommandReceivedSubscriptionName,
                        EnvironmentSettingNames.CommandReceivedTopicName)
                        .ConfigureAwait(false)
                },
                {
                    "ChargeCommandRejectedTopicExists", await TopicExistsAsync(domainConnectionString, EnvironmentSettingNames.CommandRejectedTopicName)
                    .ConfigureAwait(false)
                },
                {
                    "ChargeCommandRejectedSubscriptionExists",
                    await SubscriptionExistsAsync(
                        domainConnectionString,
                        EnvironmentSettingNames.CommandRejectedSubscriptionName,
                        EnvironmentSettingNames.CommandRejectedTopicName)
                        .ConfigureAwait(false)
                },
                {
                    "ChargeCommandAcceptedTopicExists", await TopicExistsAsync(domainConnectionString, EnvironmentSettingNames.CommandAcceptedTopicName)
                    .ConfigureAwait(false)
                },
                {
                    "ChargeCommandAcceptedSubscriptionExists",
                    await SubscriptionExistsAsync(
                        domainConnectionString,
                        EnvironmentSettingNames.CommandAcceptedSubscriptionName,
                        EnvironmentSettingNames.CommandAcceptedTopicName)
                        .ConfigureAwait(false)
                },
                {
                    // Used by ChargeDataAvailableNotifierEndpoint
                    "ChargeCommandDataAvailableNotifierSubscriptionExists",
                    await SubscriptionExistsAsync(
                        domainConnectionString,
                        EnvironmentSettingNames.ChargeAcceptedSubDataAvailableNotifier,
                        EnvironmentSettingNames.CommandAcceptedTopicName)
                        .ConfigureAwait(false)
                },
                {
                    // Used by ChargeIntegrationEventsPublisherEndpoint
                    "ChargeCommandAcceptedReceiverSubscriptionExists",
                    await SubscriptionExistsAsync(
                        domainConnectionString,
                        EnvironmentSettingNames.CommandAcceptedReceiverSubscriptionName,
                        EnvironmentSettingNames.CommandAcceptedTopicName)
                        .ConfigureAwait(false)
                },
            };
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
