using System.Diagnostics.CodeAnalysis;
using AutoFixture.Xunit2;
using GreenEnergyHub.Charges.Application.Validation;
using GreenEnergyHub.Charges.Application.Validation.InputValidation;
using GreenEnergyHub.Charges.Domain.ChangeOfCharges.Transaction;
using GreenEnergyHub.Charges.TestCore;
using Moq;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Application.Validation.InputValidation
{
    [UnitTest]
    public class ChargeCommandInputValidatorTests
    {
        [Theory]
        [InlineAutoData]
        public void Validate_WhenValidatingChargeCommand_ReturnsChargeCommandValidationResult(
            [NotNull] InputValidationRulesFactory inputValidationRulesFactory,
            [NotNull] ChargeCommand chargeCommand)
        {
            // Arrange
            var sut = new ChargeCommandInputValidator(inputValidationRulesFactory);

            // Act
            var result = sut.Validate(chargeCommand);

            // Assert
            Assert.IsType<ChargeCommandValidationResult>(result);
        }
    }
}
