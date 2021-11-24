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

namespace GreenEnergyHub.Charges.IntegrationTests.Fixtures
{
    public static class ChargesServiceBusResourceNames
    {
        public const string PostOfficeTopicKey = "post-office";
        public const string PostOfficeSubscriptionName = "defaultSubscription";

        public const string ChargeLinkAcceptedTopicKey = "link-command-accepted";
        public const string ChargeLinkAcceptedDataAvailableNotifierSubscriptionName = "chargelinkaccepted-sub-dataavailablenotifier";
        public const string ChargeLinkAcceptedEventPublisherSubscriptionName = "chargelinkaccepted-sub-eventpublisher";
        public const string ChargeLinkAcceptedEventReplierSubscriptionName = "chargelinkaccepted-sub-replier";

        public const string ChargeLinkCreatedTopicKey = "charge-link-created";

        public const string ChargeLinkReceivedTopicKey = "link-command-received";
        public const string ChargeLinkReceivedSubscriptionName = "link-command-received-receiver";

        public const string CommandAcceptedTopicKey = "command-accepted";
        public const string CommandAcceptedSubscriptionName = "command-accepted";
        public const string CommandAcceptedReceiverSubscriptionName = "charge-command-accepted-receiver";
        public const string ChargeAcceptedSubDataAvailableNotifier = "chargeaccepted-sub-dataavailablenotifier";

        public const string CommandReceivedTopicKey = "command-received";
        public const string CommandReceivedSubscriptionName = "command-received";

        public const string CommandRejectedTopicKey = "command-rejected";
        public const string CommandRejectedSubscriptionName = "command-rejected";

        public const string CreateLinkRequestQueueKey = "create-link-request";
        public const string CreateLinkReplyQueueKey = "create-link-reply";
        public const string CreateLinkMessagesRequestQueueKey = "create-link-messages-request";

        public const string ConsumptionMeteringPointCreatedTopicKey = "consumption-metering-point-created";
        public const string ConsumptionMeteringPointCreatedSubscriptionName = "consumption-metering-point-created-sub-charges";

        public const string ChargeCreatedTopicKey = "charge-created";
        public const string ChargeCreatedSubscriptionName = "charge-created-sub";

        public const string ChargePricesUpdatedTopicKey = "charge-prices-updated";
        public const string ChargePricesUpdatedSubscriptionName = "charge-prices-updated";

        public const string MessageHubDataAvailableQueueKey = "message-hub-data-available";
        public const string MessageHubRequestQueueKey = "message-hub-request";
        public const string MessageHubReplyQueueKey = "message-hub-reply";
        public const string MessageHubStorageConnectionString = "UseDevelopmentStorage=true";
        public const string MessageHubStorageContainerName = "messagehub-bundles";
    }
}
