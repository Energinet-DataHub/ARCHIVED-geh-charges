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
using AutoFixture.Xunit2;
using Energinet.DataHub.Charges.Libraries.DefaultChargeLink;
using Energinet.DataHub.Charges.Libraries.Models;
using GreenEnergyHub.Charges.Application;
using GreenEnergyHub.Charges.Application.ChargeLinks.Handlers;
using GreenEnergyHub.Charges.Domain.ChargeLinkCommandAcceptedEvents;
using GreenEnergyHub.Charges.Domain.ChargeLinkCommands;
using GreenEnergyHub.TestHelpers;
using Moq;
using NodaTime;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Application.ChargeLinks.Handlers
{
    [UnitTest]
    public class ChargeLinkEventReplyHandlerTests
    {
        [Theory]
        [InlineAutoDomainData]
        public async Task HandleAsync_WhenCalledWithReplyToSetInMessageMetaDataContext_ReplyWithDefaultChargeLinkSucceededDto(
            [Frozen] [NotNull] Mock<IMessageMetaDataContext> messageMetaDataContext,
            [Frozen] [NotNull] Mock<ICorrelationContext> correlationContext,
            [Frozen] [NotNull] Mock<IDefaultChargeLinkClient> defaultChargeLinkClient,
            [NotNull] string replyTo,
            [NotNull] string correlationId,
            [NotNull] CreateDefaultChargeLinksReplierHandler sut)
        {
            // Arrange
            messageMetaDataContext.Setup(m => m.IsReplyToSet()).Returns(true);
            messageMetaDataContext.Setup(m => m.ReplyTo).Returns(replyTo);
            correlationContext.Setup(c => c.Id).Returns(correlationId);

            var command = GetChargeLinkCommandAcceptedEvent();

            // Act
            await sut.HandleAsync(command).ConfigureAwait(false);

            // Assert
            defaultChargeLinkClient.Verify(
                x => x.CreateDefaultChargeLinksSucceededReplyAsync(
                    It.IsAny<CreateDefaultChargeLinksSucceededDto>(),
                    correlationId,
                    replyTo));
        }

        [Theory]
        [InlineAutoDomainData]
        public async Task HandleAsync_WhenCalled_ThrowsInvalidOperationExceptionIfMeteringPointIdsDiffer(
            [Frozen] [NotNull] Mock<IMessageMetaDataContext> messageMetaDataContext,
            [NotNull] string replyTo,
            [NotNull] CreateDefaultChargeLinksReplierHandler sut)
        {
            // Arrange
            messageMetaDataContext.Setup(m => m.IsReplyToSet()).Returns(true);
            messageMetaDataContext.Setup(m => m.ReplyTo).Returns(replyTo);

            const string optionalMeteringPointId = "optionalMeteringPointId";
            var command = GetChargeLinkCommandAcceptedEvent(optionalMeteringPointId);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => sut.HandleAsync(command));
        }

        private static ChargeLinkCommandAcceptedEvent GetChargeLinkCommandAcceptedEvent(
            string optionalMeteringPointId = "first")
        {
            const string meteringPointId = "first";

            var command = new ChargeLinkCommandAcceptedEvent(
                new[]
                {
                    new ChargeLinkCommand
                    {
                        ChargeLink = new ChargeLinkDto { MeteringPointId = meteringPointId },
                    },
                    new ChargeLinkCommand
                    {
                        ChargeLink = new ChargeLinkDto { MeteringPointId = optionalMeteringPointId },
                    },
                },
                Instant.MinValue);
            return command;
        }
    }
}
