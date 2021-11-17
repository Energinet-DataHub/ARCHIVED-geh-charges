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
using GreenEnergyHub.Charges.Domain.AvailableChargeLinksData;
using GreenEnergyHub.Charges.Domain.Charges;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeLinkCommandAcceptedEvents;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeLinkCommands;
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

        public ChargeLinkDataAvailableNotifier(
            IDataAvailableNotificationSender dataAvailableNotificationSender,
            IChargeRepository chargeRepository,
            IAvailableChargeLinksDataRepository availableChargeLinksDataRepository,
            IMarketParticipantRepository marketParticipantRepository,
            IAvailableChargeLinksDataFactory availableChargeLinksDataFactory,
            IClock clock)
        {
            _dataAvailableNotificationSender = dataAvailableNotificationSender;
            _chargeRepository = chargeRepository;
            _availableChargeLinksDataRepository = availableChargeLinksDataRepository;
            _marketParticipantRepository = marketParticipantRepository;
            _availableChargeLinksDataFactory = availableChargeLinksDataFactory;
            _clock = clock;
        }

        public async Task NotifyAsync(ChargeLinkCommandAcceptedEvent chargeLinkCommandAcceptedEvent)
        {
            if (chargeLinkCommandAcceptedEvent == null) throw new ArgumentNullException(nameof(chargeLinkCommandAcceptedEvent));

            var dataAvailableNotificationDtos = new List<DataAvailableNotificationDto>();

            // It is the responsibility of the Charge Domain to find the recipient and
            // not considered part of the Create Metering Point orchestration.
            // We select the first as all bundled messages will have the same recipient
            var meteringPointId = SingleMeteringPointId(chargeLinkCommandAcceptedEvent);
            var recipient = _marketParticipantRepository.GetGridAccessProvider(meteringPointId);

            // When available this should be parsed on from API management to be more precise.
            var now = _clock.GetCurrentInstant();
            var availableChargeLinksData = new List<AvailableChargeLinksData>();

            foreach (var chargeLinkCommand in chargeLinkCommandAcceptedEvent.ChargeLinkCommands)
            {
                var charge = await _chargeRepository.GetChargeAsync(new ChargeIdentifier(
                    chargeLinkCommand.ChargeLink.SenderProvidedChargeId,
                    chargeLinkCommand.ChargeLink.ChargeOwner,
                    chargeLinkCommand.ChargeLink.ChargeType)).ConfigureAwait(false);

                if (charge.TaxIndicator)
                {
                    var dataAvailableNotificationDto = CreateDataAvailableNotificationDto(chargeLinkCommand, recipient.Id);
                    dataAvailableNotificationDtos.Add(dataAvailableNotificationDto);
                    availableChargeLinksData.Add(_availableChargeLinksDataFactory.CreateAvailableChargeLinksData(
                        chargeLinkCommand,
                        recipient,
                        now,
                        dataAvailableNotificationDto.Uuid));
                }
            }

            await _availableChargeLinksDataRepository.StoreAsync(availableChargeLinksData);

            var dataAvailableNotificationSenderTasks = dataAvailableNotificationDtos
                .Select(x => _dataAvailableNotificationSender.SendAsync(x));

            await Task.WhenAll(dataAvailableNotificationSenderTasks).ConfigureAwait(false);
        }

        private static string SingleMeteringPointId(ChargeLinkCommandAcceptedEvent chargeLinkCommandAcceptedEvent)
        {
            var isSameMeteringPoint = chargeLinkCommandAcceptedEvent
                .ChargeLinkCommands
                .Select(command => command.ChargeLink.MeteringPointId)
                .Distinct()
                .Count() == 1;

            if (!isSameMeteringPoint)
                throw new InvalidOperationException("All commands in accepted event must be for same metering point.");

            return chargeLinkCommandAcceptedEvent.ChargeLinkCommands.First().ChargeLink.MeteringPointId;
        }

        private static DataAvailableNotificationDto CreateDataAvailableNotificationDto(
            ChargeLinkCommand chargeLinkCommand, string recipientId)
        {
            // The ID that the charges domain must handle when peeking
            var chargeDomainReferenceId = Guid.NewGuid();

            // Different processes must not be bundled together.
            // The can be differentiated by business reason codes.
            var messageType = chargeLinkCommand.Document.BusinessReasonCode.ToString();

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
