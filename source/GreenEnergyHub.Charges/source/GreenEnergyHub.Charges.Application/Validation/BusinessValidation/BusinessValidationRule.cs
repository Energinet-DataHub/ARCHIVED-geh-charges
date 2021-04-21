namespace GreenEnergyHub.Charges.Application.Validation
{
    public class BusinessValidationRule : IBusinessValidationRule
    {
        public BusinessValidationRule(bool isValid)
        {
            IsValid = isValid;
        }

        public bool IsValid { get; }
    }
}
