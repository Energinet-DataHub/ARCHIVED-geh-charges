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

using AutoFixture.Xunit2;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommands.Validation;
using GreenEnergyHub.Charges.Infrastructure.Core.Cim.ValidationErrors;
using GreenEnergyHub.Charges.TestCore.Attributes;
using Moq;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.MessageHub.Models.AvailableChargeReceiptData
{
    // TODO BJARKE
    /*[UnitTest]
    public class CimValidationErrorTextFactoryTests
    {
        [Theory]
        [InlineAutoMoqData]
        public void Create_WhenTwoMergeFields_ReturnsExpectedText(
            ValidationRuleIdentifier anyValidationRuleIdentifier,
            string firstParameterValue,
            string secondParameterValue,
            ValidationErrorMessageParameterType firstParameterType,
            ValidationErrorMessageParameterType secondParameterType,
            [Frozen] Mock<ICimValidationErrorMessageProvider> cimValidationErrorMessageProvider,
            CimValidationErrorTextFactory sut)
        {
            // Arrange
            var cimErrorMessageWithTwoMergeFields = "First = {{" + firstParameterType + "}}, second = {{" + secondParameterType + "}}";
            var expected = $"First = {firstParameterValue}, second = {secondParameterValue}";
            var error = new ValidationError(
                anyValidationRuleIdentifier,
                new ValidationErrorMessageParameter(firstParameterValue, firstParameterType),
                new ValidationErrorMessageParameter(secondParameterValue, secondParameterType));
            cimValidationErrorMessageProvider
                .Setup(f => f.GetCimValidationErrorMessage(anyValidationRuleIdentifier))
                .Returns(cimErrorMessageWithTwoMergeFields);

            // Act
            var actual = sut.Create(error);

            // Assert
            actual.Should().Be(expected);
        }
    }*/
}
