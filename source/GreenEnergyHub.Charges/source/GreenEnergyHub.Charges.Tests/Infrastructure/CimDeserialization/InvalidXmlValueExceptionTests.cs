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
using GreenEnergyHub.Charges.Infrastructure.CimDeserialization.MarketDocument;
using GreenEnergyHub.Charges.Infrastructure.Core.Function;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Infrastructure.CimDeserialization
{
    [UnitTest]
    public class InvalidXmlValueExceptionTests
    {
        [Fact]
        public void InvalidXmlValueException_WhenConstructorIsCalledWithEmptyContent_ThenExceptionWithCorrectMessage()
        {
            // Arrange
            const string errorIdentifier = "mRID";
            const string errorContent = "";
            var expectedErrorMessage = B2BErrorMessageFactory.CreateIsEmptyOrWhitespaceErrorMessage(errorIdentifier);

            // Act
            var exception = new InvalidXmlValueException(B2BErrorCode.IsEmptyOrWhitespaceErrorMessage, errorIdentifier, errorContent);

            // Assert
            exception.Should().BeOfType<InvalidXmlValueException>();
            exception.Message.Should().Be(expectedErrorMessage.WriteAsXmlString());
        }

        [Fact]
        public void InvalidXmlValueException_WhenConstructorIsCalledWithInvalidEnum_ThenExceptionWithCorrectMessage()
        {
            // Arrange
            const string errorIdentifier = "process.processType";
            const string errorContent = "D17";
            var expectedErrorMessage = B2BErrorMessageFactory.CreateCouldNotMapEnumErrorMessage(errorIdentifier, errorContent);

            // Act
            var exception = new InvalidXmlValueException(B2BErrorCode.CouldNotMapEnumErrorMessage, errorIdentifier, errorContent);

            // Assert
            exception.Should().BeOfType<InvalidXmlValueException>();
            exception.Message.Should().Be(expectedErrorMessage.WriteAsXmlString());
        }
    }
}
