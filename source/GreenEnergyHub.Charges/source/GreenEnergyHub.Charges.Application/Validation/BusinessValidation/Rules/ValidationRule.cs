namespace GreenEnergyHub.Charges.Application.Validation.BusinessValidation.Rules
{
    public enum ValidationRule
    {
        TimeLimitsNotFollowed, // VR209
        ChangingVATindicationIsNotAllowed, //VR630
        ChangingTaxTariffsIsNotAllowed, // VRXYZ
        Vr009,
        Vr150,
        Vr153,
    }
}
