using System.Collections.Generic;
using GreenEnergyHub.Charges.Domain.ChangeOfCharges.Message;
using GreenEnergyHub.Charges.Domain.ChangeOfCharges.Transaction;

namespace GreenEnergyHub.Charges.Tests.Builders
{
    public class ChangeOfChargesMessageBuilder
    {
        private readonly List<ChangeOfChargesTransaction> _transactions = new ();

        public ChangeOfChargesMessageBuilder WithTransaction(ChangeOfChargesTransaction transaction)
        {
            _transactions.Add(transaction);
            return this;
        }

        public ChangeOfChargesMessage Build()
        {
            var changeOfChargesMessage = new ChangeOfChargesMessage();
            changeOfChargesMessage.Transactions.AddRange(_transactions);
            return changeOfChargesMessage;
        }
    }
}
