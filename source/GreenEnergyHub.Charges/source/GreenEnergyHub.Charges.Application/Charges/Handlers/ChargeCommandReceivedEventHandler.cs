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
using GreenEnergyHub.Charges.Application.Charges.Acknowledgement;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommandReceivedEvents;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommands;
using GreenEnergyHub.Charges.Domain.Dtos.SharedDtos;
using GreenEnergyHub.Charges.Domain.Dtos.Validation;

namespace GreenEnergyHub.Charges.Application.Charges.Handlers
{
    public class ChargeCommandReceivedEventHandler : IChargeCommandReceivedEventHandler
    {
        private readonly IChargeInformationEventHandler _chargeInformationEventHandler;
        private readonly IChargePriceEventHandler _chargePriceEventHandler;
        private readonly IDocumentValidator<ChargeCommand> _documentValidator;
        private readonly IChargeCommandReceiptService _chargeCommandReceiptService;

        public ChargeCommandReceivedEventHandler(
            IChargeInformationEventHandler chargeInformationEventHandler,
            IChargePriceEventHandler chargePriceEventHandler,
            IDocumentValidator<ChargeCommand> documentValidator,
            IChargeCommandReceiptService chargeCommandReceiptService)
        {
            _chargeInformationEventHandler = chargeInformationEventHandler;
            _chargePriceEventHandler = chargePriceEventHandler;
            _documentValidator = documentValidator;
            _chargeCommandReceiptService = chargeCommandReceiptService;
        }

        public async Task HandleAsync(ChargeCommandReceivedEvent commandReceivedEvent)
        {
            var documentValidationResult = await _documentValidator.ValidateAsync(commandReceivedEvent.Command).ConfigureAwait(false);
            if (documentValidationResult.IsFailed)
            {
                await _chargeCommandReceiptService
                    .RejectAsync(commandReceivedEvent.Command, documentValidationResult).ConfigureAwait(false);
                return;
            }

            switch (commandReceivedEvent.Command.Document.BusinessReasonCode)
            {
                case BusinessReasonCode.UpdateChargePrices:
                    await _chargePriceEventHandler.HandleAsync(commandReceivedEvent).ConfigureAwait(false);
                    break;
                case BusinessReasonCode.UpdateChargeInformation:
                    await _chargeInformationEventHandler.HandleAsync(commandReceivedEvent).ConfigureAwait(false);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(
                        $"Invalid BusinessReasonCode {commandReceivedEvent.Command.Document.BusinessReasonCode}");
            }
        }
    }
}
