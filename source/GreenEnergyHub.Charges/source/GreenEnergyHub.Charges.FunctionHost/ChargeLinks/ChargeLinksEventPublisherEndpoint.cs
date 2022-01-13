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
using GreenEnergyHub.Charges.Domain.Dtos.ChargeLinksAcceptedEvents;
using GreenEnergyHub.Charges.FunctionHost.Common;
using GreenEnergyHub.Charges.Infrastructure.Core.MessagingExtensions;
using GreenEnergyHub.Charges.Infrastructure.Internal.ChargeLinksCommandAccepted;
using Microsoft.Azure.Functions.Worker;

namespace GreenEnergyHub.Charges.FunctionHost.ChargeLinks
{
    public class ChargeLinksEventPublisherEndpoint
    {
        /// <summary>
        /// The name of the function.
        /// Function name affects the URL and thus possibly dependent infrastructure.
        /// </summary>
        public const string FunctionName = nameof(ChargeLinksEventPublisherEndpoint);
        private readonly MessageExtractor<ChargeLinksCommandAccepted> _messageExtractor;
        private readonly IChargeLinkEventPublishHandler _chargeLinkEventPublishHandler;

        public ChargeLinksEventPublisherEndpoint(
            MessageExtractor<ChargeLinksCommandAccepted> messageExtractor,
            IChargeLinkEventPublishHandler chargeLinkEventPublishHandler)
        {
            _messageExtractor = messageExtractor;
            _chargeLinkEventPublishHandler = chargeLinkEventPublishHandler;
        }

        [Function(FunctionName)]
        public async Task RunAsync(
            [ServiceBusTrigger(
                "%" + EnvironmentSettingNames.ChargeLinksAcceptedTopicName + "%",
                "%" + EnvironmentSettingNames.ChargeLinksAcceptedSubEventPublisher + "%",
                Connection = EnvironmentSettingNames.DomainEventListenerConnectionString)]
            byte[] message)
        {
            var acceptedChargeLinkCommand = (ChargeLinksAcceptedEvent)await _messageExtractor.ExtractAsync(message).ConfigureAwait(false);

            await _chargeLinkEventPublishHandler.HandleAsync(acceptedChargeLinkCommand).ConfigureAwait(false);
        }
    }
}
