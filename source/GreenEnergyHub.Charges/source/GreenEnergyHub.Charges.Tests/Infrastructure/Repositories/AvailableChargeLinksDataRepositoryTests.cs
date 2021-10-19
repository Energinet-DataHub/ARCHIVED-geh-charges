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
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture.Xunit2;
using FluentAssertions;
using GreenEnergyHub.Charges.Domain.AvailableChargeLinksData;
using GreenEnergyHub.Charges.Domain.ChargeLinkCommandAcceptedEvents;
using GreenEnergyHub.Charges.Domain.MarketParticipants;
using GreenEnergyHub.Charges.Infrastructure.Repositories;
using GreenEnergyHub.Charges.TestCore.Attributes;
using GreenEnergyHub.Charges.TestCore.Database;
using Moq;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Infrastructure.Repositories
{
    [UnitTest]
    public class AvailableChargeLinksDataRepositoryTests : IClassFixture<ChargesDatabaseFixture>
    {
        private readonly ChargesDatabaseManager _databaseManager;

        public AvailableChargeLinksDataRepositoryTests(ChargesDatabaseFixture fixture)
        {
            _databaseManager = fixture.DatabaseManager;
        }

        [Theory]
        [InlineAutoMoqData]
        public async Task StoreAsync_StoresAvailableChargeLinksData(
            [Frozen] Mock<IAvailableChargeLinksDataFactory> chargeLinkFactory,
            [NotNull] AvailableChargeLinksData expected)
        {
            // Arrange
            await using var chargesDatabaseWriteContext = _databaseManager.CreateDbContext();

            chargeLinkFactory
                .Setup(x =>
                    x.CreateAvailableChargeLinksData(
                        It.IsAny<ChargeLinkCommandAcceptedEvent>(),
                        It.IsAny<MarketParticipant>(),
                        It.IsAny<Guid>()))
                .Returns(expected);

            var sut = new AvailableChargeLinksDataRepository(chargesDatabaseWriteContext, chargeLinkFactory.Object);

            // Act
            await sut.StoreAsync(null!, null!, Guid.NewGuid()).ConfigureAwait(false);

            // Assert
            await using var chargesDatabaseReadContext = _databaseManager.CreateDbContext();
            var actual =
                chargesDatabaseReadContext.AvailableChargeLinksData.Single(x => x.AvailableDataReferenceId == expected.AvailableDataReferenceId);
            actual.ChargeOwner.Should().Be(expected.ChargeOwner);
        }

        [Theory]
        [InlineAutoMoqData]
        public async Task GetChargeHistoriesAsync_WithMeteringPointId_ThenSuccessReturnedAsync(
            [Frozen] Mock<IAvailableChargeLinksDataFactory> chargeLinkFactory,
            [NotNull] AvailableChargeLinksData expected)
        {
            // Arrange
            await using var chargesDatabaseWriteContext = _databaseManager.CreateDbContext();
            await chargesDatabaseWriteContext.AvailableChargeLinksData.AddAsync(expected).ConfigureAwait(false);
            await chargesDatabaseWriteContext.SaveChangesAsync().ConfigureAwait(false);

            await using var chargesDatabaseReadContext = _databaseManager.CreateDbContext();
            var sut = new AvailableChargeLinksDataRepository(chargesDatabaseReadContext, chargeLinkFactory.Object);

            // Act
            var actual =
                await sut.GetAvailableChargeLinksDataAsync(new List<Guid> { expected.AvailableDataReferenceId })
                .ConfigureAwait(false);

            // Assert
            Assert.NotNull(actual);
        }
    }
}
