namespace GreenEnergyHub.Charges.Domain.ChangeOfCharges.Result
{
    public class ChangeOfChargesMessageResult
    {
        public bool IsSucceeded { get; init; }

        public static ChangeOfChargesMessageResult CreateSuccess()
        {
            return new () { IsSucceeded = true, };
        }
    }
}
