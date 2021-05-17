using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using GreenEnergyHub.Charges.Application.Validation.InputValidation.ValidationRules;
using GreenEnergyHub.Charges.Domain.ChangeOfCharges.Transaction;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Application.Validation.InputValidation.ValidationRules
{
    [UnitTest]
    public class ResolutionTariffValidationRuleTests
    {
        [Theory]
        [InlineAutoMoqData(Resolution.Unknown, false)]
        [InlineAutoMoqData(Resolution.P1D, true)]
        [InlineAutoMoqData(Resolution.P1M, false)]
        [InlineAutoMoqData(Resolution.PT1H, true)]
        [InlineAutoMoqData(Resolution.PT15M, true)]
        public void ResolutionTariffValidationRule_WithTariffType_Test(
            Resolution resolution,
            bool expected,
            [NotNull] ChargeCommand command)
        {
            command.ChargeOperation.Type = ChargeType.Tariff;
            command.ChargeOperation.Resolution = resolution;
            var sut = new ResolutionTariffValidationRule(command);
            sut.IsValid.Should().Be(expected);
        }

        [Theory]
        [InlineAutoMoqData(Resolution.Unknown, true)]
        [InlineAutoMoqData(Resolution.P1D, true)]
        [InlineAutoMoqData(Resolution.P1M, true)]
        [InlineAutoMoqData(Resolution.PT1H, true)]
        [InlineAutoMoqData(Resolution.PT15M, true)]
        public void ResolutionTariffValidationRule_WithSubscriptionType_Test(
            Resolution resolution,
            bool expected,
            [NotNull] ChargeCommand command)
        {
            command.ChargeOperation.Type = ChargeType.Subscription;
            command.ChargeOperation.Resolution = resolution;
            var sut = new ResolutionTariffValidationRule(command);
            sut.IsValid.Should().Be(expected);
        }

        [Theory]
        [InlineAutoMoqData(Resolution.Unknown, true)]
        [InlineAutoMoqData(Resolution.P1D, true)]
        [InlineAutoMoqData(Resolution.P1M, true)]
        [InlineAutoMoqData(Resolution.PT1H, true)]
        [InlineAutoMoqData(Resolution.PT15M, true)]
        public void ResolutionTariffValidationRule_WithFeeType_Test(
            Resolution resolution,
            bool expected,
            [NotNull] ChargeCommand command)
        {
            command.ChargeOperation.Type = ChargeType.Fee;
            command.ChargeOperation.Resolution = resolution;
            var sut = new ResolutionTariffValidationRule(command);
            sut.IsValid.Should().Be(expected);
        }
    }
}
