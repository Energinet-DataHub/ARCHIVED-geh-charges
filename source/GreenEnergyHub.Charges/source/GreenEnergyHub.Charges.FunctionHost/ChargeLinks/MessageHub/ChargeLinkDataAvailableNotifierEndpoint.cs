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
using GreenEnergyHub.Charges.Application.ChargeLinks.MessageHub;
using GreenEnergyHub.Charges.Domain.ChargeLinkCommandAcceptedEvents;
using GreenEnergyHub.Charges.FunctionHost.Common;
using GreenEnergyHub.Charges.Infrastructure.Internal.ChargeLinkCommandAccepted;
using GreenEnergyHub.Charges.Infrastructure.Messaging;
using Microsoft.Azure.Functions.Worker;

namespace GreenEnergyHub.Charges.FunctionHost.ChargeLinks.MessageHub
{
    /// <summary>
    /// The function will initiate the communication with the post office
    /// by notifying that a charge link has been created.
    /// This is the RSM-031 CIM XML 'NotifyBillingMasterData'.
    /// </summary>
    public class ChargeLinkDataAvailableNotifierEndpoint
    {
        private const string FunctionName = nameof(ChargeLinkDataAvailableNotifierEndpoint);
        private readonly MessageExtractor<ChargeLinkCommandAccepted> _messageExtractor;
        private readonly IChargeLinkDataAvailableNotifier _chargeLinkDataAvailableNotifier;
        private readonly IMessageMetaDataContext _messageMetaDataContext;
        private readonly IChargeLinkDataAvailableNotifierEndpointHandler _chargeLinkDataAvailableNotifierEndpointHandler;

        public ChargeLinkDataAvailableNotifierEndpoint(
            MessageExtractor<ChargeLinkCommandAccepted> messageExtractor,
            IChargeLinkDataAvailableNotifier chargeLinkDataAvailableNotifier,
            IMessageMetaDataContext messageMetaDataContext,
            IChargeLinkDataAvailableNotifierEndpointHandler chargeLinkDataAvailableNotifierEndpointHandler)
        {
            _messageExtractor = messageExtractor;
            _chargeLinkDataAvailableNotifier = chargeLinkDataAvailableNotifier;
            _messageMetaDataContext = messageMetaDataContext;
            _chargeLinkDataAvailableNotifierEndpointHandler = chargeLinkDataAvailableNotifierEndpointHandler;
        }

        [Function(FunctionName)]
        public async Task RunAsync(
            [ServiceBusTrigger(
                "%" + EnvironmentSettingNames.ChargeLinkAcceptedTopicName + "%",
                "%" + EnvironmentSettingNames.ChargeLinkAcceptedSubDataAvailableNotifier + "%",
                Connection = EnvironmentSettingNames.DomainEventListenerConnectionString)]
            [NotNull] byte[] message)
        {
            var chargeLinkCommandAcceptedEvent =
                (ChargeLinkCommandAcceptedEvent)await _messageExtractor.ExtractAsync(message).ConfigureAwait(false);

            await _chargeLinkDataAvailableNotifier.NotifyAsync(chargeLinkCommandAcceptedEvent).ConfigureAwait(false);

            if (_messageMetaDataContext.IsReplyToSet())
            {
                await _chargeLinkDataAvailableNotifierEndpointHandler
                    .HandleAsync(chargeLinkCommandAcceptedEvent)
                    .ConfigureAwait(false);
            }
        }
    }
}
