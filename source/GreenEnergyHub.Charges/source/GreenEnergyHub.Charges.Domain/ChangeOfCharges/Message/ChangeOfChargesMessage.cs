using System.Collections.Generic;
using GreenEnergyHub.Charges.Domain.ChangeOfCharges.Transaction;

namespace GreenEnergyHub.Charges.Domain.ChangeOfCharges.Message
{
    public class ChangeOfChargesMessage
    {
        public List<ChangeOfChargesTransaction> Transactions { get; } = new ();
    }
}
