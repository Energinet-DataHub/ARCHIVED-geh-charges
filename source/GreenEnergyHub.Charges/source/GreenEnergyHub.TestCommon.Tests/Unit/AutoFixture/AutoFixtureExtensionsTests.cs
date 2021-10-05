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

using AutoFixture;
using FluentAssertions;
using FluentAssertions.Execution;
using GreenEnergyHub.TestCommon.AutoFixture.Extensions;
using Xunit;

namespace GreenEnergyHub.TestCommon.Tests.Unit.AutoFixture
{
    public class AutoFixtureExtensionsTests
    {
        public class ForConstructorOn
        {
            [Fact]
            public void When_SetParameters_Then_ParameterValueIsUsedForCreation()
            {
                // Arrange
                var sut = new Fixture();

                // Act
                var actual = sut.ForConstructorOn<Example>()
                    .SetParameter("number").To(5)
                    .SetParameter("text").To("example text")
                    .Create();

                // Assert
                using var assertionScope = new AssertionScope();
                actual.Number.Should().Be(5);
                actual.Text.Should().Be("example text");
            }

            public class Example
            {
                public Example(int number, string text)
                {
                    Number = number;
                    Text = text;
                }

                public int Number { get; private set; }

                public string Text { get; private set; }
            }
        }
    }
}
