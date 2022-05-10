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
using GreenEnergyHub.Charges.Application.ChargeCommands.Acknowledgement;
using GreenEnergyHub.Charges.Application.Persistence;
using GreenEnergyHub.Charges.Domain.ChargeInformation;
using GreenEnergyHub.Charges.Domain.ChargePrices;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommandReceivedEvents;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommands;
using GreenEnergyHub.Charges.Domain.Dtos.Validation;

namespace GreenEnergyHub.Charges.Application.ChargePrices.Handlers
{
    public class ChargePricesHandler : IChargePricesHandler
    {
        private readonly IChargeCommandReceiptService _chargePriceReceiptService;
        private readonly IInputValidator<ChargeCommand> _inputValidator;
        private readonly IChargeRepository _chargeInformationRepository;
        private readonly IChargePriceRepository _chargePriceRepository;
        private readonly IChargePriceFactory _chargePriceFactory;
        private readonly IUnitOfWork _unitOfWork;

        public ChargePricesHandler(
            IChargeCommandReceiptService chargePriceReceiptService,
            IInputValidator<ChargeCommand> inputValidator,
            IChargeRepository chargeInformationRepository,
            IChargePriceRepository chargePriceRepository,
            IChargePriceFactory chargePriceFactory,
            IUnitOfWork unitOfWork)
        {
            _chargePriceReceiptService = chargePriceReceiptService;
            _inputValidator = inputValidator;
            _chargeInformationRepository = chargeInformationRepository;
            _chargePriceRepository = chargePriceRepository;
            _chargePriceFactory = chargePriceFactory;
            _unitOfWork = unitOfWork;
        }

        public async Task HandleAsync(
            ChargeCommandReceivedEvent commandReceivedEvent)
        {
            ArgumentNullException.ThrowIfNull(commandReceivedEvent);

            var inputValidationResult = _inputValidator.Validate(commandReceivedEvent.Command);

            if (inputValidationResult.IsFailed)
            {
                await _chargePriceReceiptService
                    .RejectAsync(commandReceivedEvent.Command, inputValidationResult).ConfigureAwait(false);
                return;
            }

            var acceptedOperations = new List<ChargeOperationDto>();

            foreach (var operation in commandReceivedEvent.Command.ChargeOperations)
            {
                var chargeIdentifier = new ChargeIdentifier(operation.ChargeId, operation.ChargeOwner, operation.Type);
                var chargeInformation = await _chargeInformationRepository.GetOrNullAsync(chargeIdentifier).ConfigureAwait(false);
                ArgumentNullException.ThrowIfNull(chargeInformation);
                await HandlePricesFromChargePriceOperationAsync(chargeInformation.Id, operation).ConfigureAwait(false);
                acceptedOperations.Add(operation);
            }

            await _unitOfWork.SaveChangesAsync().ConfigureAwait(false);

            var acceptedChargeCommand = new ChargeCommand(
                commandReceivedEvent.Command.Document,
                acceptedOperations);
            await _unitOfWork.SaveChangesAsync().ConfigureAwait(false);
            await _chargePriceReceiptService.AcceptAsync(acceptedChargeCommand).ConfigureAwait(false);
        }

        private async Task HandlePricesFromChargePriceOperationAsync(Guid chargeInformationId, ChargeOperationDto operation)
        {
            var chargePrices = await GetChargePriceAsync(chargeInformationId, operation).ConfigureAwait(false);
            _chargePriceRepository.RemoveRange(chargePrices);
            foreach (var point in operation.Points)
            {
                var chargePrice = await _chargePriceFactory.CreateFromChargeOperationDtoAsync(operation, point)
                    .ConfigureAwait(false);
                await _chargePriceRepository.AddAsync(chargePrice).ConfigureAwait(false);
            }
        }

        private async Task<IEnumerable<ChargePrice>> GetChargePriceAsync(Guid chargeId, ChargeOperationDto operationDto)
        {
            var orderedPoints = operationDto.Points.OrderBy(x => x.Time).ToList();
            ArgumentNullException.ThrowIfNull(orderedPoints);
            var startDate = orderedPoints.First().Time;
            var endDate = orderedPoints.Last().Time;
            return
                await _chargePriceRepository
                    .GetOrNullAsync(
                        chargeId,
                        startDate,
                        endDate).ConfigureAwait(false);
        }
    }
}
