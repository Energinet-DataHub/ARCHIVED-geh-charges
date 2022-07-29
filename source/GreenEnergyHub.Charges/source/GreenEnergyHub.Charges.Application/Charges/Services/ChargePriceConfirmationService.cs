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
using System.Threading.Tasks;
using GreenEnergyHub.Charges.Application.AvailableData.Factories;
using GreenEnergyHub.Charges.Application.Messaging;
using GreenEnergyHub.Charges.Domain.AvailableData.AvailableChargeReceiptData;
using GreenEnergyHub.Charges.Domain.AvailableData.AvailableData;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommandAcceptedEvents;
using GreenEnergyHub.Charges.Domain.Dtos.ChargePriceAcceptedEvents;
using GreenEnergyHub.Charges.Domain.Dtos.ChargePriceCommands;
using GreenEnergyHub.Charges.Domain.Dtos.SharedDtos;
using Microsoft.Extensions.Logging;

namespace GreenEnergyHub.Charges.Application.Charges.Services
{
    public class ChargePriceConfirmationService : IChargePriceConfirmationService
    {
        private readonly IChargePriceAcceptedEventFactory _chargePriceAcceptedEventFactory;
        private readonly IAvailableDataRepository<AvailableChargeReceiptData> _availableChargeReceiptRepository;
        private readonly IAvailableDataFactory<AvailableChargeReceiptData, ChargePriceAcceptedEvent> _availableChargePriceReceiptDataFactory;
        private readonly IMessageDispatcher<ChargePriceAcceptedEvent> _acceptedMessageDispatcher;

        private readonly ILogger _logger;

        public ChargePriceConfirmationService(
            IChargePriceAcceptedEventFactory chargePriceAcceptedEventFactory,
            IAvailableDataRepository<AvailableChargeReceiptData> availableChargeReceiptRepository,
            IAvailableDataFactory<AvailableChargeReceiptData, ChargePriceAcceptedEvent> availableChargePriceReceiptDataFactory,
            IMessageDispatcher<ChargePriceAcceptedEvent> acceptedMessageDispatcher,
            ILoggerFactory loggerFactory)
        {
            _chargePriceAcceptedEventFactory = chargePriceAcceptedEventFactory;
            _availableChargeReceiptRepository = availableChargeReceiptRepository;
            _availableChargePriceReceiptDataFactory = availableChargePriceReceiptDataFactory;
            _acceptedMessageDispatcher = acceptedMessageDispatcher;
            _logger = loggerFactory.CreateLogger(nameof(ChargePriceConfirmationService));
        }

        public async Task SaveConfirmationsAsync(
            DocumentDto document,
            List<ChargePriceOperationDto> confirmedPriceOperations)
        {
            var command = new ChargePriceCommand(document, confirmedPriceOperations);
            var acceptedEvent = _chargePriceAcceptedEventFactory.CreateEvent(command);
            var availableData = await _availableChargePriceReceiptDataFactory.CreateAsync(acceptedEvent).ConfigureAwait(false);
            await _availableChargeReceiptRepository.AddAsync(availableData).ConfigureAwait(false);

            //TODO: Add acceptedevent to a list and send them after UnitOfWork instead of here, this is just for proof that it will go through the tests
            //TODO: After outbox is implemented, no events should be dispatched and the above TODO is about to be deleted
            await _acceptedMessageDispatcher.DispatchAsync(acceptedEvent).ConfigureAwait(false);
            _logger.LogInformation($"{confirmedPriceOperations.Count} confirmed price operations was persisted.");
        }
    }
}
