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

using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using GreenEnergyHub.Charges.Application;
using GreenEnergyHub.Charges.Application.ChargeLinks.Handlers;
using GreenEnergyHub.Charges.Domain.ChargeLinkCommandAcceptedEvents;
using GreenEnergyHub.Charges.FunctionHost.Common;
using GreenEnergyHub.Charges.Infrastructure.Internal.ChargeLinkCommandAccepted;
using GreenEnergyHub.Charges.Infrastructure.Messaging;
using Microsoft.Azure.Functions.Worker;

namespace GreenEnergyHub.Charges.FunctionHost.ChargeLinks
{
    public class CreateDefaultChargeLinksReplierEndpoint
    {
        /// <summary>
        /// The name of the function.
        /// Function name affects the URL and thus possibly dependent infrastructure.
        /// </summary>
        public const string FunctionName = nameof(CreateDefaultChargeLinksReplierEndpoint);
        private readonly MessageExtractor<ChargeLinkCommandAccepted> _messageExtractor;
        private readonly IChargeLinkEventReplyHandler _chargeLinkEventReplyHandler;
        private readonly IMessageMetaDataContext _messageMetaDataContext;

        public CreateDefaultChargeLinksReplierEndpoint(
            MessageExtractor<ChargeLinkCommandAccepted> messageExtractor,
            IChargeLinkEventReplyHandler chargeLinkEventReplyHandler,
            IMessageMetaDataContext messageMetaDataContext)
        {
            _messageExtractor = messageExtractor;
            _chargeLinkEventReplyHandler = chargeLinkEventReplyHandler;
            _messageMetaDataContext = messageMetaDataContext;
        }

        [Function(FunctionName)]
        public async Task RunAsync(
            [ServiceBusTrigger(
                "%" + EnvironmentSettingNames.ChargeLinkAcceptedTopicName + "%",
                "%" + EnvironmentSettingNames.ChargeLinkAcceptedReplier + "%",
                Connection = EnvironmentSettingNames.DomainEventListenerConnectionString)]
            [NotNull] byte[] message)
        {
            var acceptedChargeLinkCommand = (ChargeLinkCommandAcceptedEvent)await _messageExtractor.ExtractAsync(message).ConfigureAwait(false);

            if (_messageMetaDataContext.IsReplyToSet())
                await _chargeLinkEventReplyHandler.HandleAsync(acceptedChargeLinkCommand).ConfigureAwait(false);
        }
    }
}
