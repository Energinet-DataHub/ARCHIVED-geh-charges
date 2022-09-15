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

using System.Threading.Tasks;
using GreenEnergyHub.Charges.Application.ChargeLinks.Handlers;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeLinksDataAvailableNotifiedEvents;
using GreenEnergyHub.Charges.FunctionHost.Common;
using GreenEnergyHub.Charges.Infrastructure.Core.MessagingExtensions.Serialization;
using Microsoft.Azure.Functions.Worker;

namespace GreenEnergyHub.Charges.FunctionHost.ChargeLinks
{
    public class CreateDefaultChargeLinksReplierEndpoint
    {
        /// <summary>
        /// The name of the function.
        /// Function name affects the URL and thus possibly dependent infrastructure.
        /// </summary>
        private const string FunctionName = nameof(CreateDefaultChargeLinksReplierEndpoint);
        private readonly JsonMessageDeserializer<ChargeLinksDataAvailableNotifiedEvent> _deserializer;
        private readonly ICreateDefaultChargeLinksReplyHandler _createDefaultChargeLinksReplyHandler;

        public CreateDefaultChargeLinksReplierEndpoint(
            JsonMessageDeserializer<ChargeLinksDataAvailableNotifiedEvent> deserializer,
            ICreateDefaultChargeLinksReplyHandler createDefaultChargeLinksReplyHandler)
        {
            _deserializer = deserializer;
            _createDefaultChargeLinksReplyHandler = createDefaultChargeLinksReplyHandler;
        }

        [Function(FunctionName)]
        public async Task RunAsync(
            [ServiceBusTrigger(
                "%" + EnvironmentSettingNames.ChargesDomainEventTopicName + "%",
                "%" + EnvironmentSettingNames.DefaultChargeLinksDataAvailableSubscriptionName + "%",
                Connection = EnvironmentSettingNames.DomainEventListenerConnectionString)]
            byte[] message)
        {
            var notifierEvent = (ChargeLinksDataAvailableNotifiedEvent)await _deserializer.FromBytesAsync(message).ConfigureAwait(false);
            await _createDefaultChargeLinksReplyHandler.HandleAsync(notifierEvent).ConfigureAwait(false);
        }
    }
}
