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

using System;
using JetBrains.Annotations;
using Squadron;

namespace GreenEnergyHub.Charges.IntegrationTests.Application.ChangeOfCharges
{
    public class NewChargeServiceBus : AzureCloudServiceBusOptions
    {
        public static readonly string ReceivedTopicName = "RCV";
        public static readonly string SubscriptionName001 = "SUB-001";

        public override void Configure([NotNull] ServiceBusOptionsBuilder builder)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            builder
                .Namespace("sbn-charges-integration-test")
                .AddTopic(ReceivedTopicName)
                .AddSubscription(SubscriptionName001);
        }
    }
}
