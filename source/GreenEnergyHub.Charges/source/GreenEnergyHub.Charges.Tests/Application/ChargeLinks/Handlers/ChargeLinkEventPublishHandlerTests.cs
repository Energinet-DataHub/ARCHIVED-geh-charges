﻿// Copyright 2020 Energinet DataHub A/S
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
using Energinet.DataHub.Charges.Libraries.DefaultChargeLink;
using Energinet.DataHub.Charges.Libraries.Models;
using FluentAssertions;
using GreenEnergyHub.Charges.Application;
using GreenEnergyHub.Charges.Application.ChargeLinks.Handlers;
using GreenEnergyHub.Charges.Domain.ChargeLinkCommandAcceptedEvents;
using GreenEnergyHub.Charges.Domain.ChargeLinkCommands;
using GreenEnergyHub.Charges.Domain.ChargeLinkCreatedEvents;
using GreenEnergyHub.TestHelpers;
using Moq;
using NodaTime;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Application.ChargeLinks.Handlers
{
    [UnitTest]
    public class ChargeLinkEventPublishHandlerTests
    {
        [Theory]
        [InlineAutoDomainData]
        public async Task HandleAsync_WhenCalled_UsesFactoryToCreateEventAndDispatchesIt(
            [Frozen] [NotNull] Mock<IChargeLinkCreatedEventFactory> factory,
            [Frozen] [NotNull] Mock<IMessageDispatcher<ChargeLinkCreatedEvent>> dispatcher,
            [NotNull] ChargeLinkCommandAcceptedEvent command,
            [NotNull] ChargeLinkCreatedEvent createdEvent,
            [NotNull] ChargeLinkEventPublishHandler sut)
        {
            // Arrange
            factory.Setup(
                    f => f.CreateEvent(
                        It.IsAny<ChargeLinkCommand>()))
                .Returns(createdEvent);

            var dispatched = false;
            dispatcher.Setup(
                    d => d.DispatchAsync(
                        createdEvent,
                        It.IsAny<CancellationToken>()))
                .Callback<ChargeLinkCreatedEvent, CancellationToken>(
                    (_, _) => dispatched = true);

            // Act
            await sut.HandleAsync(command).ConfigureAwait(false);

            // Assert
            dispatched.Should().BeTrue();
        }
    }
}
