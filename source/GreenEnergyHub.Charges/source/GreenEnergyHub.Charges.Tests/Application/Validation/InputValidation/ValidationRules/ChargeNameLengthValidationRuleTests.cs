using System.Diagnostics.CodeAnalysis;
using System.Linq;
using FluentAssertions;
using GreenEnergyHub.Charges.Application.Validation.InputValidation.ValidationRules;
using GreenEnergyHub.Charges.Domain.ChangeOfCharges.Transaction;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Application.Validation.InputValidation.ValidationRules
{
    [UnitTest]
    public class ChargeNameLengthValidationRuleTests
    {
        [Theory]
        [InlineAutoMoqData(49, true)]
        [InlineAutoMoqData(50, true)]
        [InlineAutoMoqData(51, false)]
        public void ChargeNameLengthValidationRule_WhenCalledWithChargeNameLength_EqualsExpectedResult(int chargeNameLength, bool expected, [NotNull] ChargeCommand command)
        {
            command.ChargeOperation.ChargeName =
                string.Join(string.Empty, Enumerable.Repeat(0, chargeNameLength).Select(n => "a"));
            var sut = new ChargeNameLengthValidationRule(command);
            sut.IsValid.Should().Be(expected);
        }
    }
}
