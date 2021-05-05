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
using GreenEnergyHub.Charges.Domain.ChangeOfCharges.Transaction;
using GreenEnergyHub.Charges.Infrastructure.Context;
using GreenEnergyHub.Charges.Infrastructure.Context.Model;
using GreenEnergyHub.Charges.Infrastructure.Mapping;
using GreenEnergyHub.Charges.Infrastructure.Repositories;
using GreenEnergyHub.TestHelpers;
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
        private const string KnownChargeType = "knownChargeType";
        private const string KnownResolutionType = "knownResolutionType";
        private const string KnownVatPayer = "knownVatPayer";
        private const string KnownChargeOwner = "knownChargeOwner";

        private readonly DbContextOptions<ChargesDatabaseContext> _dbContextOptions = new DbContextOptionsBuilder<ChargesDatabaseContext>()
            .UseSqlite("Filename=Test.db")
            .Options;

        [Theory]
        [InlineAutoDomainData("unknown", "NotUsed", "NotUsed", "NotUsed", "No charge type for unknown")]
        [InlineAutoDomainData(KnownChargeType, "unknown", "NotUsed", "NotUsed", "No resolution type for unknown")]
        [InlineAutoDomainData(KnownChargeType, KnownResolutionType, "unknown", "NotUsed", "No VAT payer type for unknown")]
        [InlineAutoDomainData(KnownChargeType, KnownResolutionType, KnownVatPayer, "unknown", "No market participant for unknown")]
        public async Task StoreChargeAsync_WhenValueNotFoundInDbContext_ThenFailureStatusReturnedAsync(string chargeType, string resolutionType, string vatPayerType, string chargeOwner, string failureReason)
        {
            // Arrange
            var message = GetValidCharge();
            message.Type = chargeType;
            message.ChargeTypeOwnerMRid = chargeOwner;
            message!.MktActivityRecord!.ChargeType!.VatPayer = vatPayerType;
            message!.Period!.Resolution = resolutionType;

            SeedDatabase();
            await using var chargesDatabaseContext = new ChargesDatabaseContext(_dbContextOptions);
            var sut = new ChargeRepository(chargesDatabaseContext);

            // Acy & Assert
            var ex = await Assert.ThrowsAsync<Exception>(async () => await sut.StoreChargeAsync(message).ConfigureAwait(false)).ConfigureAwait(false);
            Assert.Equal(failureReason, ex.Message);
        }

        #region Argument validation
        [Theory]
        [InlineAutoDomainData(null, "NotUsed", "NotUsed", "NotUsed", "Fails as Type is invalid")]
        [InlineAutoDomainData(" ", "NotUsed", "NotUsed", "NotUsed", "Fails as Type is invalid")]
        [InlineAutoDomainData(KnownChargeType, null, "NotUsed", "NotUsed", "Fails as Resolution is invalid")]
        [InlineAutoDomainData(KnownChargeType, " ", "NotUsed", "NotUsed", "Fails as Resolution is invalid")]
        [InlineAutoDomainData(KnownChargeType, KnownResolutionType, null, "NotUsed", "Fails as VatPayer is invalid")]
        [InlineAutoDomainData(KnownChargeType, KnownResolutionType, " ", "NotUsed", "Fails as VatPayer is invalid")]
        [InlineAutoDomainData(KnownChargeType, KnownResolutionType, KnownVatPayer, null, "Fails as ChargeTypeOwnerMRid is invalid")]
        [InlineAutoDomainData(KnownChargeType, KnownResolutionType, KnownVatPayer, " ", "Fails as ChargeTypeOwnerMRid is invalid")]
        public async Task StoreChargeAsync_WhenValuesInMessageUsedForDbContextLookupsAreInvalid_ThenExceptionThrownAsync(string chargeType, string resolutionType, string vatPayerType, string chargeOwner, string exceptionMessage)
        {
            // Arrange
            var message = GetValidCharge();
            message.Type = chargeType;
            message.ChargeTypeOwnerMRid = chargeOwner;
#pragma warning disable CS8602 // Dereference of a possibly null reference.
            message.MktActivityRecord.ChargeType.VatPayer = vatPayerType;
            message.Period.Resolution = resolutionType;
#pragma warning restore CS8602 // Dereference of a possibly null reference.

            SeedDatabase();
            await using var context = new ChargesDatabaseContext(_dbContextOptions);
            var sut = new ChargeRepository(context);

            // Act
            var exception = await Record.ExceptionAsync(async () => await sut.StoreChargeAsync(message).ConfigureAwait(false)).ConfigureAwait(false);

            // Assert
            Assert.IsType<ArgumentException>(exception);
            Assert.Contains(exceptionMessage, exception.Message);
        }

        [Theory]
        [InlineAutoDomainData(null, "Valid", "Valid", "Valid", "Valid", "ChargeTypeMRid")]
        [InlineAutoDomainData(" ", "Valid", "Valid", "Valid", "Valid", "ChargeTypeMRid")]
        [InlineAutoDomainData("Valid", null, "Valid", "Valid", "Valid", "CorrelationId")]
        [InlineAutoDomainData("Valid", " ", "Valid", "Valid", "Valid", "CorrelationId")]
        [InlineAutoDomainData("Valid", "Valid", null, "Valid", "Valid", "LastUpdatedBy")]
        [InlineAutoDomainData("Valid", "Valid", " ", "Valid", "Valid", "LastUpdatedBy")]
        [InlineAutoDomainData("Valid", "Valid", "Valid", null, "Valid", "Name")]
        [InlineAutoDomainData("Valid", "Valid", "Valid", " ", "Valid", "Name")]
        [InlineAutoDomainData("Valid", "Valid", "Valid", "Valid", null, "Description")]
        [InlineAutoDomainData("Valid", "Valid", "Valid", "Valid", " ", "Description")]
        public async Task StoreChargeAsync_WhenValuesInMessageAreInvalid_ThenExceptionThrownAsync(string chargeTypeMRid, string correlationId, string lastUpdatedBy, string shortDescription, string longDescription, string argumentThatFails)
        {
            // Arrange
            var message = GetValidCharge(correlationId);
            message.ChargeTypeMRid = chargeTypeMRid;
            message.LastUpdatedBy = lastUpdatedBy;
#pragma warning disable CS8602 // Dereference of a possibly null reference.
            message.MktActivityRecord.ChargeType.Name = shortDescription;
            message.MktActivityRecord.ChargeType.Description = longDescription;
#pragma warning restore CS8602 // Dereference of a possibly null reference.

            SeedDatabase();
            await using var context = new ChargesDatabaseContext(_dbContextOptions);
            var sut = new ChargeRepository(context);

            // Act
            var exception = await Record.ExceptionAsync(async () => await sut.StoreChargeAsync(message).ConfigureAwait(false)).ConfigureAwait(false);

            // Assert
            Assert.IsType<ArgumentException>(exception);
            Assert.Contains($"{argumentThatFails} must have value", exception.Message);
        }

        #endregion

        [Fact]
        public async Task StoreChargeAsync_WhenChargeIsSaved_ThenSuccessReturnedAsync()
        {
            // Arrange
            var message = GetValidCharge();

            SeedDatabase();
            await using var chargesDatabaseContext = new ChargesDatabaseContext(_dbContextOptions);
            var sut = new ChargeRepository(chargesDatabaseContext);

            // Act & Assert
            await sut.StoreChargeAsync(message).ConfigureAwait(false);
        }

        [Fact]
        public void MapChangeOfChargesMessageToCharge_WhenMessageWithProperties_ThenReturnsChargeWithPropertiesSet()
        {
            // Arrange
            var changeOfChargesMessage = GetValidCharge();
            changeOfChargesMessage!.MktActivityRecord!.ValidityEndDate = Instant.MaxValue;
            var chargeType = new ChargeType { Code = changeOfChargesMessage.Type, Id = 1, Name = "Name" };
            var chargeTypeOwnerMRid = new MarketParticipant { Id = 1, MRid = changeOfChargesMessage.ChargeTypeOwnerMRid, Name = "Name" };
            var resolutionType = new ResolutionType { Id = 1, Name = changeOfChargesMessage.Period.Resolution };
            var vatPayerType = new VatPayerType { Id = 1, Name = changeOfChargesMessage.MktActivityRecord.ChargeType.VatPayer };

            // When
            var result = ChangeOfChargesMapper.MapChangeOfChargesTransactionToCharge(changeOfChargesMessage, chargeType, chargeTypeOwnerMRid, resolutionType, vatPayerType);

            var properties = result.GetType().GetProperties();
            foreach (var property in properties)
            {
                var value = property.GetValue(result);
                Assert.NotNull(value);
            }
        }

        private static Charge GetValidCharge(string correlationId = "some-correlation-id")
        {
            var transaction = new Charge
            {
                ChargeTypeMRid = "chargeTypeMRid",
                CorrelationId = correlationId,
                LastUpdatedBy = "lastUpdatedBy",
                Type = KnownChargeType,
                ChargeTypeOwnerMRid = KnownChargeOwner,
                MktActivityRecord = new MktActivityRecord
                {
                    MRid = "MktActivtyRecordMRIDValue",
                    ChargeType = new Domain.ChangeOfCharges.Transaction.ChargeType
                        { VatPayer = KnownVatPayer, Name = "shortDescription", Description = "longDescription", },
                },
                Period = new ChargeTypePeriod()
                {
                    Resolution = KnownResolutionType,
                    Points = { new Point { Position = 1, PriceAmount = 1m, Time = SystemClock.Instance.GetCurrentInstant(), } },
                },
            };
            transaction.Period.AddPoints(new List<Point>
                    {
                    new Point
                    {
                        Position = 1, PriceAmount = 1m, Time = SystemClock.Instance.GetCurrentInstant(),
                    },
                    });
            return transaction;
        }

        private void SeedDatabase()
        {
            using var context = new ChargesDatabaseContext(_dbContextOptions);
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            var chargeTypes = new List<ChargeType> { new ChargeType { Code = KnownChargeType, Id = 1, } };
            context.AddRange(chargeTypes);

            var resolutionTypes = new List<ResolutionType> { new ResolutionType { Name = KnownResolutionType, Id = 1, } };
            context.AddRange(resolutionTypes);

            var vatPayerTypes = new List<VatPayerType> { new VatPayerType { Name = KnownVatPayer, Id = 1, } };
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
