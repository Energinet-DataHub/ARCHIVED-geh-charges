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

using System.Linq;
using AutoFixture.Xunit2;
using FluentAssertions;
using GreenEnergyHub.Charges.Application.Messaging;
using GreenEnergyHub.Charges.Infrastructure.Core.MessageMetaData;
using GreenEnergyHub.Charges.Infrastructure.Core.MessagingExtensions.Factories;
using GreenEnergyHub.TestHelpers;
using Moq;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Infrastructure.Core.MessagingExtensions.Factories
{
    [UnitTest]
    public class ServiceBusMessageFactoryTests
    {
        [Theory]
        [InlineAutoDomainData]
        public void ExternalMessage_DoesNotContain_ReplyTo(
            byte[] data,
            ServiceBusMessageFactory sut)
        {
            var actual = sut.CreateExternalMessage(data);
            actual.ReplyTo.Should().BeNull();
        }

        [Theory]
        [InlineAutoDomainData]
        public void InternalMessage_DoesContain_ReplyTo(
            [Frozen] Mock<IMessageMetaDataContext> messageMetaDataContext,
            string data,
            string replyTo,
            ServiceBusMessageFactory sut)
        {
            // Arrange
            messageMetaDataContext.Setup(m => m.ReplyTo).Returns(replyTo);
            messageMetaDataContext.Setup(m => m.IsReplyToSet()).Returns(true);

            // Act
            var actual = sut.CreateInternalMessage(data);

            // Assert
            actual.ApplicationProperties.First(x => x.Key == MessageMetaDataConstants.ReplyTo).Value.Should().Be(replyTo);
        }
    }
}
