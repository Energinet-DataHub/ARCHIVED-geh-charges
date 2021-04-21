using System.Threading.Tasks;
using GreenEnergyHub.Charges.Application.ChangeOfCharges.Repositories;
using GreenEnergyHub.Charges.Application.Validation;
using GreenEnergyHub.Charges.Domain.ChangeOfCharges.Transaction;

namespace GreenEnergyHub.Charges.Application
{
    public class ChargeCommandHandler : IChargeCommandHandler
    {
        private readonly IChargeCommandAcknowledgementService _chargeCommandAcknowledgementService;
        private readonly IChargeCommandValidator _chargeCommandValidator;
        private readonly IChargeRepository _chargeRepository;
        private readonly IChargeFactory _chargeFactory;

        public ChargeCommandHandler(IChargeCommandAcknowledgementService chargeCommandAcknowledgementService, IChargeCommandValidator chargeCommandValidator, IChargeRepository chargeRepository, IChargeFactory chargeFactory)
        {
            _chargeCommandAcknowledgementService = chargeCommandAcknowledgementService;
            _chargeCommandValidator = chargeCommandValidator;
            _chargeRepository = chargeRepository;
            _chargeFactory = chargeFactory;
        }

        public async Task HandleAsync(ChargeCommand command)
        {
            var validationResult = await _chargeCommandValidator.ValidateAsync(command).ConfigureAwait(false);
            if (validationResult.IsFailed)
            {
                await _chargeCommandAcknowledgementService.RejectAsync(validationResult).ConfigureAwait(false);
                return;
            }

            var charge = await _chargeFactory.CreateFromCommandAsync(command).ConfigureAwait(false);
            var storageResult = await _chargeRepository.StoreChargeAsync(charge).ConfigureAwait(false);
            if (!storageResult.Success)
            {
                await _chargeCommandAcknowledgementService.RejectAsync(storageResult).ConfigureAwait(false);
            }
        }
    }
}
