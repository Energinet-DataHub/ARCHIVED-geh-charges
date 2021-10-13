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
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Energinet.DataHub.MessageHub.Client.DataAvailable;
using Energinet.DataHub.MessageHub.Client.Model;
using GreenEnergyHub.Charges.Domain.ChargeLinkCommandAcceptedEvents;
using GreenEnergyHub.Charges.Domain.Charges;

namespace GreenEnergyHub.Charges.Application.ChargeLinks.MessageHub
{
    public class ChargeLinkCreatedDataAvailableNotifier : IChargeLinkCreatedDataAvailableNotifier
    {
        /// <summary>
        /// The upper anticipated weight (kilobytes) contribution to the final bundle from the charge link created event.
        /// </summary>
        private const int MessageWeight = 2;

        private readonly IDataAvailableNotificationSender _dataAvailableNotificationSender;
        private readonly IChargeRepository _chargeRepository;

        public ChargeLinkCreatedDataAvailableNotifier(IDataAvailableNotificationSender dataAvailableNotificationSender, IChargeRepository chargeRepository)
        {
            _dataAvailableNotificationSender = dataAvailableNotificationSender;
            _chargeRepository = chargeRepository;
        }

        public async Task NotifyAsync([NotNull] ChargeLinkCommandAcceptedEvent chargeLinkCommandAcceptedEvent)
        {
            if (chargeLinkCommandAcceptedEvent == null) throw new ArgumentNullException(nameof(chargeLinkCommandAcceptedEvent));

            var link = chargeLinkCommandAcceptedEvent.ChargeLink;

            var charge = await _chargeRepository
                .GetChargeAsync(link.SenderProvidedChargeId, link.ChargeOwner, link.ChargeType)
                .ConfigureAwait(false);

            if (!charge.TaxIndicator) return;

            var dataAvailableNotification = CreateDataAvailableNotificationDto(chargeLinkCommandAcceptedEvent);
            await _dataAvailableNotificationSender
                .SendAsync(dataAvailableNotification)
                .ConfigureAwait(false);
        }

        private static DataAvailableNotificationDto CreateDataAvailableNotificationDto(
            ChargeLinkCommandAcceptedEvent createdChargeLink)
        {
            // The ID that the charges domain must handle when peeking
            var chargeDomainReferenceId = Guid.NewGuid();

            // The grid operator initiating the creation of the charge link is also
            // the receiver of the confirmation
            var receiver = createdChargeLink.Document.Sender.Id;

            // Different processes must not be bundled together.
            // The can be differentiated by business reason codes.
            var messageType = createdChargeLink.Document.BusinessReasonCode.ToString();

            return new DataAvailableNotificationDto(
                chargeDomainReferenceId,
                new GlobalLocationNumberDto(receiver),
                new MessageTypeDto(messageType),
                DomainOrigin.Charges,
                SupportsBundling: true,
                MessageWeight);
        }
    }
}
