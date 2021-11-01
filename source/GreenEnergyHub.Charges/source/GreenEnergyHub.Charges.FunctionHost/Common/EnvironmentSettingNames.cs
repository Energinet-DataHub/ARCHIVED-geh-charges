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
        public const string AzureWebJobsStorage = "AzureWebJobsStorage";

        public const string DomainEventSenderConnectionString = "DOMAINEVENT_SENDER_CONNECTION_STRING";

        public const string DomainEventListenerConnectionString = "DOMAINEVENT_LISTENER_CONNECTION_STRING";

        public const string DataHubSenderConnectionString = "INTEGRATIONEVENT_SENDER_CONNECTION_STRING";

        public const string DataHubListenerConnectionString = "INTEGRATIONEVENT_LISTENER_CONNECTION_STRING";

        public const string CreateLinkReplyQueueName = "CREATE_LINK_REPLY_QUEUE_NAME";

        public const string CreateLinkMessagesReplyQueueName = "CREATE_LINK_MESSAGES_REPLY_QUEUE_NAME";

        /// <summary>
        /// Deprecated: This is from an old implementation where we mimicked sending to the post office
        /// by sending to a topic we provisioned ourselves in the charges domain.
        /// </summary>
        public const string PostOfficeTopicName = "POST_OFFICE_TOPIC_NAME";

        public const string ChargeDbConnectionString = "CHARGE_DB_CONNECTION_STRING";

        public const string ChargeLinkAcceptedTopicName = "CHARGE_LINK_ACCEPTED_TOPIC_NAME";
    }
}
