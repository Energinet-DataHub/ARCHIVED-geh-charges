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
using System.Threading.Tasks;
using GreenEnergyHub.Charges.Application.ChargeLinks.Handlers;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeLinksAcceptedEvents;
using GreenEnergyHub.Charges.MessageHub.Application.MessageHub;
using GreenEnergyHub.Charges.MessageHub.Models.AvailableChargeLinksData;

// TODO BJARKE: Refactor so that data available notification publish event that we use for BRS-004 reply
namespace GreenEnergyHub.Charges.FunctionHost.ToDo
{
    public class ChargeLinkDataAvailableNotifierAndReplyHandler : IChargeLinkDataAvailableNotifierAndReplyHandler
    {
        private readonly IAvailableDataNotifier<AvailableChargeLinksData, ChargeLinksAcceptedEvent> _availableDataNotifer;
        private readonly IChargeLinkDataAvailableReplyHandler _chargeLinkDataAvailableReplyHandler;

        public ChargeLinkDataAvailableNotifierAndReplyHandler(
            IAvailableDataNotifier<AvailableChargeLinksData, ChargeLinksAcceptedEvent> availableDataNotifer,
            IChargeLinkDataAvailableReplyHandler chargeLinkDataAvailableReplyHandler)
        {
            _availableDataNotifer = availableDataNotifer;
            _chargeLinkDataAvailableReplyHandler = chargeLinkDataAvailableReplyHandler;
        }

        public async Task NotifyAndReplyAsync(ChargeLinksAcceptedEvent chargeLinksAcceptedEvent)
        {
            if (chargeLinksAcceptedEvent == null) throw new ArgumentNullException(nameof(chargeLinksAcceptedEvent));

            await _availableDataNotifer.NotifyAsync(chargeLinksAcceptedEvent);
            await _chargeLinkDataAvailableReplyHandler.ReplyAsync(chargeLinksAcceptedEvent);
        }
    }
}
