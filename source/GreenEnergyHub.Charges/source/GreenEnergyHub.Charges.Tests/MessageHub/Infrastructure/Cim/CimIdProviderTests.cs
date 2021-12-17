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
using GreenEnergyHub.Charges.MessageHub.Infrastructure.Cim;
using GreenEnergyHub.Charges.TestCore.Attributes;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.MessageHub.Infrastructure.Cim
{
    [UnitTest]
    public class CimIdProviderTests
    {
        [Theory]
        [InlineAutoMoqData]
        public void GetUniqueId_WhenCalled_ReturnsValidId(
            CimIdProvider sut)
        {
            // Act
            var actual = sut.GetUniqueId();

            // Arrange
            actual.Should().NotBeNullOrWhiteSpace();
            actual.Should().NotContain("-");
        }
    }
}
