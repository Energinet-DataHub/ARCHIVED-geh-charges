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
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using FluentAssertions;
using GreenEnergyHub.Charges.Application.ChargeLinks;
using GreenEnergyHub.Charges.Domain.ChargeLinks;
using GreenEnergyHub.TestHelpers;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Application.ChargeLinks
{
    [UnitTest]
    public class ChargeLinkMessageHandlerTests
    {
        [Theory]
        [InlineAutoDomainData]
        public async Task HandleAsync_WhenCalledWithValidChargeLinkXML_ShouldReturnOk(
            [NotNull] ChargeLinkCommandHandler sut)
        {
            // Arrange
            var chargeLinkCommand = new ChargeLinkCommand(Guid.NewGuid().ToString());

            // Act
            var result = await sut.HandleAsync(chargeLinkCommand).ConfigureAwait(false);

            // Assert
            result.IsSucceeded.Should().BeTrue();
        }
    }
}
