using System.Threading.Tasks;
using GreenEnergyHub.Charges.Domain.ChangeOfCharges.Message;
using GreenEnergyHub.Charges.Domain.ChangeOfCharges.Result;
using GreenEnergyHub.Charges.Domain.ChangeOfCharges.Transaction;

namespace GreenEnergyHub.Charges.Application.ChangeOfCharges
{
    public class ChangeOfChargesMessageHandler : IChangeOfChargesMessageHandler
    {
        public Task<ChangeOfChargesMessageResult> HandleAsync(ChangeOfChargesMessage message)
        {
            throw new System.NotImplementedException();
        }
    }
}
