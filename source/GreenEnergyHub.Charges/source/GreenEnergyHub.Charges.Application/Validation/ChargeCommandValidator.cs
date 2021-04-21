using System.Threading.Tasks;
using GreenEnergyHub.Charges.Application.ChangeOfCharges;
using GreenEnergyHub.Charges.Application.Validation.BusinessValidation;
using GreenEnergyHub.Charges.Domain.ChangeOfCharges.Transaction;

namespace GreenEnergyHub.Charges.Application.Validation
{
    public class ChargeCommandValidator : IChargeCommandValidator
    {
        private readonly IChargeCommandBusinessValidator _chargeCommandBusinessValidator;
        private readonly IChangeOfChargeTransactionInputValidator _chargeTransactionInputValidator;

        public ChargeCommandValidator(
            IChangeOfChargeTransactionInputValidator chargeTransactionInputValidator,
            IChargeCommandBusinessValidator chargeCommandBusinessValidator)
        {
            _chargeTransactionInputValidator = chargeTransactionInputValidator;
            _chargeCommandBusinessValidator = chargeCommandBusinessValidator;
        }

        public async Task<ChargeCommandValidationResult> ValidateAsync(ChargeCommand command)
        {
            var inputValidationResult =
                await _chargeTransactionInputValidator.ValidateAsync(command).ConfigureAwait(false);
            if (inputValidationResult.IsFailed) return inputValidationResult;

            var businessValidationResult =
                await _chargeCommandBusinessValidator.ValidateAsync(command).ConfigureAwait(false);
            return businessValidationResult;
        }
    }
}
