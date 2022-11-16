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

using System.ComponentModel;
using FluentAssertions;
using GreenEnergyHub.Charges.Infrastructure.Core.Function;
using GreenEnergyHub.Charges.IntegrationTest.Core.TestHelpers;
using GreenEnergyHub.Charges.TestCore.Attributes;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Infrastructure.Core.Function
{
    [UnitTest]
    public class B2BErrorMessageFactoryTests
    {
        [Theory]
        [InlineAutoMoqData(B2BErrorCode.ActorIsNotWhoTheyClaimToBeErrorMessage, "", "", B2BErrorCodeConstants.SenderIsNotAuthorized, ErrorMessageConstants.ActorIsNotWhoTheyClaimToBeErrorMessage)]
        [InlineAutoMoqData(B2BErrorCode.CouldNotMapEnumErrorMessage, "reason", "D17", B2BErrorCodeConstants.SyntaxValidation, ErrorMessageConstants.SyntaxValidationErrorMessage + "\r\n" + ErrorMessageConstants.UnsupportedEnumErrorMessage)]
        [InlineAutoMoqData(B2BErrorCode.IsEmptyOrWhitespaceErrorMessage, "mRID", "", B2BErrorCodeConstants.SyntaxValidation, ErrorMessageConstants.SyntaxValidationErrorMessage + "\r\n" + ErrorMessageConstants.ValueIsEmptyOrIsWhiteSpace)]
        public void Create_WhenFactoryCreateWithValidB2BCode_ReturnCorrectErrorMessage(
            B2BErrorCode errorCode,
            string errorIdentifier,
            string errorContent,
            string expectedCode,
            string expectedMessageUnformatted)
        {
            //Arrange
            var expectedMessageFormattet = string.Format(expectedMessageUnformatted, errorIdentifier, errorContent);
            // Act
            var sut = B2BErrorMessageFactory.Create(errorCode, errorIdentifier, errorContent);

            // Assert
            sut.Code.Should().Be(expectedCode);
            Assert.Equal(expectedMessageFormattet, sut.Message, ignoreLineEndingDifferences: true);
        }

        [Fact]
        public void Create_WhenFactoryCreateWithInvalidB2BCode_ThrowsException()
        {
            // Arrange
            var invalidB2BCode = (B2BErrorCode)0;

            // Act + Assert
            Assert.Throws<InvalidEnumArgumentException>(() => B2BErrorMessageFactory.Create(invalidB2BCode, string.Empty, string.Empty));
        }
    }
}
