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
using Energinet.DataHub.MessageHub.Model.Model;
using GreenEnergyHub.Charges.Domain.AvailableChargeData;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommandAcceptedEvents;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommands;
using GreenEnergyHub.Charges.Domain.MarketParticipants;
using NodaTime;

namespace GreenEnergyHub.Charges.Application.ChargeLinks.MessageHub
{
    public class ChargeDataAvailableNotifier : IChargeDataAvailableNotifier
    {
        /// <summary>
        /// The upper anticipated weight (kilobytes) contribution to the final bundle from the charge created event.
        /// </summary>
        private const decimal ChargeMessageWeight = 2m;
        private const decimal ChargePointMessageWeight = 0.1m;

        private readonly IDataAvailableNotificationSender _dataAvailableNotificationSender;
        private readonly IAvailableChargeDataRepository _availableChargeDataRepository;
        private readonly IAvailableChargeDataFactory _availableChargeDataFactory;
        private readonly IMarketParticipantRepository _marketParticipantRepository;
        private readonly IClock _clock;
        private readonly ICorrelationContext _correlationContext;

        public ChargeDataAvailableNotifier(
            IDataAvailableNotificationSender dataAvailableNotificationSender,
            IAvailableChargeDataRepository availableChargeDataRepository,
            IAvailableChargeDataFactory availableChargeDataFactory,
            IMarketParticipantRepository marketParticipantRepository,
            IClock clock,
            ICorrelationContext correlationContext)
        {
            _dataAvailableNotificationSender = dataAvailableNotificationSender;
            _availableChargeDataRepository = availableChargeDataRepository;
            _availableChargeDataFactory = availableChargeDataFactory;
            _marketParticipantRepository = marketParticipantRepository;
            _clock = clock;
            _correlationContext = correlationContext;
        }

        public async Task NotifyAsync(ChargeCommandAcceptedEvent chargeCommandAcceptedEvent)
        {
            if (chargeCommandAcceptedEvent == null) throw new ArgumentNullException(nameof(chargeCommandAcceptedEvent));

            // When available this should be parsed on from API management to be more precise.
            var now = _clock.GetCurrentInstant();

            if (chargeCommandAcceptedEvent.Command.ChargeOperation.TaxIndicator is false)
                return;

            var dataAvailableNotificationDtos =
                await GenerateDataAvailableNotificationDtosAsync(chargeCommandAcceptedEvent, now);

            var dataAvailableNotificationSenderTasks = dataAvailableNotificationDtos
                .Select(x => _dataAvailableNotificationSender.SendAsync(_correlationContext.Id, x));
            await Task.WhenAll(dataAvailableNotificationSenderTasks).ConfigureAwait(false);
        }

        private async Task<List<DataAvailableNotificationDto>> GenerateDataAvailableNotificationDtosAsync(
            ChargeCommandAcceptedEvent chargeCommandAcceptedEvent,
            Instant now)
        {
            var dataAvailableNotificationDtos = new List<DataAvailableNotificationDto>();
            var pointsCount = chargeCommandAcceptedEvent.Command.ChargeOperation.Points.Count;
            var messageWeight = (int)Math.Round(
                (pointsCount * ChargePointMessageWeight) + ChargeMessageWeight,
                MidpointRounding.AwayFromZero);

            var activeGridAccessProviders = await _marketParticipantRepository.GetActiveGridAccessProvidersAsync();

            foreach (var provider in activeGridAccessProviders)
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

            return dataAvailableNotificationDtos;
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
