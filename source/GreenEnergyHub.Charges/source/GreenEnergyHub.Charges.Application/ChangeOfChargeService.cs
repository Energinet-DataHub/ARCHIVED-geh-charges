using System.Threading.Tasks;
using GreenEnergyHub.Charges.Application.ChangeOfCharges;
using GreenEnergyHub.Charges.Domain.ChangeOfCharges.Transaction;

namespace GreenEnergyHub.Charges.Application
{
    public class ChangeOfChargeService : IChangeOfChargeService
    {
        private readonly IChangeOfChargeTransactionInputValidator _changeOfChargeTransactionInputValidator;
        private IChangeOfChargeReceiptService _changeOfChargeReceiptService;

        public ChangeOfChargeService(IChangeOfChargeTransactionInputValidator changeOfChargeTransactionInputValidator, IChangeOfChargeReceiptService changeOfChargeReceiptService)
        {
            _changeOfChargeTransactionInputValidator = changeOfChargeTransactionInputValidator;
            _changeOfChargeReceiptService = changeOfChargeReceiptService;
        }

        public async Task HandleAsync(ChangeOfChargesTransaction transaction)
        {
            var validationResult = await _changeOfChargeTransactionInputValidator.ValidateAsync(transaction).ConfigureAwait(false);
            if (!validationResult.IsSucceeded)
            {
                await _changeOfChargeReceiptService.RejectAsync(validationResult).ConfigureAwait(false);
                return;
            }
        }
    }
}
