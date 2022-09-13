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

namespace GreenEnergyHub.Charges.IntegrationTest.Core.Fixtures.FunctionApp
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

        // Integration, marketparticipants domain
        public const string MarketParticipantChangedTopicKey = "market-participant-changed";
        public const string MarketParticipantChangedSubscriptionName = "market-participant-changed-to-charges";

        // Internal, charge
        public const string ChargesDomainEventsTopicKey = "sbt-charges-domain-events";

        // Internal, charge, received
        public const string ChargeCommandReceivedSubscriptionName = "sbtsub-charges-charge-command-received";

        // Internal, charge price, received
        public const string ChargePriceCommandReceivedSubscriptionName = "sbtsub-charges-charge-price-command-received";

        // Internal, charge, accepted
        public const string ChargeAcceptedDataAvailableSubscriptionName = "sbtsub-charges-charge-accepted-dataavailable";
        public const string ChargeCommandAcceptedSubscriptionName = "sbtsub-charges-charge-command-accepted";
        public const string ChargeCommandAcceptedPublishSubscriptionName = "sbtsub-charges-charge-command-accepted-publish";

        // Internal, charge, rejected
        public const string ChargeCommandRejectedSubscriptionName = "sbtsub-charges-charge-command-rejected";

        // Internal, charge price, rejected
        public const string ChargePriceRejectedSubscriptionName = "sbtsub-charges-charge-price-rejected";

        // Internal, charge price, confirmed
        public const string ChargePriceConfirmedSubscriptionName = "sbtsub-charges-charge-price-confirmed";
        public const string ChargePriceConfirmedDataAvailableSubscriptionName = "sbtsub-charges-charge-price-confirmed-dataavail";

        // Internal, charge links, received
        public const string ChargeLinksCommandReceivedSubscriptionName = "sbtsub-charges-charge-links-command-received";

        // Internal, charge links, rejected
        public const string ChargeLinksCommandRejectedSubscriptionName = "sbtsub-charges-charge-links-command-rejected";

        // Internal, charge links, accepted
        public const string ChargeLinksAcceptedConfirmationSubscriptionName = "sbtsub-charges-charge-links-accepted-confirmation";
        public const string ChargeLinksAcceptedDataAvailableSubscriptionName = "sbtsub-charges-charge-links-accepted-dataavailable";
        public const string ChargeLinksAcceptedPublishSubscriptionName = "sbtsub-charges-charge-links-accepted-publish";
        public const string DefaultChargeLinksDataAvailableSubscriptionName = "sbtsub-charges-default-charge-links-dataavailable";
    }
}
