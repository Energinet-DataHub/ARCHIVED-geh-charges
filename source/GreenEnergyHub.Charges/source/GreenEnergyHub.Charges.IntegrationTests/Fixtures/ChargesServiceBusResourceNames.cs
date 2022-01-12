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
        // Environment specific settings
        public const string MessageHubStorageConnectionString = "UseDevelopmentStorage=true";

        // Integration events, charges
        public const string ChargeCreatedTopicKey = "charge-created";
        public const string ChargeCreatedSubscriptionName = "charge-created-sub";
        public const string ChargePricesUpdatedTopicKey = "charge-prices-updated";
        public const string ChargePricesUpdatedSubscriptionName = "charge-prices-updated-sub";

        // Integration events, charge links
        public const string ChargeLinkCreatedTopicKey = "charge-link-created";

        // Integration, metering point domain
        public const string ConsumptionMeteringPointCreatedTopicKey = "consumption-metering-point-created";
        public const string ConsumptionMeteringPointCreatedSubscriptionName = "consumption-metering-point-created-sub-charges";
        public const string CreateLinkRequestQueueKey = "create-link-request";
        public const string CreateLinkReplyQueueKey = "create-link-reply";

        // Integration, message hub
        public const string MessageHubDataAvailableQueueKey = "dataavailable";
        public const string MessageHubRequestQueueKey = "charges";
        public const string MessageHubReplyQueueKey = "charges-reply";
        public const string MessageHubStorageContainerName = "postoffice-reply";

        // Internal, charge, received
        public const string CommandReceivedTopicKey = "command-received";
        public const string CommandReceivedSubscriptionName = "command-received";

        // Internal, charge, accepted
        public const string CommandAcceptedTopicKey = "command-accepted";
        public const string ChargeAcceptedSubDataAvailableNotifier = "chargeaccepted-sub-dataavailablenotifier";
        public const string CommandAcceptedSubscriptionName = "command-accepted";
        public const string CommandAcceptedReceiverSubscriptionName = "charge-command-accepted-receiver";

        // Internal, charge, rejected
        public const string CommandRejectedTopicKey = "command-rejected";
        public const string CommandRejectedSubscriptionName = "command-rejected";

        // Internal, charge links, received
        public const string ChargeLinkReceivedTopicKey = "link-command-received";
        public const string ChargeLinkReceivedSubscriptionName = "link-command-received-receiver";

        // Internal, charge links, rejected
        public const string ChargeLinksRejectedTopicKey = "link-command-rejected";
        public const string ChargeLinksRejectedSubscriptionName = "charge-links-command-rejected";

        // Internal, charge links, accepted
        public const string ChargeLinkAcceptedTopicKey = "link-command-accepted";
        public const string ChargeLinkAcceptedConfirmationNotifierSubscriptionName = "chargelinkaccepted-sub-confirmationnotifier";
        public const string ChargeLinkAcceptedDataAvailableNotifierSubscriptionName = "chargelinkaccepted-sub-dataavailablenotifier";
        public const string ChargeLinkAcceptedEventPublisherSubscriptionName = "chargelinkaccepted-sub-eventpublisher";
        public const string ChargeLinkAcceptedEventReplierSubscriptionName = "chargelinkaccepted-sub-replier";
        public const string DefaultChargeLinksDataAvailableNotifiedTopicKey = "default-charge-link-available";
        public const string DefaultChargeLinksDataAvailableNotifiedSubscriptionName = "default-charge-link-available-notified";
    }
}
