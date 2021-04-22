namespace GreenEnergyHub.Charges.Application.Validation.BusinessValidation
{
    public interface IBusinessValidationRuleSet
    {
        ChargeCommandValidationResult Validate();
    }
}
