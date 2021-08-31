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
using System.Threading.Tasks;
using FluentAssertions;
using GreenEnergyHub.Charges.Domain.MeteringPoints;
using GreenEnergyHub.Charges.Infrastructure.Context;
using GreenEnergyHub.Charges.Infrastructure.Context.Model;
using GreenEnergyHub.Charges.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

using ChargeOperation = GreenEnergyHub.Charges.Infrastructure.Context.Model.ChargeOperation;
using DBDefaultChargeLinkSetting = GreenEnergyHub.Charges.Infrastructure.Context.Model.DefaultChargeLinkSetting;
using MarketParticipant = GreenEnergyHub.Charges.Infrastructure.Context.Model.MarketParticipant;

namespace GreenEnergyHub.Charges.Tests.Infrastructure.Repositories
{
    public class DefaultChargeLinkSettingRepositoryTests
    {
        private const string ExpectedChargeId = "2EF7AC02-08ED-4CBD-A0ED-33663BBC17BB";

        private readonly DbContextOptions<ChargesDatabaseContext> _dbContextOptions =
            new DbContextOptionsBuilder<ChargesDatabaseContext>()
                .UseSqlite("Filename=Test.db")
                .Options;

        [Fact]
        public async Task GetDefaultChargeLinks_WhenCalledWithMeteringPointType_ReturnsDefaultCharges()
        {
            // Arrange
            SeedDatabase();
            await using var chargesDatabaseContext = new ChargesDatabaseContext(_dbContextOptions);
            var sut = new DefaultChargeLinkSettingRepository(chargesDatabaseContext);

            // Act
            var actual = await
                sut.GetDefaultChargeLinkSettingAsync(MeteringPointType.Consumption).ConfigureAwait(false);

            // Assert
            Assert.NotNull(actual);
            actual.First().DefaultCharge.Id.Should().BeEquivalentTo(ExpectedChargeId);
        }

        private void SeedDatabase()
        {
            using var context = new ChargesDatabaseContext(_dbContextOptions);
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();
            var marketParticipant = context.MarketParticipants.Add(
                new MarketParticipant { Name = "Name", Role = 1, MarketParticipantId = "MarketParticipantId" });
            context.SaveChanges();
            var charge = context.Charges.Add(new Charge
            {
                Currency = "DKK",
                Resolution = 1,
                ChargeType = 1,
                TaxIndicator = 0,
                TransparentInvoicing = 1,
                ChargeId = ExpectedChargeId,
                MarketParticipantRowId = marketParticipant.Entity.RowId,
            });
            context.SaveChanges();
            var writeDateTime = DateTime.UtcNow;
            var chargeOperation = context.ChargeOperations.Add(new ChargeOperation
            {
                CorrelationId = "1BD3787A-EF15-41E8-BE83-191C42AC97D3",
                ChargeOperationId = "86EDF16A-5ADD-4C86-A90F-459B5AD0D1F1",
                WriteDateTime = writeDateTime,
                ChargeRowId = charge.Entity.RowId,
            });
            context.SaveChanges();
            context.ChargePeriodDetails.Add(new ChargePeriodDetails
            {
                Description = "Description",
                Name = "Name",
                Retired = false,
                VatClassification = 1,
                ChargeRowId = charge.Entity.RowId,
                StartDateTime = writeDateTime,
                ChargeOperationRowId = chargeOperation.Entity.RowId,
            });
            context.DefaultChargeLinkSetting.Add(
                new DBDefaultChargeLinkSetting
                    {
                        RowId = 1,
                        ChargeRowId = charge.Entity.RowId,
                        MeteringPointType = (int)MeteringPointType.Consumption,
                        StartDateTime = writeDateTime,
                    });
            context.SaveChanges();
        }
    }
}
