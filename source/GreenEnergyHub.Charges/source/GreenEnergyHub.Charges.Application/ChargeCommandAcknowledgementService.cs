using System.Threading.Tasks;
using GreenEnergyHub.Charges.Application.Validation;

namespace GreenEnergyHub.Charges.Application
{
    public class ChargeCommandAcknowledgementService : IChargeCommandAcknowledgementService
    {
        public Task RejectAsync(ChargeCommandValidationResult validationResult)
        {
            return Task.CompletedTask;
        }
    }
}
