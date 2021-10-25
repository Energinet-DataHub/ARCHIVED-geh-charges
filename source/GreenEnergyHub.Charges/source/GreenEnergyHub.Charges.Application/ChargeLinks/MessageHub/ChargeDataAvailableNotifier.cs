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
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Energinet.DataHub.MessageHub.Client.DataAvailable;
using Energinet.DataHub.MessageHub.Client.Model;
using GreenEnergyHub.Charges.Domain.AvailableChargeData;
using GreenEnergyHub.Charges.Domain.ChargeCommandAcceptedEvents;
using GreenEnergyHub.Charges.Domain.ChargeCommands;
using GreenEnergyHub.Charges.Domain.Charges;
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
        private readonly IChargeRepository _chargeRepository;
        private readonly IAvailableChargeDataFactory _availableChargeDataFactory;
        private readonly IMarketParticipantRepository _marketParticipantRepository;
        private readonly IClock _clock;

        public ChargeDataAvailableNotifier(
            IDataAvailableNotificationSender dataAvailableNotificationSender,
            IAvailableChargeDataRepository availableChargeDataRepository,
            IChargeRepository chargeRepository,
            IAvailableChargeDataFactory availableChargeDataFactory,
            IMarketParticipantRepository marketParticipantRepository,
            IClock clock)
        {
            _dataAvailableNotificationSender = dataAvailableNotificationSender;
            _availableChargeDataRepository = availableChargeDataRepository;
            _chargeRepository = chargeRepository;
            _availableChargeDataFactory = availableChargeDataFactory;
            _marketParticipantRepository = marketParticipantRepository;
            _clock = clock;
        }

        public async Task NotifyAsync(ChargeCommandAcceptedEvent chargeCommandAcceptedEvent)
        {
            if (chargeCommandAcceptedEvent == null) throw new ArgumentNullException(nameof(chargeCommandAcceptedEvent));

            // When available this should be parsed on from API management to be more precise.
            var now = _clock.GetCurrentInstant();

            var charge = await _chargeRepository.GetChargeAsync(new ChargeIdentifier(
                chargeCommandAcceptedEvent.Command.ChargeOperation.ChargeId,
                chargeCommandAcceptedEvent.Command.ChargeOperation.ChargeOwner,
                chargeCommandAcceptedEvent.Command.ChargeOperation.Type)).ConfigureAwait(false);

            if (charge.TaxIndicator is false)
                return;

            var gridAccessProviders = await _marketParticipantRepository.GetActiveGridAccessProvidersAsync();

            var messageWeight =
                (int)(chargeCommandAcceptedEvent.Command.ChargeOperation.Points.Count * ChargePointMessageWeight) +
                (int)ChargeMessageWeight;

            var dataAvailableNotificationDtos = new List<DataAvailableNotificationDto>();
            foreach (var provider in gridAccessProviders)
            {
                var dataAvailableNotificationDto = CreateDataAvailableNotificationDto(
                    chargeCommandAcceptedEvent.Command,
                    provider.Id,
                    messageWeight);
                dataAvailableNotificationDtos.Add(dataAvailableNotificationDto);

                var availableChargeData = _availableChargeDataFactory.Create(
                    chargeCommandAcceptedEvent.Command,
                    provider,
                    now,
                    dataAvailableNotificationDto.Uuid);
                await _availableChargeDataRepository.StoreAsync(availableChargeData);
            }

            var dataAvailableNotificationSenderTasks = dataAvailableNotificationDtos
                .Select(x => _dataAvailableNotificationSender.SendAsync(x));
            await Task.WhenAll(dataAvailableNotificationSenderTasks).ConfigureAwait(false);
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
