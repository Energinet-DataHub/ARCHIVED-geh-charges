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
using System.Threading.Tasks;
using GreenEnergyHub.Charges.Domain.ChangeOfCharges.Transaction;
using GreenEnergyHub.Charges.Domain.Common;
using GreenEnergyHub.Charges.Infrastructure.Context;
using GreenEnergyHub.Charges.Infrastructure.Context.Model;
using GreenEnergyHub.Charges.Infrastructure.Mapping;
using GreenEnergyHub.Charges.Infrastructure.Repositories;
using GreenEnergyHub.TestHelpers.Traits;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using Xunit;
using Charge = GreenEnergyHub.Charges.Domain.Charge;
using ChargeType = GreenEnergyHub.Charges.Infrastructure.Context.Model.ChargeType;
using MarketParticipant = GreenEnergyHub.Charges.Infrastructure.Context.Model.MarketParticipant;

namespace GreenEnergyHub.Charges.Tests.Infrastructure.Repositories
{
    /// <summary>
    /// Tests <see cref="ChargeRepository"/> using an SQLite in-memory database.
    /// </summary>
    [Trait(TraitNames.Category, TraitValues.UnitTest)]
    public class ChargeRepositoryTest
    {
        private const string KnownChargeOwner = "knownChargeOwner";

        private readonly DbContextOptions<ChargesDatabaseContext> _dbContextOptions = new DbContextOptionsBuilder<ChargesDatabaseContext>()
            .UseSqlite("Filename=Test.db")
            .Options;

        // TODO: LRN: We (PTA) are unsure if these tests are needed after we redesign our DB schema.
        // [Theory]
        // [InlineAutoDomainData("unknown", "NotUsed", "NotUsed", "NotUsed", "No charge type for unknown")]
        // [InlineAutoDomainData(KnownChargeType, "unknown", "NotUsed", "NotUsed", "No resolution type for unknown")]
        // [InlineAutoDomainData(KnownChargeType, KnownResolutionType, "unknown", "NotUsed", "No VAT payer type for unknown")]
        // [InlineAutoDomainData(KnownChargeType, KnownResolutionType, KnownVatPayer, "unknown", "No market participant for unknown")]
        // public async Task StoreChargeAsync_WhenValueNotFoundInDbContext_ThenFailureStatusReturnedAsync(string chargeType, string resolutionType, string vatPayerType, string chargeOwner, string failureReason)
        // {
        //     // Arrange
        //     var charge = GetValidCharge();
        //     charge.ChargeNew.Type = chargeType;
        //     charge.ChargeNew.Owner = chargeOwner;
        //     charge.ChargeNew.Vat = vatPayerType;
        //     charge.ChargeNew.Resolution = resolutionType;
        //
        //     SeedDatabase();
        //     await using var chargesDatabaseContext = new ChargesDatabaseContext(_dbContextOptions);
        //     var sut = new ChargeRepository(chargesDatabaseContext);
        //
        //     // Acy & Assert
        //     var ex = await Assert.ThrowsAsync<Exception>(async () => await sut.StoreChargeAsync(charge).ConfigureAwait(false)).ConfigureAwait(false);
        //     Assert.Equal(failureReason, ex.Message);
        // }
        //
        // #region Argument validation
        // [Theory]
        // [InlineAutoDomainData(null, "NotUsed", "NotUsed", "NotUsed", "Fails as Type is invalid")]
        // [InlineAutoDomainData(" ", "NotUsed", "NotUsed", "NotUsed", "Fails as Type is invalid")]
        // [InlineAutoDomainData(KnownChargeType, null, "NotUsed", "NotUsed", "Fails as Resolution is invalid")]
        // [InlineAutoDomainData(KnownChargeType, " ", "NotUsed", "NotUsed", "Fails as Resolution is invalid")]
        // [InlineAutoDomainData(KnownChargeType, KnownResolutionType, null, "NotUsed", "Fails as Vat is invalid")]
        // [InlineAutoDomainData(KnownChargeType, KnownResolutionType, " ", "NotUsed", "Fails as Vat is invalid")]
        // [InlineAutoDomainData(KnownChargeType, KnownResolutionType, KnownVatPayer, null, "Fails as Owner is invalid")]
        // [InlineAutoDomainData(KnownChargeType, KnownResolutionType, KnownVatPayer, " ", "Fails as Owner is invalid")]
        // public async Task StoreChargeAsync_WhenValuesInMessageUsedForDbContextLookupsAreInvalid_ThenExceptionThrownAsync(string chargeType, string resolutionType, string vatPayerType, string chargeOwner, string exceptionMessage)
        // {
        //     // Arrange
        //     var charge = GetValidCharge();
        //     charge.ChargeNew.Type = chargeType;
        //     charge.ChargeNew.Owner = chargeOwner;
        //     charge.ChargeNew.Vat = vatPayerType;
        //     charge.ChargeNew.Resolution = resolutionType;
        //
        //     SeedDatabase();
        //     await using var context = new ChargesDatabaseContext(_dbContextOptions);
        //     var sut = new ChargeRepository(context);
        //
        //     // Act
        //     var exception = await Record.ExceptionAsync(async () => await sut.StoreChargeAsync(charge).ConfigureAwait(false)).ConfigureAwait(false);
        //
        //     // Assert
        //     Assert.IsType<ArgumentException>(exception);
        //     Assert.Contains(exceptionMessage, exception.Message);
        // }
        // [Theory]
        // [InlineAutoDomainData(null, "Valid", "Valid", "Valid")]
        // [InlineAutoDomainData("Valid", null, "Valid", "Valid")]
        // [InlineAutoDomainData("Valid", "Valid", null, "Valid")]
        // [InlineAutoDomainData("Valid", "Valid", "Valid", null)]
        // public async Task StoreChargeAsync_WhenValuesInMessageAreInvalid_ThenExceptionThrownAsync(
        //     string chargeId,
        //     string correlationId,
        //     string lastUpdatedBy,
        //     string name)
        // {
        //     // Arrange
        //     var charge = GetValidCharge();
        //     charge.ChargeNew.Id = chargeId;
        //     charge.ChargeEvent.CorrelationId = correlationId;
        //     charge.ChargeEvent.LastUpdatedBy = lastUpdatedBy;
        //     charge.ChargeNew.Name = name;
        //
        //     SeedDatabase();
        //     await using var context = new ChargesDatabaseContext(_dbContextOptions);
        //     var sut = new ChargeRepository(context);
        //
        //     // Act
        //     var exception = await Record.ExceptionAsync(async () => await sut.StoreChargeAsync(charge).ConfigureAwait(false)).ConfigureAwait(false);
        //
        //     // Assert
        //     Assert.IsType<DbUpdateException>(exception);
        // }
        [Fact]
        public async Task StoreChargeAsync_WhenChargeIsSaved_ThenSuccessReturnedAsync()
        {
            // Arrange
            var charge = GetValidCharge();

            SeedDatabase();
            await using var chargesDatabaseContext = new ChargesDatabaseContext(_dbContextOptions);
            var sut = new ChargeRepository(chargesDatabaseContext);

            // Act & Assert
            await sut.StoreChargeAsync(charge).ConfigureAwait(false);
        }

        [Fact]
        public void MapChangeOfChargesMessageToCharge_WhenMessageWithProperties_ThenReturnsChargeWithPropertiesSet()
        {
            // Arrange
            var charge = GetValidCharge();
            charge.StartDateTime = Instant.MinValue;
            charge.EndDateTime = Instant.MaxValue;
            var chargeType = new ChargeType { Code = charge.Type.ToString(), Id = 1, Name = "Name" };
            var chargeTypeOwnerMRid = new MarketParticipant { Id = 1, MRid = charge.Owner, Name = "Name" };
            var resolutionType = new ResolutionType { Id = 1, Name = charge.Resolution.ToString() };
            var vatPayerType = new VatPayerType { Id = 1, Name = charge.VatClassification.ToString() };

            // When
            var result = ChangeOfChargesMapper.MapDomainChargeToCharge(charge, chargeType, chargeTypeOwnerMRid, resolutionType, vatPayerType);

            var properties = result.GetType().GetProperties();
            foreach (var property in properties)
            {
                var value = property.GetValue(result);
                Assert.NotNull(value);
            }
        }

        private static Charge GetValidCharge()
        {
            var transaction = new Charge
            {
                Name = "description",
                Id = "Id",
                Owner = KnownChargeOwner,
                StartDateTime = SystemClock.Instance.GetCurrentInstant(),
                Points = new List<Point>
                    {
                        new Point { Position = 0, Time = SystemClock.Instance.GetCurrentInstant(), Price = 200m },
                    },
                Resolution = Resolution.P1D,
                Type = Domain.ChangeOfCharges.Transaction.ChargeType.Fee,
                VatClassification = VatClassification.NoVat,
                Description = "LongDescription",

                Document = new Document
                {
                    Id = "id",
                    CorrelationId = "CorrelationId",
                    RequestDate = SystemClock.Instance.GetCurrentInstant(),
                    Type = DocumentType.RequestUpdateChargeInformation,
                    IndustryClassification = IndustryClassification.Electricity,
                    CreatedDateTime = SystemClock.Instance.GetCurrentInstant(),
                },
                ChargeOperationId = "id",
                Status = OperationType.Addition,
                BusinessReasonCode = BusinessReasonCode.UpdateChargeInformation,
                LastUpdatedBy = "LastUpdatedBy",
            };
            return transaction;
        }

        private void SeedDatabase()
        {
            using var context = new ChargesDatabaseContext(_dbContextOptions);
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            var chargeTypes = new List<ChargeType> { new ChargeType { Name = "Fee", Id = 1, } };
            context.AddRange(chargeTypes);

            var resolutionTypes = new List<ResolutionType> { new ResolutionType { Name = "P1D", Id = 1, } };
            context.AddRange(resolutionTypes);

            var vatPayerTypes = new List<VatPayerType> { new VatPayerType { Name = "D01", Id = 1, } };
            context.AddRange(vatPayerTypes);

            var chargeOwners = new List<MarketParticipant>
            {
                new MarketParticipant
                {
                    MRid = KnownChargeOwner,
                    Id = 1,
                    Name = "Name",
                    Role = "Role",
                },
            };
            context.AddRange(chargeOwners);

            context.SaveChanges();
        }
    }
}
