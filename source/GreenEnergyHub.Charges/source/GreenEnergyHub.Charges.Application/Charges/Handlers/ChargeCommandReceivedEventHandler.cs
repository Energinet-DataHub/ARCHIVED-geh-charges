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
using GreenEnergyHub.Charges.Application.Charges.Acknowledgement;
using GreenEnergyHub.Charges.Core.DateTime;
using GreenEnergyHub.Charges.Domain.Charges;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommandReceivedEvents;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommands;
using GreenEnergyHub.Charges.Domain.Dtos.Validation;

namespace GreenEnergyHub.Charges.Application.Charges.Handlers
{
    public class ChargeCommandReceivedEventHandler : IChargeCommandReceivedEventHandler
    {
        private readonly IChargeCommandReceiptService _chargeCommandReceiptService;
        private readonly IValidator<ChargeCommand> _validator;
        private readonly IChargeRepository _chargeRepository;
        private readonly IChargeFactory _chargeFactory;

        public ChargeCommandReceivedEventHandler(
            IChargeCommandReceiptService chargeCommandReceiptService,
            IValidator<ChargeCommand> validator,
            IChargeRepository chargeRepository,
            IChargeFactory chargeFactory)
        {
            _chargeCommandReceiptService = chargeCommandReceiptService;
            _validator = validator;
            _chargeRepository = chargeRepository;
            _chargeFactory = chargeFactory;
        }

        public async Task HandleAsync(ChargeCommandReceivedEvent commandReceivedEvent)
        {
            if (commandReceivedEvent == null) throw new ArgumentNullException(nameof(commandReceivedEvent));

            // input validation
            var inputValidationResult = _validator.InputValidate(commandReceivedEvent.Command);
            if (inputValidationResult.IsFailed)
            {
                await _chargeCommandReceiptService
                    .RejectAsync(commandReceivedEvent.Command, inputValidationResult).ConfigureAwait(false);
                return;
            }

            // business validation
            var businessValidationResult = await _validator
                .BusinessValidateAsync(commandReceivedEvent.Command).ConfigureAwait(false);
            if (businessValidationResult.IsFailed)
            {
                await _chargeCommandReceiptService.RejectAsync(
                    commandReceivedEvent.Command, businessValidationResult).ConfigureAwait(false);
                return;
            }

            // get charges
            var existingCharge = await GetChargeAsync(commandReceivedEvent.Command).ConfigureAwait(false);

            // is create, update or stop?
            var operationType = GetOperationType(commandReceivedEvent.Command, existingCharge);

            // create flow
            if (operationType == OperationType.Create)
            {
                var charge = await _chargeFactory
                    .CreateFromCommandAsync(commandReceivedEvent.Command)
                    .ConfigureAwait(false);
                await _chargeRepository.StoreChargeAsync(charge).ConfigureAwait(false);
            }

            // update flow
            if (operationType == OperationType.Update)
            {
                if (existingCharge == null)
                    throw new InvalidOperationException("Could not update charge period. Charge is null");

                var newChargePeriod = new ChargePeriod(
                    Guid.NewGuid(),
                    commandReceivedEvent.Command.ChargeOperation.ChargeName,
                    commandReceivedEvent.Command.ChargeOperation.ChargeDescription,
                    commandReceivedEvent.Command.ChargeOperation.VatClassification,
                    commandReceivedEvent.Command.ChargeOperation.TransparentInvoicing,
                    commandReceivedEvent.Command.ChargeOperation.StartDateTime,
                    commandReceivedEvent.Command.ChargeOperation.EndDateTime.TimeOrEndDefault());

                var previousPeriods = existingCharge.Periods.Where(p =>
                    p.EndDateTime <= newChargePeriod.StartDateTime);

                var updatedChargePeriods = new List<ChargePeriod>();
                updatedChargePeriods.AddRange(previousPeriods);

                var overlappingPeriod = existingCharge.Periods.SingleOrDefault(p =>
                    p.EndDateTime >= newChargePeriod.EndDateTime);

                if (overlappingPeriod != null)
                {
                    var updatedOverlappingPeriod = new ChargePeriod(
                        overlappingPeriod.Id,
                        overlappingPeriod.Name,
                        overlappingPeriod.Description,
                        overlappingPeriod.VatClassification,
                        overlappingPeriod.TransparentInvoicing,
                        overlappingPeriod.StartDateTime,
                        newChargePeriod.StartDateTime);
                    updatedChargePeriods.Add(updatedOverlappingPeriod);
                }

                updatedChargePeriods.Add(newChargePeriod);

                var updatedCharge = new Charge(
                    existingCharge.Id,
                    existingCharge.SenderProvidedChargeId,
                    existingCharge.OwnerId,
                    existingCharge.Type,
                    existingCharge.Resolution,
                    existingCharge.TaxIndicator,
                    existingCharge.Points.ToList(),
                    updatedChargePeriods);

                await _chargeRepository.UpdateChargeAsync(updatedCharge).ConfigureAwait(false);
            }

            // stop flow
            if (operationType == OperationType.Stop)
            {
                // new stop flow
            }

            await _chargeCommandReceiptService.AcceptAsync(commandReceivedEvent.Command).ConfigureAwait(false);
        }

        private static OperationType GetOperationType(ChargeCommand command, Charge? charge)
        {
            if (command.ChargeOperation.StartDateTime == command.ChargeOperation.EndDateTime)
            {
                return OperationType.Stop;
            }

            return charge != null ? OperationType.Update : OperationType.Create;
        }

        private async Task<Charge?> GetChargeAsync(ChargeCommand command)
        {
            var chargeIdentifier = new ChargeIdentifier(
                command.ChargeOperation.ChargeId,
                command.ChargeOperation.ChargeOwner,
                command.ChargeOperation.Type);

            return await _chargeRepository.GetOrNullAsync(chargeIdentifier).ConfigureAwait(false);
        }
    }

    // Internal, so far...
    internal enum OperationType
    {
        Create = 0,
        Update = 1,
        Stop = 2,
    }
}
