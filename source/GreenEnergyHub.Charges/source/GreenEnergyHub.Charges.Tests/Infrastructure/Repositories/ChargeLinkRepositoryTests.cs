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
using System.Threading.Tasks;
using FluentAssertions;
using GreenEnergyHub.Charges.Core.DateTime;
using GreenEnergyHub.Charges.Domain.ChargeLinks;
using GreenEnergyHub.Charges.Domain.MeteringPoints;
using GreenEnergyHub.Charges.Infrastructure.Context;
using GreenEnergyHub.Charges.Infrastructure.Repositories;
using GreenEnergyHub.Charges.TestCore.Database;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using Xunit;
using Xunit.Categories;
using MarketParticipant = GreenEnergyHub.Charges.Infrastructure.Context.Model.MarketParticipant;

namespace GreenEnergyHub.Charges.Tests.Infrastructure.Repositories
{
    /// <summary>
    /// Tests <see cref="ChargeRepository"/> using a SQL database.
    /// </summary>
    [UnitTest]
    public class ChargeLinkRepositoryTests : IClassFixture<ChargesDatabaseFixture>
    {
        private const string ExpectedOperationId = "expected-operation-id";
        private const string ExpectedCorrelationId = "expected-correlation-id";
        private const int ExpectedOperationDetailsFactor = 127;

        private readonly Instant _expectedPeriodDetailsStartDateTime = SystemClock.Instance.GetCurrentInstant();

        private readonly ChargesDatabaseManager _databaseManager;

        public ChargeLinkRepositoryTests(ChargesDatabaseFixture fixture)
        {
            _databaseManager = fixture.DatabaseManager;
        }

        [Fact]
        public async Task StoreAsync_StoresChargeLink()
        {
            // Arrange
            await using var chargesDatabaseWriteContext = _databaseManager.CreateDbContext();

            var ids = SeedDatabase(chargesDatabaseWriteContext);
            var expected = CreateNewExpectedChargeLink(ids);
            var sut = new ChargeLinkRepository(chargesDatabaseWriteContext);

            // Act
            await sut.StoreAsync(expected).ConfigureAwait(false);

            // Assert
            await using var chargesDatabaseReadContext = _databaseManager.CreateDbContext();

            var actual = await chargesDatabaseReadContext.ChargeLinks.SingleAsync(
                    c => c.ChargeId == ids.chargeId && c.MeteringPointId == ids.meteringPointId)
                .ConfigureAwait(false);
            actual.Should().BeEquivalentTo(expected);
        }

        private ChargeLink CreateNewExpectedChargeLink((Guid chargeId, Guid meteringPointId) ids)
        {
            var operation = new ChargeLinkOperation(ExpectedOperationId, ExpectedCorrelationId);

            var periodDetails = new ChargeLinkPeriodDetails(
                _expectedPeriodDetailsStartDateTime,
                ((Instant?)null).TimeOrEndDefault(),
                ExpectedOperationDetailsFactor,
                operation.Id);

            return new ChargeLink(
                ids.chargeId,
                ids.meteringPointId,
                new List<ChargeLinkOperation> { operation },
                new List<ChargeLinkPeriodDetails> { periodDetails });
        }

        private static (Guid chargeId, Guid meteringPointId) SeedDatabase(ChargesDatabaseContext context)
        {
            var marketParticipant = new MarketParticipant { Name = "Name", Role = 1, MarketParticipantId = "MarketParticipantId" };

            context.MarketParticipants.Add(marketParticipant);
            context.SaveChanges(); // Sets marketParticipant.RowId

            var charge = new Charges.Infrastructure.Context.Model.Charge
            {
                Currency = "DKK",
                SenderProvidedChargeId = "charge id",
                MarketParticipantId = marketParticipant.Id,
            };
            context.Charges.Add(charge);

            var meteringPoint = MeteringPoint.Create(
                "some-id",
                MeteringPointType.ElectricalHeating,
                "some-area-id",
                SystemClock.Instance.GetCurrentInstant(),
                ConnectionState.Connected,
                SettlementMethod.Flex);
            context.MeteringPoints.Add(meteringPoint);

            context.SaveChanges();

            return (charge.Id, meteringPoint.Id);
        }
    }
}
