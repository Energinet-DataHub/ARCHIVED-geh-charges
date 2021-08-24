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
using System.Threading.Tasks;
using AutoFixture.Xunit2;
using GreenEnergyHub.Charges.Application;
using GreenEnergyHub.Charges.Application.ChangeOfCharges.Repositories;
using GreenEnergyHub.Charges.Domain.Events.Integration;
using GreenEnergyHub.Charges.Domain.MeteringPoints;
using GreenEnergyHub.Charges.TestCore;
using GreenEnergyHub.TestHelpers;
using Moq;
using NodaTime;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Application.MeteringPoints
{
    [UnitTest]
    public class MeteringPointCreatedHandlerTests
    {
        [Theory]
        [InlineAutoDomainData]
        public async Task HandleAsync_WhenCalled_ShouldCallRepository(
            [NotNull][Frozen] Mock<IMeteringPointRepository> meteringPointRepository,
            [NotNull] MeteringPointCreatedEventHandler sut)
        {
            var meteringPointCreatedEvent = GetMeteringPointCreatedEvent();

            await sut.HandleAsync(meteringPointCreatedEvent).ConfigureAwait(false);

            meteringPointRepository
                .Verify(v => v.StoreMeteringPointAsync(It.IsAny<MeteringPoint>()), Times.Exactly(1));
        }

        [Theory]
        [InlineAutoMoqData]
        public async Task HandleAsync_WhenEventIsNull_ThrowsArgumentNullException(
            [NotNull] MeteringPointCreatedEventHandler sut)
        {
            // Arrange
            MeteringPointCreatedEvent? meteringPointCreatedEvent = null;

            // Act / Assert
            await Assert.ThrowsAsync<ArgumentNullException>(
                    () => sut.HandleAsync(meteringPointCreatedEvent!))
                .ConfigureAwait(false);
        }

        private static MeteringPointCreatedEvent GetMeteringPointCreatedEvent()
        {
            return new MeteringPointCreatedEvent(
                "123",
                MeteringPointType.Consumption.ToString(),
                "234",
                "2",
                "1",
                "1",
                "mrp",
                "456",
                "567",
                "678",
                "product",
                "1",
                SystemClock.Instance.GetCurrentInstant().ToString());
        }
    }
}
