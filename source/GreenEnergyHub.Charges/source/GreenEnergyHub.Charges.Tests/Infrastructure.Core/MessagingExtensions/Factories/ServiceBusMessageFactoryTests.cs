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
using System.Linq;
using AutoFixture.Xunit2;
using Energinet.DataHub.Core.App.FunctionApp.Middleware.CorrelationId;
using Energinet.DataHub.Core.TestCommon.AutoFixture.Attributes;
using FluentAssertions;
using GreenEnergyHub.Charges.Application.Messaging;
using GreenEnergyHub.Charges.Core.DateTime;
using GreenEnergyHub.Charges.Infrastructure.Core.MessageMetaData;
using GreenEnergyHub.Charges.Infrastructure.Core.MessagingExtensions.Factories;
using GreenEnergyHub.TestHelpers;
using Moq;
using NodaTime;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Infrastructure.Core.MessagingExtensions.Factories
{
    [UnitTest]
    public class ServiceBusMessageFactoryTests
    {
        [Theory]
        [InlineAutoDomainData]
        public void CreateExternalMessage_MessageDoesNotContainReplyTo(byte[] data, ServiceBusMessageFactory sut)
        {
            var actual = sut.CreateExternalMessage(data, "FakeMessageType");
            actual.ReplyTo.Should().BeNull();
        }

        [Theory]
        [InlineAutoDomainData]
        public void CreateExternalMessage_WhenMessageMetaDataContextContainsData_MessageContainsRelevantApplicationProperties(
            [Frozen] Mock<IMessageMetaDataContext> messageMetaDataContext,
            [Frozen] Mock<ICorrelationContext> correlationContext,
            byte[] data,
            ServiceBusMessageFactory sut)
        {
            // Arrange
            const string messageType = "FakeMessageType";
            var currentInstant = SystemClock.Instance.GetCurrentInstant();
            messageMetaDataContext.Setup(m => m.RequestDataTime).Returns(currentInstant);
            correlationContext.Setup(c => c.Id).Returns(Guid.NewGuid().ToString);

            // Act
            var actual = sut.CreateExternalMessage(data, messageType);

            // Assert
            actual.ApplicationProperties.First(x => x.Key == MessageMetaDataConstants.OperationTimestamp).Value
                .Should().Be(currentInstant.GetCreatedDateTimeFormat());
            actual.ApplicationProperties.First(x => x.Key == MessageMetaDataConstants.CorrelationId).Value
                .Should().Be(correlationContext.Object.Id);
            actual.ApplicationProperties.First(x => x.Key == MessageMetaDataConstants.MessageVersion).Value
                .Should().Be(1);
            actual.ApplicationProperties.First(x => x.Key == MessageMetaDataConstants.MessageType).Value
                .Should().Be(messageType);
            actual.ApplicationProperties.First(x => x.Key == MessageMetaDataConstants.EventIdentification).Value
                .Should().NotBeNull();
        }

        [Theory]
        [InlineAutoMoqData("replyTo", true)]
        [InlineAutoMoqData(null!, false)]
        public void CreateInternalMessage_WhenMessageMetaDataContextContainsData_MessageContainsRelevantApplicationProperties(
            string replyTo,
            bool replyToSet,
            [Frozen] Mock<IMessageMetaDataContext> messageMetaDataContext,
            [Frozen] Mock<ICorrelationContext> correlationContext,
            string data,
            ServiceBusMessageFactory sut)
        {
            // Arrange
            const string messageType = "FakeMessageType";
            var currentInstant = SystemClock.Instance.GetCurrentInstant();
            messageMetaDataContext.Setup(m => m.RequestDataTime).Returns(currentInstant);
            messageMetaDataContext.Setup(m => m.IsReplyToSet()).Returns(replyToSet);
            messageMetaDataContext.Setup(m => m.ReplyTo).Returns(replyTo);
            correlationContext.Setup(c => c.Id).Returns(Guid.NewGuid().ToString);

            // Act
            var actual = sut.CreateInternalMessage(data, messageType);

            // Assert
            actual.ApplicationProperties.First(x => x.Key == MessageMetaDataConstants.OperationTimestamp).Value
                .Should().Be(currentInstant.GetCreatedDateTimeFormat());
            actual.ApplicationProperties.First(x => x.Key == MessageMetaDataConstants.CorrelationId).Value
                .Should().Be(correlationContext.Object.Id);
            actual.ApplicationProperties.First(x => x.Key == MessageMetaDataConstants.MessageVersion).Value
                .Should().Be(1);
            actual.ApplicationProperties.First(x => x.Key == MessageMetaDataConstants.MessageType).Value
                .Should().Be(messageType);
            actual.ApplicationProperties.First(x => x.Key == MessageMetaDataConstants.EventIdentification).Value
                .Should().NotBeNull();
        }

        [Theory]
        [InlineAutoDomainData]
        public void CreateInternalMessage_WhenReplyToIsSet_MessageContainsReplyTo(
            [Frozen] Mock<IMessageMetaDataContext> messageMetaDataContext,
            string data,
            string replyTo,
            ServiceBusMessageFactory sut)
        {
            // Arrange
            messageMetaDataContext.Setup(m => m.ReplyTo).Returns(replyTo);
            messageMetaDataContext.Setup(m => m.IsReplyToSet()).Returns(true);

            // Act
            var actual = sut.CreateInternalMessage(data, "FakeSubject");

            // Assert
            actual.ApplicationProperties.First(x => x.Key == MessageMetaDataConstants.ReplyTo).Value.Should().Be(replyTo);
        }

        [Theory]
        [InlineAutoMoqData("replyTo", true)]
        [InlineAutoMoqData(null!, false)]
        public void CreateInternalMessage_WhenSubjectProvided_MessageContainsSubject(
            string replyTo,
            bool replyToSet,
            [Frozen] Mock<IMessageMetaDataContext> messageMetaDataContext,
            string data,
            ServiceBusMessageFactory sut)
        {
            // Arrange
            const string subject = "FakeSubject";
            messageMetaDataContext.Setup(m => m.ReplyTo).Returns(replyTo);
            messageMetaDataContext.Setup(m => m.IsReplyToSet()).Returns(replyToSet);

            // Act
            var actual = sut.CreateInternalMessage(data, subject);

            // Assert
            actual.Subject.Should().Be(subject);
        }
    }
}
