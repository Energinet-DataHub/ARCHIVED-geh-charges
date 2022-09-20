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

using GreenEnergyHub.Charges.Infrastructure.Core.Function;

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
        public const string DomainEventManagerConnectionString = "DOMAINEVENT_MANAGER_CONNECTION_STRING";
        public const string DomainEventSenderConnectionString = "DOMAINEVENT_SENDER_CONNECTION_STRING";
        public const string DataHubListenerConnectionString = "INTEGRATIONEVENT_LISTENER_CONNECTION_STRING";
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

        // Integration event shared topic
        public const string IntegrationEventTopicName = "INTEGRATION_EVENT_TOPIC_NAME";

        // Integration, metering point domain
        public const string MeteringPointCreatedSubscriptionName = "METERING_POINT_CREATED_SUBSCRIPTION_NAME";
        public const string CreateLinksRequestQueueName = "CREATE_LINKS_REQUEST_QUEUE_NAME";

        // Integration, message hub
        public const string MessageHubDataAvailableQueue = "MESSAGEHUB_DATAAVAILABLE_QUEUE";
        public const string MessageHubRequestQueue = "MESSAGEHUB_BUNDLEREQUEST_QUEUE";
        public const string MessageHubReplyQueue = "MESSAGEHUB_BUNDLEREPLY_QUEUE";
        public const string MessageHubStorageContainer = "MESSAGEHUB_STORAGE_CONTAINER";

        // Integration, request response logging
        public const string RequestResponseLoggingConnectionString = "REQUEST_RESPONSE_LOGGING_CONNECTION_STRING";
        public const string RequestResponseLoggingContainerName = "REQUEST_RESPONSE_LOGGING_CONTAINER_NAME";

        // Integration, marketparticipant/actor domain
        public const string MarketParticipantChangedSubscriptionName = "MARKET_PARTICIPANT_CHANGED_SUBSCRIPTION_NAME";

        // Domain event
        [DomainEventSetting]
        public const string ChargesDomainEventTopicName = "DOMAIN_EVENTS_TOPIC_NAME";

        // Domain event, charge information, received
        [DomainEventSetting]
        public const string ChargeInformationCommandReceivedSubscriptionName = "CHARGE_COMMAND_RECEIVED_SUBSCRIPTION_NAME";

        // Domain event, charge price, received
        [DomainEventSetting]
        public const string ChargePriceCommandReceivedSubscriptionName = "CHARGE_PRICE_COMMAND_RECEIVED_SUBSCRIPTION_NAME";

        // Domain event, charge information, accepted
        [DomainEventSetting]
        public const string ChargeAcceptedDataAvailableSubscriptionName = "CHARGE_ACCEPTED_DATAAVAILABLE_SUBSCRIPTION_NAME";
        [DomainEventSetting]
        public const string ChargeCommandAcceptedSubscriptionName = "CHARGE_COMMAND_ACCEPTED_SUBSCRIPTION_NAME";
        [DomainEventSetting]
        public const string ChargeCommandAcceptedPublishSubscriptionName = "CHARGE_COMMAND_ACCEPTED_PUBLISH_SUBSCRIPTION_NAME";

        // Domain event, charge information, rejected
        [DomainEventSetting]
        public const string ChargeCommandRejectedSubscriptionName = "CHARGE_COMMAND_REJECTED_SUBSCRIPTION_NAME";

        // Domain event, charge price, confirmed
        [DomainEventSetting]
        public const string ChargePriceOperationsConfirmedSubscriptionName = "CHARGE_PRICE_OPERATIONS_CONFIRMED_SUBSCRIPTION_NAME";
        [DomainEventSetting]
        public const string ChargePriceConfirmedDataAvailableSubscriptionName = "CHARGE_PRICE_CONFIRMED_DATAAVAILABLE_SUBSCRIPTION_NAME";
        [DomainEventSetting]
        public const string ChargePriceConfirmedPublishSubscriptionName = "CHARGE_PRICE_CONFIRMED_PUBLISH_SUBSCRIPTION_NAME";

        // Domain event, charge price, rejected
        [DomainEventSetting]
        public const string ChargePriceOperationsRejectedSubscriptionName = "CHARGE_PRICE_OPERATIONS_REJECTED_SUBSCRIPTION_NAME";

        // Domain event, charge links, received
        [DomainEventSetting]
        public const string ChargeLinksCommandReceivedSubscriptionName = "CHARGE_LINKS_COMMAND_RECEIVED_SUBSCRIPTION_NAME";

        // Domain event, charge links, accepted
        [DomainEventSetting]
        public const string ChargeLinksAcceptedConfirmationSubscriptionName = "CHARGE_LINKS_ACCEPTED_CONFIRMATION_SUBSCRIPTION_NAME";
        [DomainEventSetting]
        public const string ChargeLinksAcceptedDataAvailableSubscriptionName = "CHARGE_LINKS_ACCEPTED_DATAAVAILABLE_SUBSCRIPTION_NAME";
        [DomainEventSetting]
        public const string ChargeLinksAcceptedPublishSubscriptionName = "CHARGE_LINKS_ACCEPTED_PUBLISH_SUBSCRIPTION_NAME";
        [DomainEventSetting]
        public const string DefaultChargeLinksDataAvailableSubscriptionName = "DEFAULT_CHARGE_LINKS_DATAAVAILABLE_SUBSCRIPTION_NAME";

        // Domain event, charge links, rejected
        [DomainEventSetting]
        public const string ChargeLinksCommandRejectedSubscriptionName = "CHARGE_LINKS_COMMAND_REJECTED_SUBSCRIPTION_NAME";

        // JWT Token auth
        public const string B2CTenantId = "B2C_TENANT_ID";
        public const string BackendServiceAppId = "BACKEND_SERVICE_APP_ID";
    }
}
