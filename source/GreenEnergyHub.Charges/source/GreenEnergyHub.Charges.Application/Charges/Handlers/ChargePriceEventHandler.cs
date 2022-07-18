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
using GreenEnergyHub.Charges.Application.Persistence;
using GreenEnergyHub.Charges.Domain.Charges;
using GreenEnergyHub.Charges.Domain.Charges.Exceptions;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeInformationCommands.Validation.BusinessValidation.ValidationRules;
using GreenEnergyHub.Charges.Domain.Dtos.ChargePriceCommandReceivedEvents;
using GreenEnergyHub.Charges.Domain.Dtos.ChargePriceCommands;
using GreenEnergyHub.Charges.Domain.Dtos.Validation;
using GreenEnergyHub.Charges.Domain.MarketParticipants;

namespace GreenEnergyHub.Charges.Application.Charges.Handlers
{
    public class ChargePriceEventHandler : IChargePriceEventHandler
    {
        private readonly IChargeRepository _chargeRepository;
        private readonly IMarketParticipantRepository _marketParticipantRepository;
        private readonly IInputValidator<ChargePriceOperationDto> _inputValidator;
        private readonly IUnitOfWork _unitOfWork;

        public ChargePriceEventHandler(
            IChargeRepository chargeRepository,
            IMarketParticipantRepository marketParticipantRepository,
            IInputValidator<ChargePriceOperationDto> inputValidator,
            IUnitOfWork unitOfWork)
        {
            _chargeRepository = chargeRepository;
            _marketParticipantRepository = marketParticipantRepository;
            _inputValidator = inputValidator;
            _unitOfWork = unitOfWork;
        }

        public async Task HandleAsync(ChargePriceCommandReceivedEvent commandReceivedEvent)
        {
            ArgumentNullException.ThrowIfNull(commandReceivedEvent);

            var operations = commandReceivedEvent.Command.Operations.ToArray();
            var operationsToBeRejected = new List<ChargePriceOperationDto>();
            var rejectionRules = new List<IValidationRuleContainer>();
            var operationsToBeConfirmed = new List<ChargePriceOperationDto>();

            for (var i = 0; i < operations.Length; i++)
            {
                var operation = operations[i];

                var inputValidationResult = _inputValidator.Validate(operation);
                if (inputValidationResult.IsFailed)
                {
                    operationsToBeRejected = operations[i..].ToList();
                    CollectRejectionRules(rejectionRules, inputValidationResult, operationsToBeRejected, operation);
                    break;
                }

                var charge = await GetChargeAsync(operation).ConfigureAwait(false);

                try
                {
                    //await HandleOperationAsync(operation, charge).ConfigureAwait(false);
                }
                catch (ChargeOperationFailedException exception)
                {
                    operationsToBeRejected = operations[i..].ToList();
                    CollectRejectionRules(
                        rejectionRules,
                        ValidationResult.CreateFailure(exception.InvalidRules),
                        operationsToBeRejected,
                        operation);
                    break;
                }

                operationsToBeConfirmed.Add(operation);
            }

            //ConfirmationRepository
            //RejectionServiceRepository
            //NotificationServiceRepository
            await _unitOfWork.SaveChangesAsync().ConfigureAwait(false);

            //var document = commandReceivedEvent.Command.Document;
            //await _chargeCommandReceiptService.RejectInvalidOperationsAsync(operationsToBeRejected, document, rejectionRules).ConfigureAwait(false);
            //await _chargeCommandReceiptService.AcceptValidOperationsAsync(operationsToBeConfirmed, document).ConfigureAwait(false);
        }

        //private async Task HandleOperationAsync(ChargePriceOperationDto operation, Charge? charge)
        //{
        //    var operationType = GetOperationType(operation, charge);
        //    switch (operationType)
        //    {
        //        case OperationType.Create:
        //            await HandleCreateEventAsync(operation).ConfigureAwait(false);
        //            break;
        //        case OperationType.Update:
        //            HandleUpdateEvent(charge!, operation);
        //            break;
        //        case OperationType.Stop:
        //            charge!.Stop(operation.EndDateTime);
        //            break;
        //        case OperationType.CancelStop:
        //            HandleCancelStopEvent(charge!, operation);
        //            break;
        //        default:
        //            throw new InvalidOperationException("Could not handle charge command.");
        //    }
        //}
        private static void CollectRejectionRules(
            List<IValidationRuleContainer> rejectionRules,
            ValidationResult validationResult,
            IEnumerable<ChargePriceOperationDto> operationsToBeRejected,
            ChargePriceOperationDto operation)
        {
            rejectionRules.AddRange(validationResult.InvalidRules);
            rejectionRules.AddRange(operationsToBeRejected.Skip(1)
                .Select(subsequentOperation =>
                    new OperationValidationRuleContainer(
                        new PreviousOperationsMustBeValidRule(operation.Id), subsequentOperation.Id)));
        }

        private async Task<Charge?> GetChargeAsync(ChargePriceOperationDto chargeOperationDto)
        {
            var marketParticipant = await _marketParticipantRepository
                .GetSystemOperatorOrGridAccessProviderAsync(chargeOperationDto.ChargeOwner)
                .ConfigureAwait(false);

            var chargeIdentifier = new ChargeIdentifier(
                chargeOperationDto.ChargeId,
                marketParticipant.Id,
                chargeOperationDto.Type);
            return await _chargeRepository.SingleOrNullAsync(chargeIdentifier).ConfigureAwait(false);
        }
    }
}
