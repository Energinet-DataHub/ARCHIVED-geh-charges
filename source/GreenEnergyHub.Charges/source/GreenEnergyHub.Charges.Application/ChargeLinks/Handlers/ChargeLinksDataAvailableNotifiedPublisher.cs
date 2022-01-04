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
using GreenEnergyHub.Charges.Domain.Dtos.ChargeLinksAcceptedEvents;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeLinksDataAvailableNotifiedEvents;
using GreenEnergyHub.Charges.Infrastructure.Core.MessageMetaData;
using GreenEnergyHub.Charges.Infrastructure.Core.MessagingExtensions;

namespace GreenEnergyHub.Charges.Application.ChargeLinks.Handlers
{
    public class ChargeLinksDataAvailableNotifiedPublisher : IChargeLinksDataAvailableNotifiedPublisher
    {
        private readonly IMessageDispatcher<ChargeLinksDataAvailableNotifiedEvent> _messageDispatcher;
        private readonly IChargeLinksDataAvailableNotifiedEventFactory _chargeLinksDataAvailableNotifiedEventFactory;
        private readonly IMessageMetaDataContext _messageMetaDataContext;

        public ChargeLinksDataAvailableNotifiedPublisher(
            IMessageDispatcher<ChargeLinksDataAvailableNotifiedEvent> messageDispatcher,
            IChargeLinksDataAvailableNotifiedEventFactory chargeLinksDataAvailableNotifiedEventFactory,
            IMessageMetaDataContext messageMetaDataContext)
        {
            _messageDispatcher = messageDispatcher;
            _chargeLinksDataAvailableNotifiedEventFactory = chargeLinksDataAvailableNotifiedEventFactory;
            _messageMetaDataContext = messageMetaDataContext;
        }

        public async Task PublishAsync(ChargeLinksAcceptedEvent chargeLinksAcceptedEvent)
        {
            if (chargeLinksAcceptedEvent == null) throw new ArgumentNullException(nameof(chargeLinksAcceptedEvent));

            // TODO: Move if-clause to replier
            if (_messageMetaDataContext.IsReplyToSet())
            {
                // TODO: Event and factory should be named "ChargeLinksDataAvailableNotified"
                var defaultChargeLinksCreatedEvent =
                    _chargeLinksDataAvailableNotifiedEventFactory.Create(chargeLinksAcceptedEvent);
                await _messageDispatcher.DispatchAsync(defaultChargeLinksCreatedEvent);
            }
        }
    }
}
