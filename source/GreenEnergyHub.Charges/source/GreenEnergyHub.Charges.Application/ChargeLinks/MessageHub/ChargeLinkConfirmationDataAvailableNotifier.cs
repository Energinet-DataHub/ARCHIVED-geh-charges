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

using Energinet.DataHub.MessageHub.Client.DataAvailable;
using GreenEnergyHub.Charges.Application.MessageHub;
using GreenEnergyHub.Charges.Domain.AvailableChargeLinkReceiptData;
using GreenEnergyHub.Charges.Domain.AvailableData;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeLinksAcceptedEvents;
using GreenEnergyHub.Charges.Domain.MarketParticipants;

namespace GreenEnergyHub.Charges.Application.ChargeLinks.MessageHub
{
    public class ChargeLinkConfirmationDataAvailableNotifier
        : AvailableDataNotifier<AvailableChargeLinkReceiptData, ChargeLinksAcceptedEvent>
    {
        public ChargeLinkConfirmationDataAvailableNotifier(
            IAvailableDataFactory<AvailableChargeLinkReceiptData, ChargeLinksAcceptedEvent> availableDataFactory,
            IAvailableDataRepository<AvailableChargeLinkReceiptData> availableDataRepository,
            IAvailableDataNotificationFactory<AvailableChargeLinkReceiptData> availableDataNotificationFactory,
            IDataAvailableNotificationSender dataAvailableNotificationSender,
            ICorrelationContext correlationContext)
            : base(
                availableDataFactory,
                availableDataRepository,
                availableDataNotificationFactory,
                dataAvailableNotificationSender,
                correlationContext)
        {
        }

        protected override bool ShouldSendMessage(ChargeLinksAcceptedEvent input)
        {
            // We do not need to send to system operators
            return input.ChargeLinksCommand.Document.Sender.BusinessProcessRole !=
                   MarketParticipantRole.SystemOperator;
        }
    }
}
