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

using GreenEnergyHub.Charges.Infrastructure.Core.Function;

namespace GreenEnergyHub.Charges.Infrastructure.Core.InternalMessaging
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

        // Integration events topic
        public const string IntegrationEventTopicKey = "integration_event_topic";

        // Integration, metering point domain
        public const string MeteringPointCreatedSubscriptionName = "metering-point-created";
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
        public const string MarketParticipantChangedSubscriptionName = "market-participant-changed";

        // Domain event
        [DomainEventSetting]
        public const string ChargesDomainEventsTopicKey = "sbt-charges-domain-events";

        // Domain event, charge, received
        [DomainEventSetting]
        public const string ChargeCommandReceivedSubscriptionName = "sbtsub-charges-charge-command-received";

        // Domain event, charge price, received
        [DomainEventSetting]
        public const string ChargePriceCommandReceivedSubscriptionName = "sbtsub-charges-charge-price-command-received";

        // Domain event, charge, accepted
        [DomainEventSetting]
        public const string ChargeAcceptedDataAvailableSubscriptionName = "sbtsub-charges-charge-accepted-dataavailable";
        [DomainEventSetting]
        public const string ChargeCommandAcceptedSubscriptionName = "sbtsub-charges-charge-command-accepted";
        [DomainEventSetting]
        public const string ChargeCommandAcceptedPublishSubscriptionName = "sbtsub-charges-charge-command-accepted-publish";

        // Domain event, charge, rejected
        [DomainEventSetting]
        public const string ChargeCommandRejectedSubscriptionName = "sbtsub-charges-charge-command-rejected";

        // Domain event, charge price, rejected
        [DomainEventSetting]
        public const string ChargePriceRejectedSubscriptionName = "sbtsub-charges-charge-price-rejected";

        // Domain event, charge price, confirmed
        [DomainEventSetting]
        public const string ChargePriceConfirmedSubscriptionName = "sbtsub-charges-charge-price-confirmed";
        [DomainEventSetting]
        public const string ChargePriceConfirmedDataAvailableSubscriptionName = "sbtsub-charges-charge-price-confirmed-dataavail";
        [DomainEventSetting]
        public const string ChargePriceConfirmedPublishSubscriptionName = "sbtsub-charges-charge-price-confirmed-publish";

        // Domain event, charge links, received
        [DomainEventSetting]
        public const string ChargeLinksCommandReceivedSubscriptionName = "sbtsub-charges-charge-links-command-received";

        // Domain event, charge links, rejected
        [DomainEventSetting]
        public const string ChargeLinksCommandRejectedSubscriptionName = "sbtsub-charges-charge-links-command-rejected";

        // Domain event, charge links, accepted
        [DomainEventSetting]
        public const string ChargeLinksAcceptedConfirmationSubscriptionName = "sbtsub-charges-charge-links-accepted-confirmation";
        [DomainEventSetting]
        public const string ChargeLinksAcceptedDataAvailableSubscriptionName = "sbtsub-charges-charge-links-accepted-dataavailable";
        [DomainEventSetting]
        public const string ChargeLinksAcceptedPublishSubscriptionName = "sbtsub-charges-charge-links-accepted-publish";
        [DomainEventSetting]
        public const string DefaultChargeLinksDataAvailableSubscriptionName = "sbtsub-charges-default-charge-links-dataavailable";
    }
}