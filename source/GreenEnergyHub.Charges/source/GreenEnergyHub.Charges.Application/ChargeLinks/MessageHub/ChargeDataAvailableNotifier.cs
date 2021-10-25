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
using Energinet.DataHub.MessageHub.Client.DataAvailable;
using Energinet.DataHub.MessageHub.Client.Model;
using GreenEnergyHub.Charges.Domain.AvailableChargeData;
using GreenEnergyHub.Charges.Domain.ChargeCommandAcceptedEvents;
using GreenEnergyHub.Charges.Domain.ChargeCommands;
using GreenEnergyHub.Charges.Domain.MarketParticipants;
using NodaTime;

namespace GreenEnergyHub.Charges.Application.ChargeLinks.MessageHub
{
    public class ChargeDataAvailableNotifier : IChargeDataAvailableNotifier
    {
        /// <summary>
        /// The upper anticipated weight (kilobytes) contribution to the final bundle from the charge link created event.
        /// </summary>
        private const decimal ChargeMessageWeight = 2m;
        private const decimal ChargePointMessageWeight = 0.1m;

        private readonly IDataAvailableNotificationSender _dataAvailableNotificationSender;
        private readonly IAvailableChargeDataRepository _availableChargeDataRepository;
        private readonly IAvailableChargeDataFactory _availableChargeDataFactory;
        private readonly IMarketParticipantRepository _marketParticipantRepository;
        private readonly IClock _clock;

        public ChargeDataAvailableNotifier(
            IDataAvailableNotificationSender dataAvailableNotificationSender,
            IAvailableChargeDataRepository availableChargeDataRepository,
            IAvailableChargeDataFactory availableChargeDataFactory,
            IMarketParticipantRepository marketParticipantRepository,
            IClock clock)
        {
            _dataAvailableNotificationSender = dataAvailableNotificationSender;
            _availableChargeDataRepository = availableChargeDataRepository;
            _availableChargeDataFactory = availableChargeDataFactory;
            _marketParticipantRepository = marketParticipantRepository;
            _clock = clock;
        }

        public async Task NotifyAsync(ChargeCommandAcceptedEvent chargeCommandAcceptedEvent)
        {
            if (chargeCommandAcceptedEvent == null) throw new ArgumentNullException(nameof(chargeCommandAcceptedEvent));

            var now = _clock.GetCurrentInstant();

            var gridAccessProviders = await _marketParticipantRepository.GetActiveGridAccessProvidersAsync();

            var messageWeight =
                (int)(chargeCommandAcceptedEvent.Command.ChargeOperation.Points.Count * ChargePointMessageWeight) +
                (int)ChargeMessageWeight;

            foreach (var provider in gridAccessProviders)
            {
                var dataAvailableNotificationDto = CreateDataAvailableNotificationDto(
                    chargeCommandAcceptedEvent.Command,
                    provider.Id,
                    messageWeight);

                var availableChargeData = _availableChargeDataFactory.Create(
                    chargeCommandAcceptedEvent.Command,
                    provider,
                    now,
                    dataAvailableNotificationDto.Uuid);
                await _availableChargeDataRepository.StoreAsync(availableChargeData);

                await _dataAvailableNotificationSender.SendAsync(dataAvailableNotificationDto);
            }
        }

        private static DataAvailableNotificationDto CreateDataAvailableNotificationDto(
            ChargeCommand chargeCommand, string recipientId, int messageWeight)
        {
            // The ID that the charges domain must handle when peeking
            var chargeDomainReferenceId = Guid.NewGuid();

            // Different processes must not be bundled together.
            // The can be differentiated by business reason codes.
            var messageType = chargeCommand.Document.BusinessReasonCode.ToString();

            return new DataAvailableNotificationDto(
                chargeDomainReferenceId,
                new GlobalLocationNumberDto(recipientId),
                new MessageTypeDto(messageType),
                DomainOrigin.Charges,
                SupportsBundling: true,
                messageWeight);
        }
    }
}
