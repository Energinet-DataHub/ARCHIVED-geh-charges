using GreenEnergyHub.Charges.Domain.ChangeOfCharges.Transaction;

namespace GreenEnergyHub.Charges.Application
{
    public abstract class ServiceBusMessageWrapper
    {
        protected ServiceBusMessageWrapper(ChangeOfChargesTransaction transaction)
        {
            Transaction = transaction;
        }

        public ChangeOfChargesTransaction Transaction { get; }
    }
}
