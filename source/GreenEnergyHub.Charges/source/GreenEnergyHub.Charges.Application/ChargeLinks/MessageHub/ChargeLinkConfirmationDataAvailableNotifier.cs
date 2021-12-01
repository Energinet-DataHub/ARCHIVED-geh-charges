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

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Energinet.DataHub.MessageHub.Client.DataAvailable;
using Energinet.DataHub.MessageHub.Model.Model;
using GreenEnergyHub.Charges.Domain.AvailableChargeLinkReceiptData;
using GreenEnergyHub.Charges.Domain.AvailableData;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeLinksAcceptedEvents;
using GreenEnergyHub.Charges.Domain.MarketParticipants;

namespace GreenEnergyHub.Charges.Application.ChargeLinks.MessageHub
{
    public class ChargeLinkConfirmationDataAvailableNotifier : IChargeLinkConfirmationDataAvailableNotifier
    {
        /// <summary>
        /// The upper anticipated weight (kilobytes) contribution to the final bundle of the charge link receipt.
        /// </summary>
        public const int MessageWeight = 2;

        /// <summary>
        /// Is used in communication with Message Hub.
        /// Be cautious to change it!
        /// </summary>
        public const string MessageTypePrefix = "ChargeLinkReceiptDataAvailable";

        private readonly IAvailableChargeLinkReceiptDataFactory _availableChargeLinkReceiptDataFactory;
        private readonly IMessageMetaDataContext _messageMetaDataContext;
        private readonly IAvailableDataRepository<AvailableChargeLinkReceiptData> _availableChargeLinkReceiptDataRepository;
        private readonly IDataAvailableNotificationSender _dataAvailableNotificationSender;
        private readonly ICorrelationContext _correlationContext;

        public ChargeLinkConfirmationDataAvailableNotifier(
            IAvailableChargeLinkReceiptDataFactory availableChargeLinkReceiptDataFactory,
            IMessageMetaDataContext messageMetaDataContext,
            IAvailableDataRepository<AvailableChargeLinkReceiptData> availableChargeLinkReceiptDataRepository,
            IDataAvailableNotificationSender dataAvailableNotificationSender,
            ICorrelationContext correlationContext)
        {
            _availableChargeLinkReceiptDataFactory = availableChargeLinkReceiptDataFactory;
            _messageMetaDataContext = messageMetaDataContext;
            _availableChargeLinkReceiptDataRepository = availableChargeLinkReceiptDataRepository;
            _dataAvailableNotificationSender = dataAvailableNotificationSender;
            _correlationContext = correlationContext;
        }

        public async Task NotifyAsync([NotNull] ChargeLinksAcceptedEvent chargeLinksAcceptedEvent)
        {
            if (!ShouldSendReceipt(chargeLinksAcceptedEvent))
                return;

            var confirmations = CreateConfirmations(chargeLinksAcceptedEvent);

            await StoreConfirmationsAsync(confirmations).ConfigureAwait(false);

            await NotifyMessageHubAsync(confirmations).ConfigureAwait(false);
        }

        private bool ShouldSendReceipt(ChargeLinksAcceptedEvent chargeLinksAcceptedEvent)
        {
            // We do not need to send to system operators
            return chargeLinksAcceptedEvent.ChargeLinksCommand.Document.Sender.BusinessProcessRole !=
                   MarketParticipantRole.SystemOperator;
        }

        private IReadOnlyCollection<AvailableChargeLinkReceiptData> CreateConfirmations(ChargeLinksAcceptedEvent chargeLinksAcceptedEvent)
        {
            return _availableChargeLinkReceiptDataFactory.CreateConfirmations(
                    chargeLinksAcceptedEvent.ChargeLinksCommand,
                    _messageMetaDataContext.RequestDataTime);
        }

        private async Task StoreConfirmationsAsync(IReadOnlyCollection<AvailableChargeLinkReceiptData> confirmations)
        {
            await _availableChargeLinkReceiptDataRepository.StoreAsync(confirmations.ToList());
        }

        private async Task NotifyMessageHubAsync(IReadOnlyCollection<AvailableChargeLinkReceiptData> confirmations)
        {
            var notifications = CreateNotifications(confirmations);

            var dataAvailableNotificationSenderTasks = notifications
                .Select(x => _dataAvailableNotificationSender.SendAsync(_correlationContext.Id, x));

            await Task.WhenAll(dataAvailableNotificationSenderTasks).ConfigureAwait(false);
        }

        private IEnumerable<DataAvailableNotificationDto> CreateNotifications(
            IReadOnlyCollection<AvailableChargeLinkReceiptData> confirmations)
        {
            return confirmations.Select(
                confirmation => new DataAvailableNotificationDto(
                    confirmation.AvailableDataReferenceId,
                    new GlobalLocationNumberDto(confirmation.RecipientId),
                    new MessageTypeDto(MessageTypePrefix + "_" + confirmation.BusinessReasonCode),
                    DomainOrigin.Charges,
                    true,
                    MessageWeight))
                .ToList();
        }
    }
}
