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
using System.Linq;
using System.Xml.Linq;
using FluentAssertions;
using GreenEnergyHub.Charges.MessageHub.Infrastructure.Cim;
using GreenEnergyHub.Charges.TestCore.Attributes;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.MessageHub.Infrastructure.Cim
{
    [UnitTest]
    public class CimHelperTests
    {
        [Theory]
        [InlineAutoMoqData]
        public void GetElementIfNeeded_WhenNotNeeded_ReturnsEmptyListAndDoesNotInvokeGetValue(
            XNamespace cimNamespace,
            string elementName)
        {
            // Act
            var actual = CimXmlHelper.GetElementIfNeeded(
                cimNamespace,
                true,
                elementName,
                () => throw new InvalidOperationException());

            // Assert
            actual.Should().BeEmpty();
        }

        [Theory]
        [InlineAutoMoqData]
        public void GetElementIfNeeded_WhenNeeded_AddsSingleElementWithCorrectContent(
            XNamespace cimNamespace,
            string elementName,
            string value)
        {
            // Act
            var actual = CimXmlHelper.GetElementIfNeeded(
                cimNamespace,
                false,
                elementName,
                () => value).ToArray();

            // Assert
            actual.Should().ContainSingle();
            actual.First().Name.Namespace.Should().Be(cimNamespace);
            actual.First().Name.LocalName.Should().Be(elementName);
            actual.First().Value.Should().Be(value);
        }
    }
}
