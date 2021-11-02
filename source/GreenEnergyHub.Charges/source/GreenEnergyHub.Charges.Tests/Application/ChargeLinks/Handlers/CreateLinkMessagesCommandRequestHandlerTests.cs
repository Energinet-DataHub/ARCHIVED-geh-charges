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

using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using AutoFixture.Xunit2;
using Energinet.DataHub.Charges.Libraries.DefaultChargeLinkMessages;
using Energinet.DataHub.Charges.Libraries.Models;
using GreenEnergyHub.Charges.Application;
using GreenEnergyHub.Charges.Application.ChargeLinks.Handlers;
using GreenEnergyHub.Charges.Domain.CreateLinkMessagesCommandEvent;
using GreenEnergyHub.TestHelpers;
using Moq;
using Xunit;

namespace GreenEnergyHub.Charges.Tests.Application.ChargeLinks.Handlers
{
    public class CreateLinkMessagesCommandRequestHandlerTests
    {
        [Theory]
        [InlineAutoDomainData]
        public async Task HandleAsync_WhenCalledWithReplyToSetInMessageMetaDataContext_ReplyWithDefaultChargeLinkSucceededDto(
            [Frozen] [NotNull] Mock<IMessageMetaDataContext> messageMetaDataContext,
            [Frozen] [NotNull] Mock<ICorrelationContext> correlationContext,
            [Frozen] [NotNull] Mock<IDefaultChargeLinkMessagesRequestClient> defaultChargeLinkMessagesClient,
            [NotNull] string replyTo,
            [NotNull] string correlationId,
            [NotNull] CreateLinkMessagesCommandEvent createLinkMessagesCommandEvent,
            [NotNull] CreateLinkMessagesCommandRequestHandler sut)
        {
            // Arrange
            messageMetaDataContext.Setup(m => m.IsReplyToSet()).Returns(true);
            messageMetaDataContext.Setup(m => m.ReplyTo).Returns(replyTo);
            correlationContext.Setup(c => c.Id).Returns(correlationId);

            // Act
            await sut.HandleAsync(createLinkMessagesCommandEvent, correlationId).ConfigureAwait(false);

            // Assert
            defaultChargeLinkMessagesClient.Verify(
                x => x.CreateDefaultChargeLinkMessagesSucceededRequestAsync(
                    It.IsAny<CreateDefaultChargeLinkMessagesSucceededDto>(),
                    correlationId,
                    replyTo));
        }
    }
}
