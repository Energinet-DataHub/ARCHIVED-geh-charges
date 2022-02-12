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

namespace GreenEnergyHub.Charges.IntegrationTests.Fixtures.FunctionApp
{
    public static class ChargesServiceBusResourceNames
    {
        // Environment specific settings
        public const string MessageHubStorageConnectionString = "UseDevelopmentStorage=true";
        public const string RequestResponseLoggingConnectionString = "UseDevelopmentStorage=true";

        // Integration events, charges
        public const string ChargeCreatedTopicKey = "charge-created";
        public const string ChargeCreatedSubscriptionName = "charge-created-sub";
        public const string ChargePricesUpdatedTopicKey = "charge-prices-updated";
        public const string ChargePricesUpdatedSubscriptionName = "charge-prices-updated-sub";

        // Integration events, charge links
        public const string ChargeLinksCreatedTopicKey = "charge-links-created";

        // Integration, metering point domain
        public const string MeteringPointCreatedTopicKey = "metering-point-created";
        public const string MeteringPointCreatedSubscriptionName = "metering-point-created-sub-charges";
        public const string CreateLinksRequestQueueKey = "create-links-request";
        public const string CreateLinksReplyQueueKey = "create-links-reply";

        // Integration, message hub
        public const string MessageHubDataAvailableQueueKey = "dataavailable";
        public const string MessageHubRequestQueueKey = "charges";
        public const string MessageHubReplyQueueKey = "charges-reply";
        public const string MessageHubStorageContainerName = "postoffice-reply";

        // Integration, request response logging
        public const string RequestResponseLoggingContainerName = "marketoplogs";

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
        public const string ChargeLinksReceivedTopicKey = "links-command-received";
        public const string ChargeLinksReceivedSubscriptionName = "links-command-received-receiver";

        // Internal, charge links, rejected
        public const string ChargeLinksRejectedTopicKey = "links-command-rejected";
        public const string ChargeLinksRejectedSubscriptionName = "links-command-rejected";

        // Internal, charge links, accepted
        public const string ChargeLinksAcceptedTopicKey = "links-command-accepted";
        public const string ChargeLinksAcceptedConfirmationNotifierSubscriptionName = "charge-links-accepted-sub-confirmation-notifier";
        public const string ChargeLinksAcceptedDataAvailableNotifierSubscriptionName = "charge-links-accepted-sub-data-available-notifier";
        public const string ChargeLinksAcceptedEventPublisherSubscriptionName = "charge-links-accepted-sub-event-publisher";
        public const string ChargeLinksAcceptedEventReplierSubscriptionName = "charge-links-accepted-sub-replier";
        public const string DefaultChargeLinksDataAvailableNotifiedTopicKey = "default-charge-links-available";
        public const string DefaultChargeLinksDataAvailableNotifiedSubscriptionName = "default-charge-links-available-notified";
    }
}
