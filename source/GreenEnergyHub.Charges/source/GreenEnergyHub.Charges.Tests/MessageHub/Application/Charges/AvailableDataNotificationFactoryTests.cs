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

using System.Collections.Generic;
using Energinet.DataHub.MessageHub.Model.Model;
using FluentAssertions;
using GreenEnergyHub.Charges.Domain.MarketParticipants;
using GreenEnergyHub.Charges.MessageHub.Application.MessageHub;
using GreenEnergyHub.Charges.MessageHub.Models.AvailableData;
using Moq;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.MessageHub.Application.Charges
{
    [UnitTest]
    public class AvailableDataNotificationFactoryTests
    {
        public void Create_WhenGivenAvailableConfirmations_CreatesNotifications(
            Mock<IBundleSpecification<AvailableDataBase>> bundleSpecification,
            IReadOnlyList<AvailableDataBase> availableData,
            string messageType,
            int messageWeight,
            AvailableDataNotificationFactory<AvailableDataBase> sut)
        {
            // Arrange
            bundleSpecification.Setup(
                    b => b.GetMessageType(
                        It.IsAny<BusinessReasonCode>()))
                .Returns(messageType);

            bundleSpecification.Setup(
                    b => b.GetMessageWeight(
                        It.IsAny<AvailableDataBase>()))
                .Returns(messageWeight);

            // Act
            var actualNotificationList = sut.Create(availableData, bundleSpecification.Object);

            // Assert
            actualNotificationList.Should().HaveSameCount(availableData);
            for (var i = 0; i < actualNotificationList.Count; i++)
            {
                actualNotificationList[i].Uuid.Should().Be(availableData[i].AvailableDataReferenceId);
                actualNotificationList[i].Recipient.Value.Should().Be(availableData[i].RecipientId);
                actualNotificationList[i].MessageType.Value.Should().Be(messageType);
                actualNotificationList[i].Origin.Should().Be(DomainOrigin.Charges);
                actualNotificationList[i].SupportsBundling.Should().BeTrue();
                actualNotificationList[i].RelativeWeight.Should().Be(messageWeight);
            }
        }
    }
}
