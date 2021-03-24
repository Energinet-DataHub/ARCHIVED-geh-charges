using GreenEnergyHub.Charges.Domain.ChangeOfCharges.Transaction;
using NodaTime;

namespace GreenEnergyHub.Charges.Domain.Events.Local
{
    public class ChargeTransactionReceived : ILocalEvent
    {
        public ChargeTransactionReceived(string correlationId, ChangeOfChargesTransaction transaction)
        {
            CorrelationId = correlationId;
            Transaction = transaction;
        }

        public Instant PublishedTime { get; } = SystemClock.Instance.GetCurrentInstant();

        public string CorrelationId { get; }

        public ChangeOfChargesTransaction Transaction { get; }
    }
}
