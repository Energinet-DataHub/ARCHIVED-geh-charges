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
using System.Threading;
using System.Threading.Tasks;
using AutoFixture.Xunit2;
using GreenEnergyHub.Charges.Application;
using GreenEnergyHub.Charges.Application.ChargeLinks;
using GreenEnergyHub.Charges.Application.Mapping;
using GreenEnergyHub.Charges.Domain.ChargeLinks;
using GreenEnergyHub.TestHelpers;
using Moq;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Application.ChargeLinks
{
    [UnitTest]
    public class ChargeLinkCommandAcceptedHandlerTests
    {
        [Theory]
        [InlineAutoDomainData]
        public async Task HandleAsync_WhenCalledWithValidChargeLinkXML_ShouldReturnOk(
            [NotNull] [Frozen] Mock<IMessageDispatcher<ChargeLinkCommandAcceptedEvent>> messageDispatcher,
            [NotNull] [Frozen] Mock<IChargeLinkCommandMapper> chargeLinkCommandMapper,
            [NotNull] ChargeLinkCommandReceivedEvent chargeLinkCommandReceivedEvent,
            [NotNull] ChargeLinkCommandAcceptedEvent chargeLinkCommandAcceptedEvent,
            [NotNull] ChargeLinkCommandAcceptedHandler sut)
        {
            // Arrange
            chargeLinkCommandMapper.Setup(x => x.Map(chargeLinkCommandReceivedEvent))
                .Returns(chargeLinkCommandAcceptedEvent);

            // Act
            await sut.HandleAsync(chargeLinkCommandReceivedEvent).ConfigureAwait(false);

            // Assert
            messageDispatcher.Verify(
                x => x.DispatchAsync(chargeLinkCommandAcceptedEvent, It.IsAny<CancellationToken>()));
        }
    }
}
