using System.Threading.Tasks;
using GreenEnergyHub.Charges.Application.Validation;
using GreenEnergyHub.Charges.Domain.ChangeOfCharges.Transaction;

namespace GreenEnergyHub.Charges.Application
{
    public interface IChargeCommandAcknowledgementService
    {
        Task RejectAsync(ChargeCommandValidationResult validationResult);

        Task AcceptAsync(ChargeCommand command);
    }
}
