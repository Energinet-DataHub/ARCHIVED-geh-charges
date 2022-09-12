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
using System.Threading.Tasks;
using AutoFixture.Xunit2;
using Energinet.DataHub.Core.App.FunctionApp.Middleware.CorrelationId;
using Energinet.DataHub.MessageHub.Client.DataAvailable;
using Energinet.DataHub.MessageHub.Model.Model;
using FluentAssertions;
using GreenEnergyHub.Charges.MessageHub.BundleSpecification;
using GreenEnergyHub.Charges.MessageHub.MessageHub;
using GreenEnergyHub.Charges.MessageHub.Models.AvailableData;
using GreenEnergyHub.Charges.TestCore.Attributes;
using Moq;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.MessageHub.MessageHub
{
    [UnitTest]
    public class AvailableDataNotifierTests
    {
        [Theory]
        [InlineAutoMoqData]
        public async Task NotifyAsync_WhenGivenInput_Notifies(
            [Frozen] Mock<IAvailableDataFactory<AvailableDataBase, object>> availableDataFactory,
            [Frozen] Mock<IAvailableDataRepository<AvailableDataBase>> availableDataRepository,
            [Frozen] Mock<IAvailableDataNotificationFactory<AvailableDataBase>> availableDataNotificationFactory,
            [Frozen] Mock<IDataAvailableNotificationSender> dataAvailableNotificationSender,
            [Frozen] Mock<ICorrelationContext> correlationContext,
            object input,
            List<AvailableDataBase> availableData,
            IReadOnlyList<DataAvailableNotificationDto> notifications,
            AvailableDataNotifier<AvailableDataBase, object> sut)
        {
            // Arrange
            availableDataFactory.Setup(
                    f => f.CreateAsync(input))
                .ReturnsAsync(availableData);

            availableDataNotificationFactory.Setup(
                    f => f.Create(
                        availableData,
                        It.IsAny<IBundleSpecification<AvailableDataBase>>()))
                .Returns(notifications);

            var correlationId = Guid.NewGuid().ToString();
            correlationContext.Setup(c => c.Id)
                .Returns(correlationId);

            // Act
            await sut.NotifyAsync(input);

            // Assert
            availableDataFactory.Verify(
                f => f.CreateAsync(input),
                Times.Once);

            availableDataRepository.Verify(
                r => r.StoreAsync(availableData),
                Times.Once);

            availableDataNotificationFactory.Verify(
                f => f.Create(
                    availableData,
                    It.IsAny<IBundleSpecification<AvailableDataBase>>()),
                Times.Once);

            dataAvailableNotificationSender.Verify(
                d => d.SendAsync(
                    correlationId,
                    It.IsAny<DataAvailableNotificationDto>()),
                Times.Exactly(notifications.Count));
        }

        [Theory]
        [InlineAutoMoqData]
        public async Task NotifyAsync_WhenNoAvailableData_DoesNotProceed(
            [Frozen] Mock<IAvailableDataFactory<AvailableDataBase, object>> availableDataFactory,
            [Frozen] Mock<IAvailableDataRepository<AvailableDataBase>> availableDataRepository,
            object input,
            AvailableDataNotifier<AvailableDataBase, object> sut)
        {
            // Arrange
            var emptyList = new List<AvailableDataBase>();
            availableDataFactory.Setup(
                    f => f.CreateAsync(input))
                .ReturnsAsync(emptyList);

            // Act
            await sut.NotifyAsync(input);

            // Assert
            availableDataFactory.Verify(
                f => f.CreateAsync(input),
                Times.Once);

            emptyList.Should().BeEmpty();
            availableDataRepository.Verify(
                r => r.StoreAsync(
                    It.IsAny<IEnumerable<AvailableDataBase>>()),
                Times.Never);
        }
    }
}
