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
using FluentAssertions;
using GreenEnergyHub.Charges.Application;
using GreenEnergyHub.Charges.Application.ChargeLinks;
using GreenEnergyHub.Charges.Application.ChargeLinks.Handlers;
using GreenEnergyHub.Charges.Domain.ChargeLinks;
using GreenEnergyHub.Charges.Domain.ChargeLinks.Command;
using GreenEnergyHub.Charges.Domain.ChargeLinks.Events.Local;
using GreenEnergyHub.Charges.Domain.Charges.Commands;
using GreenEnergyHub.TestHelpers;
using Moq;
using NodaTime;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Application.ChargeLinks
{
    [UnitTest]
    public class ChargeLinkCommandHandlerTests
    {
        [Theory]
        [InlineAutoDomainData]
        public async Task HandleAsync_WhenCalledWithValidChargeLink_ShouldReturnOk(
            [NotNull] [Frozen] Mock<IMessageDispatcher<ChargeLinkCommandReceivedEvent>> messageDispatcher,
            [NotNull] ChargeLinkCommand chargeLinkCommand,
            [NotNull] ChargeLinkCommandHandler sut)
        {
            // Arrange
            var linkCommandReceivedEvent = new ChargeLinkCommandReceivedEvent(
                Instant.FromUtc(2021, 7, 7, 7, 50, 49),
                "CorrelationId",
                chargeLinkCommand);

            // Act
            var result = await sut.HandleAsync(chargeLinkCommand).ConfigureAwait(false);

            // Assert
            result.IsSucceeded.Should().BeTrue();
            messageDispatcher.Verify(
                x => x.DispatchAsync(linkCommandReceivedEvent, It.IsAny<CancellationToken>()));
        }
    }
}
