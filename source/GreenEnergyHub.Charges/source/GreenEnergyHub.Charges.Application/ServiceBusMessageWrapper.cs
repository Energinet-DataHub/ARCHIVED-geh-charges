using GreenEnergyHub.Charges.Domain.ChangeOfCharges.Transaction;

namespace GreenEnergyHub.Charges.Application
{
    public class ServiceBusMessageWrapper
    {
        public ChangeOfChargesTransaction Transaction { get; set; } = new ();
    }
}
