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
using System.Collections.Generic;
using Energinet.DataHub.MessageHub.Model.Model;
using FluentAssertions;
using GreenEnergyHub.Charges.Domain.Dtos.SharedDtos;
using GreenEnergyHub.Charges.MessageHub.AvailableData.Models.AvailableChargeData;
using GreenEnergyHub.Charges.MessageHub.AvailableData.Models.AvailableData;
using GreenEnergyHub.Charges.MessageHub.BundleSpecification;
using GreenEnergyHub.Charges.MessageHub.MessageHub;
using GreenEnergyHub.Charges.Tests.Builders.MessageHub;
using GreenEnergyHub.TestHelpers;
using Moq;
using NodaTime;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.MessageHub.MessageHub
{
    [UnitTest]
    public class AvailableDataNotificationFactoryTests
    {
        [Theory]
        [InlineAutoDomainData]
        public void Create_WhenGivenAvailableConfirmations_CreatesNotifications(
            Mock<IBundleSpecification<AvailableDataBase>> bundleSpecification,
            string messageType,
            int messageWeight,
            AvailableDataNotificationFactory<AvailableDataBase> sut)
        {
            // Arrange
            var actorId = Guid.NewGuid();
            var availableData = GenerateListOfAvailableChargeDataForSameCharge(actorId, 3);
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
                actualNotificationList[i].Recipient.Value.Should().Be(availableData[i].ActorId);
                actualNotificationList[i].MessageType.Value.Should().Be(messageType);
                actualNotificationList[i].Origin.Should().Be(DomainOrigin.Charges);
                actualNotificationList[i].SupportsBundling.Should().BeTrue();
                actualNotificationList[i].RelativeWeight.Should().Be(messageWeight);
                actualNotificationList[i].DocumentType.Should().Be(availableData[i].DocumentType.ToString());
            }
        }

        private static List<AvailableChargeData> GenerateListOfAvailableChargeDataForSameCharge(Guid actorId, int numberOfAvailableChargeData)
        {
            var builder = new AvailableChargeDataBuilder();
            var now = Instant.FromDateTimeUtc(DateTime.UtcNow);
            var availableChargeDataList = new List<AvailableChargeData>();

            for (var i = 0; i < numberOfAvailableChargeData; i++)
            {
                var data = builder.WithActorId(actorId).WithRequestDateTime(now).WithOperationOrder(i).Build();
                availableChargeDataList.Add(data);
            }

            return availableChargeDataList;
        }
    }
}
