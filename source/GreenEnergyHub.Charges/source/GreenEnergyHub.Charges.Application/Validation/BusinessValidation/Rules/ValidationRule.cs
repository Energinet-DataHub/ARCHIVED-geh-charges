namespace GreenEnergyHub.Charges.Application.Validation.BusinessValidation.Rules
{
    public enum ValidationRule
    {
        TimeLimitsNotFollowed, // VR209
        ChangingVATindicationIsNotAllowed, //VR630
        ChangingTaxTariffsIsNotAllowed, // VRXYZ
        ProcessIsMandatory, //VR009
        SenderIsMandatory, // VR150
        RecipientIsMandatory, // VR153
    }
}
