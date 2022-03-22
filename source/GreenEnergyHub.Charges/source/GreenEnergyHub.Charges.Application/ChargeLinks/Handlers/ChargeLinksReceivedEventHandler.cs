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
using GreenEnergyHub.Charges.Application.ChargeLinks.Services;
using GreenEnergyHub.Charges.Application.Persistence;
using GreenEnergyHub.Charges.Domain.ChargeLinks;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeLinksCommands;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeLinksReceivedEvents;
using GreenEnergyHub.Charges.Domain.Dtos.Validation;

namespace GreenEnergyHub.Charges.Application.ChargeLinks.Handlers
{
    public class ChargeLinksReceivedEventHandler : IChargeLinksReceivedEventHandler
    {
        private readonly IChargeLinksReceiptService _chargeLinksReceiptService;
        private readonly IChargeLinkFactory _chargeLinkFactory;
        private readonly IChargeLinksRepository _chargeLinksRepository;
        private readonly IValidator<ChargeLinksCommand> _validator;
        private readonly IUnitOfWork _unitOfWork;

        public ChargeLinksReceivedEventHandler(
            IChargeLinksReceiptService chargeLinksReceiptService,
            IChargeLinkFactory chargeLinkFactory,
            IChargeLinksRepository chargeLinksRepository,
            IValidator<ChargeLinksCommand> validator,
            IUnitOfWork unitOfWork)
        {
            _chargeLinksReceiptService = chargeLinksReceiptService;
            _chargeLinkFactory = chargeLinkFactory;
            _chargeLinksRepository = chargeLinksRepository;
            _validator = validator;
            _unitOfWork = unitOfWork;
        }

        public async Task HandleAsync(ChargeLinksReceivedEvent chargeLinksReceivedEvent)
        {
            var inputValidationResult = _validator.InputValidate(chargeLinksReceivedEvent.ChargeLinksCommand);
            if (inputValidationResult.IsFailed)
            {
                await _chargeLinksReceiptService
                    .RejectAsync(chargeLinksReceivedEvent.ChargeLinksCommand, inputValidationResult)
                    .ConfigureAwait(false);
                return;
            }

            var businessValidationResult = await _validator
                .BusinessValidateAsync(chargeLinksReceivedEvent.ChargeLinksCommand).ConfigureAwait(false);
            if (businessValidationResult.IsFailed)
            {
                await _chargeLinksReceiptService
                    .RejectAsync(chargeLinksReceivedEvent.ChargeLinksCommand, businessValidationResult)
                    .ConfigureAwait(false);
                return;
            }

            var chargeLinks = await _chargeLinkFactory.CreateAsync(chargeLinksReceivedEvent).ConfigureAwait(false);
            await _chargeLinksRepository.AddRangeAsync(chargeLinks).ConfigureAwait(false);
            await _chargeLinksReceiptService
                .AcceptAsync(chargeLinksReceivedEvent.ChargeLinksCommand)
                .ConfigureAwait(false);

            await _unitOfWork.SaveChangesAsync().ConfigureAwait(false);
        }
    }
}
