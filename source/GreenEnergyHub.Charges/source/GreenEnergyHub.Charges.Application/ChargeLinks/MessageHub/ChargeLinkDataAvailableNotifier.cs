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
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Energinet.DataHub.MessageHub.Client.DataAvailable;
using Energinet.DataHub.MessageHub.Model.Model;
using GreenEnergyHub.Charges.Domain.AvailableChargeLinksData;
using GreenEnergyHub.Charges.Domain.Charges;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeLinksAcceptedEvents;
using GreenEnergyHub.Charges.Domain.MarketParticipants;
using NodaTime;

namespace GreenEnergyHub.Charges.Application.ChargeLinks.MessageHub
{
    public class ChargeLinkDataAvailableNotifier : IChargeLinkDataAvailableNotifier
    {
        /// <summary>
        /// The upper anticipated weight (kilobytes) contribution to the final bundle from the charge link created event.
        /// </summary>
        private const int MessageWeight = 2;

        private readonly IDataAvailableNotificationSender _dataAvailableNotificationSender;
        private readonly IChargeRepository _chargeRepository;
        private readonly IAvailableChargeLinksDataRepository _availableChargeLinksDataRepository;
        private readonly IMarketParticipantRepository _marketParticipantRepository;
        private readonly IAvailableChargeLinksDataFactory _availableChargeLinksDataFactory;
        private readonly IClock _clock;
        private readonly ICorrelationContext _correlationContext;

        public ChargeLinkDataAvailableNotifier(
            IDataAvailableNotificationSender dataAvailableNotificationSender,
            IChargeRepository chargeRepository,
            IAvailableChargeLinksDataRepository availableChargeLinksDataRepository,
            IMarketParticipantRepository marketParticipantRepository,
            IAvailableChargeLinksDataFactory availableChargeLinksDataFactory,
            IClock clock,
            ICorrelationContext correlationContext)
        {
            _dataAvailableNotificationSender = dataAvailableNotificationSender;
            _chargeRepository = chargeRepository;
            _availableChargeLinksDataRepository = availableChargeLinksDataRepository;
            _marketParticipantRepository = marketParticipantRepository;
            _availableChargeLinksDataFactory = availableChargeLinksDataFactory;
            _clock = clock;
            _correlationContext = correlationContext;
        }

        public async Task NotifyAsync([NotNull] ChargeLinksAcceptedEvent chargeLinksAcceptedEvent)
        {
            if (chargeLinksAcceptedEvent == null) throw new ArgumentNullException(nameof(chargeLinksAcceptedEvent));

            var dataAvailableNotificationDtos = new List<DataAvailableNotificationDto>();

            // It is the responsibility of the Charge Domain to find the recipient and
            // not considered part of the Create Metering Point orchestration.
            // We select the first as all bundled messages will have the same recipient
            var recipient =
                _marketParticipantRepository.GetGridAccessProvider(chargeLinksAcceptedEvent.ChargeLinksCommand.MeteringPointId);

            // When available this should be parsed on from API management to be more precise.
            var now = _clock.GetCurrentInstant();
            var availableChargeLinksData = new List<AvailableChargeLinksData>();

            foreach (var chargeLinkDto in chargeLinksAcceptedEvent.ChargeLinksCommand.ChargeLinks)
            {
                var charge = await _chargeRepository.GetChargeAsync(new ChargeIdentifier(
                    chargeLinkDto.SenderProvidedChargeId,
                    chargeLinkDto.ChargeOwner,
                    chargeLinkDto.ChargeType)).ConfigureAwait(false);

                if (charge.TaxIndicator)
                {
                    var dataAvailableNotificationDto = CreateDataAvailableNotificationDto(
                        chargeLinksAcceptedEvent.ChargeLinksCommand.Document.BusinessReasonCode,
                        recipient.Id);
                    dataAvailableNotificationDtos.Add(dataAvailableNotificationDto);
                    availableChargeLinksData.Add(_availableChargeLinksDataFactory.CreateAvailableChargeLinksData(
                        chargeLinkDto,
                        recipient,
                        chargeLinksAcceptedEvent.ChargeLinksCommand.Document.BusinessReasonCode,
                        chargeLinksAcceptedEvent.ChargeLinksCommand.MeteringPointId,
                        now,
                        dataAvailableNotificationDto.Uuid));
                }
            }

            await _availableChargeLinksDataRepository.StoreAsync(availableChargeLinksData);

            var dataAvailableNotificationSenderTasks = dataAvailableNotificationDtos
                .Select(x => _dataAvailableNotificationSender.SendAsync(_correlationContext.Id, x));

            await Task.WhenAll(dataAvailableNotificationSenderTasks).ConfigureAwait(false);
        }

        private static DataAvailableNotificationDto CreateDataAvailableNotificationDto(
            BusinessReasonCode businessReasonCode, string recipientId)
        {
            // The ID that the charges domain must handle when peeking
            var chargeDomainReferenceId = Guid.NewGuid();

            // Different processes must not be bundled together.
            // The can be differentiated by business reason codes.
            var messageType = businessReasonCode.ToString();

            return new DataAvailableNotificationDto(
                chargeDomainReferenceId,
                new GlobalLocationNumberDto(recipientId),
                new MessageTypeDto(messageType),
                DomainOrigin.Charges,
                SupportsBundling: true,
                MessageWeight);
        }
    }
}
