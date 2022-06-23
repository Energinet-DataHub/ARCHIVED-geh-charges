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

using System.Threading.Tasks;
using AutoFixture.Xunit2;
using FluentAssertions;
using GreenEnergyHub.Charges.Application.ChargeLinks.Handlers;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeLinksCommands;
using GreenEnergyHub.TestHelpers;
using Moq;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Application.ChargeLinks.Handlers
{
    [UnitTest]
    public class ChargeLinksCommandBundleHandlerTests
    {
        [Theory]
        [InlineAutoDomainData]
        public async Task HandleAsync_WhenValidChargeLinkBundle_HandlesAll(
            [Frozen] Mock<IChargeLinksCommandHandler> chargeLinksCommandHandler,
            ChargeLinksCommandBundle chargeLinksCommandBundle,
            ChargeLinksCommandBundleHandler sut)
        {
            // Act
            await sut.HandleAsync(chargeLinksCommandBundle).ConfigureAwait(false);

            // Assert
            chargeLinksCommandBundle.Commands.Should().NotBeEmpty();
            chargeLinksCommandHandler.Verify(
                x => x.HandleAsync(It.IsAny<ChargeLinksCommand>()),
                Times.Exactly(chargeLinksCommandBundle.Commands.Count));
        }
    }
}
