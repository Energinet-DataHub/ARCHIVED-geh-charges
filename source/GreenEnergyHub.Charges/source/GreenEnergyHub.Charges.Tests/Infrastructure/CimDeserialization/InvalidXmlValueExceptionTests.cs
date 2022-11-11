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

using System;
using FluentAssertions;
using GreenEnergyHub.Charges.Infrastructure.CimDeserialization.MarketDocument;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Infrastructure.CimDeserialization
{
    [UnitTest]
    public class InvalidXmlValueExceptionTests
    {
        [Fact]
        public void InvalidXmlValueException_WhenConstructorIsCalled_ThenExceptionWithCorrectMessage()
        {
            // Arrange
            const string elementName = "reasoncode";
            const string detailedError = "this is the detailed error message";
            var expectedErrorMessage = $"{elementName} element contains an invalid value. {detailedError}";

            // Act & Assert
            try
            {
                throw new InvalidXmlValueException(elementName, detailedError);
            }
            catch (Exception exception)
            {
                exception.Should().BeOfType<InvalidXmlValueException>();
                exception.Message.Should().Be(expectedErrorMessage);
            }
        }
    }
}
