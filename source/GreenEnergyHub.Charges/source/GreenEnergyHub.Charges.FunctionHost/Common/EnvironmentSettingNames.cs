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

namespace GreenEnergyHub.Charges.FunctionHost.Common
{
    /// <summary>
    /// Contains names of settings used by the function.
    /// </summary>
    public static class EnvironmentSettingNames
    {
        // Function
        public const string AzureWebJobsStorage = "AzureWebJobsStorage";

        // Environment specific settings
        public const string AppInsightsInstrumentationKey = "APPINSIGHTS_INSTRUMENTATIONKEY";
        public const string ChargeDbConnectionString = "CHARGE_DB_CONNECTION_STRING";
        public const string DomainEventListenerConnectionString = "DOMAINEVENT_LISTENER_CONNECTION_STRING";
        public const string DomainEventSenderConnectionString = "DOMAINEVENT_SENDER_CONNECTION_STRING";
        public const string DataHubListenerConnectionString = "INTEGRATIONEVENT_LISTENER_CONNECTION_STRING";
        public const string ActorRegisterDbConnectionString = "ACTOR_REGISTER_CONNECTION_STRING";
        public const string DataHubManagerConnectionString = "INTEGRATIONEVENT_MANAGER_CONNECTION_STRING";
        public const string DataHubSenderConnectionString = "INTEGRATIONEVENT_SENDER_CONNECTION_STRING";
        public const string MessageHubStorageConnectionString = "MESSAGEHUB_STORAGE_CONNECTION_STRING";

        // Localization
        public const string Currency = "CURRENCY";
        public const string LocalTimeZoneName = "LOCAL_TIMEZONENAME";

        // Integration events, charges
        public const string ChargeCreatedTopicName = "CHARGE_CREATED_TOPIC_NAME";
        public const string ChargePricesUpdatedTopicName = "CHARGE_PRICES_UPDATED_TOPIC_NAME";

        // Integration events, charge links
        public const string ChargeLinksCreatedTopicName = "CHARGE_LINKS_CREATED_TOPIC_NAME";

        // Integration, metering point domain
        public const string MeteringPointCreatedTopicName = "METERING_POINT_CREATED_TOPIC_NAME";
        public const string MeteringPointCreatedSubscriptionName = "METERING_POINT_CREATED_SUBSCRIPTION_NAME";
        public const string CreateLinksRequestQueueName = "CREATE_LINKS_REQUEST_QUEUE_NAME";
        public const string DefaultChargeLinksDataAvailableNotifiedTopicName =
            "DEFAULT_CHARGE_LINKS_DATA_AVAILABLE_NOTIFIED_TOPIC_NAME";

        public const string DefaultChargeLinksDataAvailableNotifiedSubscription =
            "DEFAULT_CHARGE_LINKS_DATA_AVAILABLE_NOTIFIED_SUBSCRIPTION_NAME";

        // Integration, message hub
        public const string MessageHubDataAvailableQueue = "MESSAGEHUB_DATAAVAILABLE_QUEUE";
        public const string MessageHubRequestQueue = "MESSAGEHUB_BUNDLEREQUEST_QUEUE";
        public const string MessageHubReplyQueue = "MESSAGEHUB_BUNDLEREPLY_QUEUE";
        public const string MessageHubStorageContainer = "MESSAGEHUB_STORAGE_CONTAINER";

        // Integration, request response logging
        public const string RequestResponseLoggingConnectionString = "REQUEST_RESPONSE_LOGGING_CONNECTION_STRING";
        public const string RequestResponseLoggingContainerName = "REQUEST_RESPONSE_LOGGING_CONTAINER_NAME";

        // Internal, charge, received
        public const string CommandReceivedTopicName = "COMMAND_RECEIVED_TOPIC_NAME";
        public const string CommandReceivedSubscriptionName = "COMMAND_RECEIVED_SUBSCRIPTION_NAME";

        // Internal, charge, accepted
        public const string CommandAcceptedTopicName = "COMMAND_ACCEPTED_TOPIC_NAME";
        public const string ChargeAcceptedSubDataAvailableNotifier = "CHARGEACCEPTED_SUB_DATAAVAILABLENOTIFIER";
        public const string CommandAcceptedSubscriptionName = "COMMAND_ACCEPTED_SUBSCRIPTION_NAME";
        public const string CommandAcceptedReceiverSubscriptionName = "COMMAND_ACCEPTED_RECEIVER_SUBSCRIPTION_NAME";

        // Internal, charge, rejected
        public const string CommandRejectedTopicName = "COMMAND_REJECTED_TOPIC_NAME";
        public const string CommandRejectedSubscriptionName = "COMMAND_REJECTED_SUBSCRIPTION_NAME";

        // Internal, charge links, received
        public const string ChargeLinksReceivedTopicName = "CHARGE_LINKS_RECEIVED_TOPIC_NAME";
        public const string ChargeLinksReceivedSubscriptionName = "CHARGE_LINKS_RECEIVED_SUBSCRIPTION_NAME";

        // Internal, charge links, accepted
        public const string ChargeLinksAcceptedTopicName = "CHARGE_LINKS_ACCEPTED_TOPIC_NAME";
        public const string ChargeLinksAcceptedSubConfirmationNotifier = "CHARGE_LINKS_ACCEPTED_SUB_CONFIRMATION_NOTIFIER";
        public const string ChargeLinksAcceptedSubDataAvailableNotifier = "CHARGE_LINKS_ACCEPTED_SUB_DATA_AVAILABLE_NOTIFIER";
        public const string ChargeLinksAcceptedSubEventPublisher = "CHARGE_LINKS_ACCEPTED_SUB_EVENT_PUBLISHER";
        public const string ChargeLinksAcceptedReplier = "CHARGE_LINKS_ACCEPTED_SUB_REPLIER";

        // Internal, charge links, rejected
        public const string ChargeLinksRejectedTopicName = "CHARGE_LINKS_REJECTED_TOPIC_NAME";
        public const string ChargeLinksRejectedSubscriptionName = "CHARGE_LINKS_REJECTED_SUBSCRIPTION_NAME";

        // JWT Token auth
        public const string B2CTenantId = "B2C_TENANT_ID";
        public const string BackendServiceAppId = "BACKEND_SERVICE_APP_ID";
    }
}
