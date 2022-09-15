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
using GreenEnergyHub.Charges.Domain.Dtos.ChargeInformationCommandReceivedEvents;
using GreenEnergyHub.Charges.Domain.Dtos.Validation;

namespace GreenEnergyHub.Charges.Application.Charges.Handlers
{
    public class ChargeInformationCommandReceivedEventHandler : IChargeCommandReceivedEventHandler
    {
        private readonly IChargeInformationOperationsHandler _chargeInformationOperationsHandler;
        private readonly IDocumentValidator _documentValidator;
        private readonly IChargeCommandReceiptService _chargeCommandReceiptService;

        public ChargeInformationCommandReceivedEventHandler(
            IChargeInformationOperationsHandler chargeInformationOperationsHandler,
            IDocumentValidator documentValidator,
            IChargeCommandReceiptService chargeCommandReceiptService)
        {
            _chargeInformationOperationsHandler = chargeInformationOperationsHandler;
            _documentValidator = documentValidator;
            _chargeCommandReceiptService = chargeCommandReceiptService;
        }

        public async Task HandleAsync(ChargeInformationCommandReceivedEvent chargeInformationCommandReceivedEvent)
        {
            var documentValidationResult = await _documentValidator.ValidateAsync(chargeInformationCommandReceivedEvent.Command).ConfigureAwait(false);
            if (documentValidationResult.IsFailed)
            {
                await _chargeCommandReceiptService
                    .RejectAsync(chargeInformationCommandReceivedEvent.Command, documentValidationResult).ConfigureAwait(false);
                return;
            }

            await _chargeInformationOperationsHandler.HandleAsync(chargeInformationCommandReceivedEvent).ConfigureAwait(false);
        }
    }
}
