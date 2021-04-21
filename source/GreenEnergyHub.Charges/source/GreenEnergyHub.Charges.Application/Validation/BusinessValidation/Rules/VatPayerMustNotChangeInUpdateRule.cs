using GreenEnergyHub.Charges.Domain;
using GreenEnergyHub.Charges.Domain.ChangeOfCharges.Transaction;

namespace GreenEnergyHub.Charges.Application.Validation.BusinessValidation.Rules
{
    public class VatPayerMustNotChangeInUpdateRule : IBusinessValidationRule
    {
        private readonly ChargeCommand _command;
        private readonly Charge _charge;

        public VatPayerMustNotChangeInUpdateRule(ChargeCommand command, Charge charge)
        {
            _command = command;
            _charge = charge;
        }

        public bool IsValid => _command!.MktActivityRecord!.ChargeType!.VatPayer == _charge.VatPayer;
    }
}
