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
using GreenEnergyHub.Charges.Application.ChargeLinks.Services;
using GreenEnergyHub.Charges.Application.Persistence;
using GreenEnergyHub.Charges.Domain.ChargeLinks;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeLinksCommands;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeLinksCommands.Validation.BusinessValidation.ValidationRules;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeLinksReceivedEvents;
using GreenEnergyHub.Charges.Domain.Dtos.SharedDtos;
using GreenEnergyHub.Charges.Domain.Dtos.Validation;

namespace GreenEnergyHub.Charges.Application.ChargeLinks.Handlers
{
    public class ChargeLinksReceivedEventHandler : IChargeLinksReceivedEventHandler
    {
        private readonly IChargeLinksReceiptService _chargeLinksReceiptService;
        private readonly IChargeLinkFactory _chargeLinkFactory;
        private readonly IChargeLinksRepository _chargeLinksRepository;
        private readonly IBusinessValidator<ChargeLinkDto> _businessValidator;
        private readonly IUnitOfWork _unitOfWork;

        public ChargeLinksReceivedEventHandler(
            IChargeLinksReceiptService chargeLinksReceiptService,
            IChargeLinkFactory chargeLinkFactory,
            IChargeLinksRepository chargeLinksRepository,
            IBusinessValidator<ChargeLinkDto> businessValidator,
            IUnitOfWork unitOfWork)
        {
            _chargeLinksReceiptService = chargeLinksReceiptService;
            _chargeLinkFactory = chargeLinkFactory;
            _chargeLinksRepository = chargeLinksRepository;
            _businessValidator = businessValidator;
            _unitOfWork = unitOfWork;
        }

        public async Task HandleAsync(ChargeLinksReceivedEvent chargeLinksReceivedEvent)
        {
            ArgumentNullException.ThrowIfNull(chargeLinksReceivedEvent);

            var operationsToBeRejected = new List<ChargeLinkDto>();
            var rejectionRules = new List<IValidationRuleContainer>();
            var operationsToBeConfirmed = new List<ChargeLinkDto>();

            var operations = chargeLinksReceivedEvent.ChargeLinksCommand.ChargeLinksOperations.ToArray();

            for (var i = 0; i < operations.Length; i++)
            {
                var operation = operations[i];

                var validationResult = await _businessValidator.ValidateAsync(operation).ConfigureAwait(false);
                if (validationResult.IsFailed)
                {
                    operationsToBeRejected = operations[i..].ToList();
                    rejectionRules.AddRange(validationResult.InvalidRules);
                    rejectionRules.AddRange(operationsToBeRejected.Skip(1)
                        .Select(toBeRejected =>
                            new OperationValidationRuleContainer(
                                new PreviousChargeLinkOperationsMustBeValidRule(operation.OperationId, toBeRejected),
                                operation.OperationId)));
                    break;
                }

                await HandleCreateEventAsync(operation).ConfigureAwait(false);
                operationsToBeConfirmed.Add(operation);
            }

            await _unitOfWork.SaveChangesAsync().ConfigureAwait(false);

            var document = chargeLinksReceivedEvent.ChargeLinksCommand.Document;
            await RejectInvalidOperationsAsync(operationsToBeRejected, document, rejectionRules).ConfigureAwait(false);
            await AcceptValidOperationsAsync(operationsToBeConfirmed, document).ConfigureAwait(false);
        }

        private async Task HandleCreateEventAsync(ChargeLinkDto chargeLinkDto)
        {
            var chargeLinks = await _chargeLinkFactory.CreateAsync(chargeLinkDto).ConfigureAwait(false);
            await _chargeLinksRepository.AddAsync(chargeLinks).ConfigureAwait(false);
        }

        private async Task RejectInvalidOperationsAsync(
            IReadOnlyCollection<ChargeLinkDto> operationsToBeRejected,
            DocumentDto document,
            IList<IValidationRuleContainer> rejectionRules)
        {
            if (operationsToBeRejected.Any())
            {
                await _chargeLinksReceiptService.RejectAsync(
                        new ChargeLinksCommand(document, operationsToBeRejected),
                        ValidationResult.CreateFailure(rejectionRules))
                    .ConfigureAwait(false);
            }
        }

        private async Task AcceptValidOperationsAsync(
            IReadOnlyCollection<ChargeLinkDto> operationsToBeConfirmed,
            DocumentDto document)
        {
            if (operationsToBeConfirmed.Any())
            {
                await _chargeLinksReceiptService.AcceptAsync(
                        new ChargeLinksCommand(document, operationsToBeConfirmed))
                    .ConfigureAwait(false);
            }
        }
    }
}
