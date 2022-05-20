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
using System.Threading.Tasks;
using AutoFixture.Xunit2;
using FluentAssertions;
using GreenEnergyHub.Charges.Domain.MarketParticipants;
using GreenEnergyHub.Charges.Infrastructure;
using GreenEnergyHub.Charges.TestCore.Attributes;
using GreenEnergyHub.Charges.Tests.Builders.Command;
using Moq;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Infrastructure
{
    [UnitTest]
    public class ActorProviderTests
    {
        [Theory]
        [InlineAutoMoqData]
        public async Task GetActorAsync_ShouldThrow_WhenNoActorFound([Frozen] Mock<IMarketParticipantRepository> marketParticipantRepository)
        {
            // Arrange
            var actorId = Guid.NewGuid();
            marketParticipantRepository.Setup(x => x.SingleOrNullAsync(actorId)).ReturnsAsync(null as MarketParticipant);

            var sut = new ActorProvider(marketParticipantRepository.Object);

            // Act and Assert
            await Assert.ThrowsAsync<Exception>(() => sut.GetActorAsync(actorId));
        }

        [Theory]
        [InlineAutoMoqData]
        public async Task GetActorAsync_ShouldReturnActor([Frozen] Mock<IMarketParticipantRepository> marketParticipantRepository)
        {
            // Arrange
            var actorId = Guid.NewGuid();
            var marketParticipant = new MarketParticipantBuilder().Build();
            marketParticipantRepository.Setup(x => x.SingleOrNullAsync(actorId)).ReturnsAsync(marketParticipant);

            var sut = new ActorProvider(marketParticipantRepository.Object);

            // Act
            var actual = await sut.GetActorAsync(actorId);

            // Assert
            actual.ActorId.Should().Be(marketParticipant.Id);
            actual.IdentificationType.Should().Be("GLN");
            actual.Identifier.Should().Be(marketParticipant.MarketParticipantId);
            actual.Roles.Should().Be(marketParticipant.BusinessProcessRole.ToString());
        }
    }
}
