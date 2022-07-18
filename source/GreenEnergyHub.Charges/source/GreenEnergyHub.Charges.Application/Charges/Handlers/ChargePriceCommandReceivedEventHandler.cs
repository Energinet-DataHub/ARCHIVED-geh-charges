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

using System.Threading.Tasks;
using GreenEnergyHub.Charges.Application.Charges.Acknowledgement;
using GreenEnergyHub.Charges.Domain.Dtos.ChargePriceCommandReceivedEvents;
using GreenEnergyHub.Charges.Domain.Dtos.ChargePriceCommands;
using GreenEnergyHub.Charges.Domain.Dtos.Validation;

namespace GreenEnergyHub.Charges.Application.Charges.Handlers
{
    public class ChargePriceCommandReceivedEventHandler : IChargePriceCommandReceivedEventHandler
    {
        private readonly IChargePriceEventHandler _chargePriceEventHandler;
        private readonly IDocumentValidator<ChargePriceCommand> _documentValidator;
        private readonly IChargeCommandReceiptService _chargeCommandReceiptService;

        public ChargePriceCommandReceivedEventHandler(
            IChargePriceEventHandler chargePriceEventHandler,
            IDocumentValidator<ChargePriceCommand> documentValidator,
            IChargeCommandReceiptService chargeCommandReceiptService)
        {
            _chargePriceEventHandler = chargePriceEventHandler;
            _documentValidator = documentValidator;
            _chargeCommandReceiptService = chargeCommandReceiptService;
        }

        public async Task HandleAsync(ChargePriceCommandReceivedEvent chargePriceCommandReceivedEvent)
        {
            var documentValidationResult = await _documentValidator
                .ValidateAsync(chargePriceCommandReceivedEvent.Command).ConfigureAwait(false);
            if (documentValidationResult.IsFailed)
            {
                //await _chargeCommandReceiptService
                //    .RejectAsync(chargePriceCommandReceivedEvent.Command, documentValidationResult).ConfigureAwait(false);
                return;
            }

            await _chargePriceEventHandler.HandleAsync(chargePriceCommandReceivedEvent).ConfigureAwait(false);
        }
    }
}
