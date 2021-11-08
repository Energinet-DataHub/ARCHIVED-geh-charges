﻿// Copyright 2020 Energinet DataHub A/S
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
        public const string AppInsightsInstrumentationKey = "APPINSIGHTS_INSTRUMENTATIONKEY";
        public const string Currency = "CURRENCY";
        public const string LocalTimeZoneName = "LOCAL_TIMEZONENAME";
        public const string AzureWebJobsStorage = "AzureWebJobsStorage";

        public const string HubSenderId = "HUB_SENDER_ID";
        public const string HubSenderRoleIntEnumValue = "HUB_SENDER_ROLE_INT_ENUM_VALUE";

        public const string ChargeDbConnectionString = "CHARGE_DB_CONNECTION_STRING";
        public const string ChargeAcceptedSubDataAvailableNotifier = "CHARGEACCEPTED_SUB_DATAAVAILABLENOTIFIER";

        public const string DomainEventSenderConnectionString = "DOMAINEVENT_SENDER_CONNECTION_STRING";
        public const string DomainEventListenerConnectionString = "DOMAINEVENT_LISTENER_CONNECTION_STRING";

        public const string DataHubSenderConnectionString = "INTEGRATIONEVENT_SENDER_CONNECTION_STRING";
        public const string DataHubListenerConnectionString = "INTEGRATIONEVENT_LISTENER_CONNECTION_STRING";

        public const string DataHubManagerConnectionString = "INTEGRATIONEVENT_MANAGER_CONNECTION_STRING";

        public const string ChargeLinkAcceptedSubEventPublisher = "CHARGELINKACCEPTED_SUB_EVENTPUBLISHER";
        public const string ChargeLinkAcceptedSubDataAvailableNotifier = "CHARGELINKACCEPTED_SUB_DATAAVAILABLENOTIFIER";
        public const string ChargeLinkAcceptedReplier = "CHARGELINKACCEPTED_SUB_REPLIER";
        public const string CreateLinkReplyQueueName = "CREATE_LINK_REPLY_QUEUE_NAME";

        public const string CreateLinkMessagesReplyQueueName = "CREATE_LINK_MESSAGES_REPLY_QUEUE_NAME";

        /// <summary>
        /// Deprecated: This is from an old implementation where we mimicked sending to the post office
        /// by sending to a topic we provisioned ourselves in the charges domain.
        /// </summary>
        public const string PostOfficeTopicName = "POST_OFFICE_TOPIC_NAME";

        public const string ChargeLinkAcceptedTopicName = "CHARGE_LINK_ACCEPTED_TOPIC_NAME";
        public const string CommandReceivedTopicName = "COMMAND_RECEIVED_TOPIC_NAME";

        public const string MessageHubDataAvailableQueue = "MESSAGEHUB_DATAAVAILABLE_QUEUE";
        public const string MessageHubRequestQueue = "MESSAGEHUB_BUNDLEREQUEST_QUEUE";
        public const string MessageHubReplyQueue = "MESSAGEHUB_BUNDLEREPLY_QUEUE";
        public const string MessageHubStorageConnectionString = "MESSAGEHUB_STORAGE_CONNECTION_STRING";
        public const string MessageHubStorageContainer = "MESSAGEHUB_STORAGE_CONTAINER";

        public const string ChargeLinkCreatedTopicName = "CHARGE_LINK_CREATED_TOPIC_NAME";
        public const string CommandAcceptedReceiverSubscriptionName = "COMMAND_ACCEPTED_RECEIVER_SUBSCRIPTION_NAME";
        public const string CommandReceivedSubscriptionName = "COMMAND_RECEIVED_SUBSCRIPTION_NAME";

        public const string ChargeLinkReceivedTopicName = "CHARGE_LINK_RECEIVED_TOPIC_NAME";
        public const string ChargeLinkReceivedSubscriptionName = "CHARGE_LINK_RECEIVED_SUBSCRIPTION_NAME";

        public const string CommandAcceptedTopicName = "COMMAND_ACCEPTED_TOPIC_NAME";
        public const string CommandAcceptedSubscriptionName = "COMMAND_ACCEPTED_SUBSCRIPTION_NAME";

        public const string CommandRejectedTopicName = "COMMAND_REJECTED_TOPIC_NAME";
        public const string CommandRejectedSubscriptionName = "COMMAND_REJECTED_SUBSCRIPTION_NAME";

        public const string CreateLinkRequestQueueName = "CREATE_LINK_REQUEST_QUEUE_NAME";
        public const string CreateLinkMessagesRequestQueueName = "CREATE_LINK_MESSAGES_REQUEST_QUEUE_NAME";

        public const string ConsumptionMeteringPointCreatedTopicName = "CONSUMPTION_METERING_POINT_CREATED_TOPIC_NAME";
        public const string ConsumptionMeteringPointCreatdSubscriptionName = "CONSUMPTION_METERING_POINT_CREATED_SUBSCRIPTION_NAME";

        public const string ChargePricesUpdatedTopicName = "CHARGE_PRICES_UPDATED_TOPIC_NAME";
    }
}
