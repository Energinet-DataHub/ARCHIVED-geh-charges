using GreenEnergyHub.Charges.Domain.ChangeOfCharges.Transaction;

namespace GreenEnergyHub.Charges.Domain
{
    /// <summary>
    /// TODO: Charge for now derives from ChargeCommand to get up and running quick and dirty.
    ///       I, however, still wanted to make my intentions/thoughts explicit.
    ///       Please consider making it a nice and sweet (domain) model completely decoupled from command.
    /// </summary>
    public class Charge : ChargeCommand
    {
    }
}
