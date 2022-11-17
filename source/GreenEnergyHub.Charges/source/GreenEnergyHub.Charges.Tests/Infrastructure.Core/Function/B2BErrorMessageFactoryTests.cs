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

using FluentAssertions;
using GreenEnergyHub.Charges.Infrastructure.Core.Function;
using GreenEnergyHub.Charges.TestCore.TestHelpers;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Infrastructure.Core.Function
{
    [UnitTest]
    public class B2BErrorMessageFactoryTests
    {
        [Fact]
        public void Create_WhenFactoryCreateWithActorIsNotWhoTheyClaimToBe_ReturnCorrectErrorMessage()
        {
            // Arrange
            var expectedCode = B2BErrorCodeConstants.SenderIsNotAuthorized;
            var expectedMessage = ErrorMessageConstants.ActorIsNotWhoTheyClaimToBeErrorMessage;

            // Act
            var sut = B2BErrorMessageFactory.CreateSenderNotAuthorizedErrorMessage();

            // Assert
            sut.Code.Should().Be(expectedCode);
            Assert.Equal(expectedMessage, sut.Message, ignoreLineEndingDifferences: true);
        }

        [Fact]
        public void Create_WhenFactoryCreateWithIsEmptyOrWhitespace_ReturnCorrectErrorMessage()
        {
            // Arrange
            var errorIdentifier = "mRID";
            var expectedCode = B2BErrorCodeConstants.SyntaxValidation;
            var expectedMessage = string.Format(ErrorMessageConstants.SyntaxValidationErrorMessage + "\r\n" + ErrorMessageConstants.ValueIsEmptyOrIsWhiteSpace, errorIdentifier);

            // Act
            var sut = B2BErrorMessageFactory.CreateIsEmptyOrWhitespaceErrorMessage(errorIdentifier);

            // Assert
            sut.Code.Should().Be(expectedCode);
            Assert.Equal(expectedMessage, sut.Message, ignoreLineEndingDifferences: true);
        }

        [Fact]
        public void Create_WhenFactoryCreateWithUnsupportedEnum_ReturnCorrectErrorMessage()
        {
            // Arrange
            var errorIdentifier = "mRID";
            var errorContent = "D17";
            var expectedCode = B2BErrorCodeConstants.SyntaxValidation;
            var expectedMessage = string.Format(ErrorMessageConstants.SyntaxValidationErrorMessage + "\r\n" + ErrorMessageConstants.UnsupportedEnumErrorMessage, errorIdentifier, errorContent);

            // Act
            var sut = B2BErrorMessageFactory.CreateCouldNotMapEnumErrorMessage(errorIdentifier, errorContent);

            // Assert
            sut.Code.Should().Be(expectedCode);
            Assert.Equal(expectedMessage, sut.Message, ignoreLineEndingDifferences: true);
        }
    }
}
