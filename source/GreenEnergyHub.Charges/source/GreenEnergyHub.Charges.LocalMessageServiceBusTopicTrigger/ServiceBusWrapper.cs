using GreenEnergyHub.Charges.Domain.ChangeOfCharges.Transaction;

namespace GreenEnergyHub.Charges.LocalMessageServiceBusTopicTrigger
{
    public class ServiceBusWrapper
    {
        public string Filter { get; set; }

        public ChangeOfChargesTransaction Transaction { get; set; }
    }
}
