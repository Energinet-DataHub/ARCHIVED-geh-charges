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

using System.Threading.Tasks;
using GreenEnergyHub.Charges.Application.ChargeLinks.Handlers;
using GreenEnergyHub.Charges.Domain.Dtos.DefaultChargeLinksDataAvailableNotifiedEvents;
using GreenEnergyHub.Charges.FunctionHost.Common;
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
        private readonly MessageExtractor<DefaultChargeLinksCreatedEvent> _messageExtractor;
        private readonly ICreateDefaultChargeLinksReplyHandler _createDefaultChargeLinksReplyHandler;

        public CreateDefaultChargeLinksReplierEndpoint(
            MessageExtractor<DefaultChargeLinksCreatedEvent> messageExtractor,
            ICreateDefaultChargeLinksReplyHandler createDefaultChargeLinksReplyHandler)
        {
            _messageExtractor = messageExtractor;
            _createDefaultChargeLinksReplyHandler = createDefaultChargeLinksReplyHandler;
        }

        [Function(FunctionName)]
        public async Task RunAsync(
            [ServiceBusTrigger(
                "%" + EnvironmentSettingNames.DefaultChargeLinksDataAvailableNotifiedTopicName + "%",
                "%" + EnvironmentSettingNames.DefaultChargeLinksDataAvailableNotifiedSubscription + "%",
                Connection = EnvironmentSettingNames.DomainEventListenerConnectionString)]
            byte[] message)
        {
            var defaultChargeLinksDataAvailableNotifierEvent = (DefaultChargeLinksCreatedEvent)await _messageExtractor.ExtractAsync(message).ConfigureAwait(false);

            await _createDefaultChargeLinksReplyHandler.HandleAsync(defaultChargeLinksDataAvailableNotifierEvent).ConfigureAwait(false);
        }
    }
}
