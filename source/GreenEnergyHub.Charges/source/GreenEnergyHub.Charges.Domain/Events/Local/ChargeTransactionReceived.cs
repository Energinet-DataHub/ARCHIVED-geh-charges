using GreenEnergyHub.Charges.Domain.ChangeOfCharges.Transaction;
using NodaTime;

namespace GreenEnergyHub.Charges.Domain.Events.Local
{
    public class ChargeTransactionReceived : IEvent
    {
        public ChargeTransactionReceived(Instant receivedTime, string correlationId, ChangeOfChargesTransaction transaction)
        {
            ReceivedTime = receivedTime;
            CorrelationId = correlationId;
            Transaction = transaction;
        }

        public Instant ReceivedTime { get; }

        public string CorrelationId { get; }

        public ChangeOfChargesTransaction Transaction { get; }
    }
}
