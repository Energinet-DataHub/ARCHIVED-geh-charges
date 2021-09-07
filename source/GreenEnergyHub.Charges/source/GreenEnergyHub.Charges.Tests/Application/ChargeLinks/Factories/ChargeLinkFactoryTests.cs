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
using System.Linq;
using System.Threading.Tasks;
using AutoFixture.Xunit2;
using FluentAssertions;
using GreenEnergyHub.Charges.Application.ChangeOfCharges.Repositories;
using GreenEnergyHub.Charges.Application.ChargeLinks.Factories;
using GreenEnergyHub.Charges.Application.Charges.Repositories;
using GreenEnergyHub.Charges.Domain.ChargeLinks.Events.Local;
using GreenEnergyHub.Charges.Domain.Charges;
using GreenEnergyHub.Charges.Domain.MeteringPoints;
using GreenEnergyHub.TestHelpers;
using Moq;
using Xunit;

namespace GreenEnergyHub.Charges.Tests.Application.ChargeLinks.Factories
{
    public class ChargeLinkFactoryTests
    {
        [Theory]
        [InlineAutoDomainData]
        public async Task CreateAsync_WhenCalled_ShouldCreateChargeLinkCorrectly(
            [NotNull] ChargeLinkCommandReceivedEvent receivedEvent,
            [NotNull] Charge returnedCharge,
            [NotNull] MeteringPoint returnedMeteringPoint,
            [Frozen] [NotNull] Mock<IChargeRepository> chargeRepository,
            [Frozen] [NotNull] Mock<IMeteringPointRepository> meteringPointRepository,
            [NotNull] ChargeLinkFactory sut)
        {
            // Arrange
            receivedEvent.SetCorrelationId(Guid.NewGuid().ToString("N"));
            returnedCharge.RowId = 11;
            returnedMeteringPoint.RowId = 22;

            chargeRepository
                .Setup(x => x.GetChargeAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<ChargeType>()))
                .ReturnsAsync(returnedCharge);

            meteringPointRepository
                .Setup(x => x.GetMeteringPointAsync(It.IsAny<string>()))
                .ReturnsAsync(returnedMeteringPoint);

            // Act
            var result = await sut.CreateAsync(receivedEvent).ConfigureAwait(false);

            // Assert
            result.ChargeRowId
                .Should()
                .Be(returnedCharge.RowId);
            result.MeteringPointRowId
                .Should()
                .Be(returnedMeteringPoint.RowId);
            result.PeriodDetails.First().StartDateTime
                .Should()
                .Be(receivedEvent.ChargeLinkCommand.ChargeLink.StartDateTime);
            result.PeriodDetails.First().EndDateTime
                .Should()
                .Be(receivedEvent.ChargeLinkCommand.ChargeLink.EndDateTime);
            result.PeriodDetails.First().Factor
                .Should()
                .Be(receivedEvent.ChargeLinkCommand.ChargeLink.Factor);
            result.Operations.First().CorrelationId
                .Should()
                .Be(receivedEvent.CorrelationId);
            result.Operations.First().Id
                .Should()
                .Be(receivedEvent.ChargeLinkCommand.ChargeLink.OperationId);
        }

        [Theory]
        [InlineAutoDomainData]
        public async Task CreateAsync_WhenCalledWithNull_ShouldThrow(
            [NotNull] ChargeLinkFactory sut)
        {
            await Assert
                .ThrowsAsync<ArgumentNullException>(async () => await sut.CreateAsync(null!)
                    .ConfigureAwait(false))
                .ConfigureAwait(false);
        }
    }
}
