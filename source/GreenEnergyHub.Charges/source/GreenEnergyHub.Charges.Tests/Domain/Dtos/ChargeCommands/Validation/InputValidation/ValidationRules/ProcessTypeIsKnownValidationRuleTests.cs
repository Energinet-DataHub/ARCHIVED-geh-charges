// Copyright 2020 Energinet DataHub A/S
//
// Licensed under the Apache License, Version 2.0 (the "License2");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System.Linq;
using FluentAssertions;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommands;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommands.Validation;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommands.Validation.InputValidation.ValidationRules;
using GreenEnergyHub.Charges.Domain.MarketParticipants;
using GreenEnergyHub.Charges.TestCore.Attributes;
using GreenEnergyHub.TestHelpers;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Domain.Dtos.ChargeCommands.Validation.InputValidation.ValidationRules
{
    [UnitTest]
    public class ProcessTypeIsKnownValidationRuleTests
    {
        [Theory]
        [InlineAutoMoqData(BusinessReasonCode.Unknown, false)]
        [InlineAutoMoqData(BusinessReasonCode.UpdateChargeInformation, true)]
        [InlineAutoMoqData(-1, false)]
        public void ProcessTypeIsKnownValidationRule_Test(
            BusinessReasonCode businessReasonCode,
            bool expected,
            ChargeCommand command)
        {
            command.Document.BusinessReasonCode = businessReasonCode;
            var sut = new ProcessTypeIsKnownValidationRule(command);
            Assert.Equal(expected, sut.IsValid);
        }

        [Theory]
        [InlineAutoDomainData]
        public void ValidationRuleIdentifier_ShouldBe_EqualTo(ChargeCommand command)
        {
            var sut = new ProcessTypeIsKnownValidationRule(command);
            sut.ValidationError.ValidationRuleIdentifier
                .Should().Be(ValidationRuleIdentifier.ProcessTypeIsKnownValidation);
        }

        [Theory]
        [InlineAutoDomainData]
        public void ValidationErrorMessageParameters_ShouldContain_RequiredErrorMessageParameterTypes(ChargeCommand command)
        {
            var sut = new ProcessTypeIsKnownValidationRule(command);
            sut.ValidationError.ValidationErrorMessageParameters
                .Select(x => x.ParameterType)
                .Should().Contain(ValidationErrorMessageParameterType.DocumentId);
        }

        [Theory]
        [InlineAutoDomainData]
        public void MessageParameter_ShouldBe_RequiredErrorMessageParameters(ChargeCommand command)
        {
            var sut = new ProcessTypeIsKnownValidationRule(command);
            sut.ValidationError.ValidationErrorMessageParameters
                .Single(x => x.ParameterType == ValidationErrorMessageParameterType.DocumentId)
                .ParameterValue.Should().Be(command.Document.Id); // MeteringPointId provided in Excel sheet for VR.009 does not make sense
        }
    }
}
