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

using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Configuration;
using Squadron;

namespace GreenEnergyHub.Charges.IntegrationTests.TestHelpers
{
    public class ChargesAzureCloudServiceBusOptions : AzureCloudServiceBusOptions
    {
        public const string ReceivedTopicName = "sbt-received";
        public const string SubscriptionName = "sbs-received";
        private const string ServiceBusNamespaceConfiguration = "Squadron:Azure:ServiceBusNamespace";

        public override void Configure([NotNull] ServiceBusOptionsBuilder builder)
        {
            var config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();

            builder
                .Namespace(config[ServiceBusNamespaceConfiguration], true)
                .AddTopic(ReceivedTopicName)
                .AddSubscription(SubscriptionName);
        }
    }
}
