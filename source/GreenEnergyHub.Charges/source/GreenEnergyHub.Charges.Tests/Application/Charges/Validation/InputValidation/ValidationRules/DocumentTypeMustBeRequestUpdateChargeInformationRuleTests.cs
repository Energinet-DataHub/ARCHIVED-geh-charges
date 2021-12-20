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

namespace GreenEnergyHub.Charges.Tests.Application.Charges.Validation.InputValidation.ValidationRules
{
    [UnitTest]
    public class DocumentTypeMustBeRequestUpdateChargeInformationRuleTests
    {
        [Theory]
        [InlineAutoMoqData(DocumentType.Unknown, false)]
        [InlineAutoMoqData(DocumentType.RequestUpdateChargeInformation, true)]
        public void DocumentTypeMustBeRequestUpdateChargeInformation_Test(
            DocumentType documentType,
            bool expected,
            ChargeCommand command)
        {
            command.Document.Type = documentType;
            var sut = new DocumentTypeMustBeRequestUpdateChargeInformationRule(command);
            Assert.Equal(expected, sut.IsValid);
        }

        [Theory]
        [InlineAutoDomainData]
        public void ValidationRuleIdentifier_ShouldBe_EqualTo(ChargeCommand command)
        {
            var sut = new DocumentTypeMustBeRequestUpdateChargeInformationRule(command);
            sut.ValidationError.ValidationRuleIdentifier.Should()
                .Be(ValidationRuleIdentifier.DocumentTypeMustBeRequestUpdateChargeInformation);
        }

        [Theory]
        [InlineAutoDomainData]
        public void ValidationRuleIdentifier_ShouldContain_RequiredErrorMessageParameterTypes(ChargeCommand command)
        {
            // Arrange
            // Act
            var sut = new DocumentTypeMustBeRequestUpdateChargeInformationRule(command);

            // Assert
            sut.ValidationError.ValidationErrorMessageParameters
                .Select(x => x.ParameterType)
                .Should().Contain(ValidationErrorMessageParameterType.DocumentType);
            sut.ValidationError.ValidationErrorMessageParameters
                .Select(x => x.ParameterType)
                .Should().Contain(ValidationErrorMessageParameterType.EnergyBusinessProcess);
        }

        [Theory]
        [InlineAutoDomainData]
        public void ValidationRuleIdentifier_ShouldBe_RequiredErrorMessageParameters(ChargeCommand command)
        {
            // Arrange
            // Act
            var sut = new DocumentTypeMustBeRequestUpdateChargeInformationRule(command);

            // Assert
            sut.ValidationError.ValidationErrorMessageParameters
                .Single(x => x.ParameterType == ValidationErrorMessageParameterType.DocumentType)
                .MessageParameter.Should().Be(command.Document.Type.ToString());
            sut.ValidationError.ValidationErrorMessageParameters
                .Single(x => x.ParameterType == ValidationErrorMessageParameterType.EnergyBusinessProcess)
                .MessageParameter.Should().Be("todo");
        }
    }
}
