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
        public const string PostOfficeTopicKey = "sbt-post-office";
        public const string PostOfficeSubscriptionName = "defaultSubscription";

        public const string ChargeLinkAcceptedTopicKey = "sbt-link-command-accepted";
        public const string ChargeLinkAcceptedDataAvailableNotifierSubscriptionName = "sbs-chargelinkaccepted-sub-dataavailablenotifier";
        public const string ChargeLinkAcceptedEventPublisherSubscriptionName = "sbs-chargelinkaccepted-sub-eventpublisher";
        public const string ChargeLinkAcceptedEventReplierSubscriptionName = "sbs-chargelinkaccepted-sub-replier";

        public const string ChargeLinkCreatedTopicKey = "charge-link-created";

        public const string ChargeLinkReceivedTopicKey = "sbt-link-command-received";
        public const string ChargeLinkReceivedSubscriptionName = "sbs-link-command-received-receiver";

        public const string CommandAcceptedTopicKey = "sbt-command-accepted";
        public const string CommandAcceptedSubscriptionName = "sbs-command-accepted";
        public const string CommandAcceptedReceiverSubscriptionName = "sbs-charge-command-accepted-receiver";
        public const string ChargeAcceptedSubDataAvailableNotifier = "sbs-chargeaccepted-sub-dataavailablenotifier";

        public const string CommandReceivedTopicKey = "sbt-command-received";
        public const string CommandReceivedSubscriptionName = "sbs-command-received";

        public const string CommandRejectedTopicKey = "sbt-command-rejected";
        public const string CommandRejectedSubscriptionName = "sbs-command-rejected";

        public const string CreateLinkRequestQueueKey = "create-link-request";

        public const string CreateLinkReplyQueueKey = "create-link-reply";

        public const string ConsumptionMeteringPointCreatedTopicKey = "consumption-metering-point-created";
        public const string ConsumptionMeteringPointCreatedSubscriptionName = "consumption-metering-point-created-sub-charges";

        public const string ChargeCreatedTopicKey = "charge-created";
        public const string ChargeCreatedSubscriptionName = "sbs-charge-created-sub";

        public const string ChargePricesUpdatedTopicKey = "charge-prices-updated";
        public const string ChargePricesUpdatedSubscriptionName = "sbs-charge-prices-updated";

        public const string MessageHubDataAvailableQueueKey = "message-hub-data-available";
        public const string MessageHubRequestQueueKey = "message-hub-request";
        public const string MessageHubReplyQueueKey = "message-hub-reply";
        public const string MessageHubStorageConnectionString = "UseDevelopmentStorage=true";
        public const string MessageHubStorageContainerName = "messagehub-bundles";
    }
}
