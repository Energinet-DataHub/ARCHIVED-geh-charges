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
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using GreenEnergyHub.Charges.Domain.Charges;
using GreenEnergyHub.Charges.Domain.Dtos.SharedDtos;
using GreenEnergyHub.Charges.Infrastructure.Persistence;
using GreenEnergyHub.Charges.IntegrationTest.Core.Fixtures.Database;
using GreenEnergyHub.Charges.QueryApi;
using GreenEnergyHub.Charges.QueryApi.Model;
using GreenEnergyHub.Charges.QueryApi.QueryServices;
using GreenEnergyHub.Charges.TestCore.Builders.Command;
using GreenEnergyHub.Charges.TestCore.Builders.Query;
using GreenEnergyHub.Charges.TestCore.Data;
using GreenEnergyHub.Charges.TestCore.TestHelpers;
using Microsoft.EntityFrameworkCore.SqlServer.NodaTime.Extensions;
using NodaTime;
using Xunit;
using Xunit.Categories;
using Charge = GreenEnergyHub.Charges.Domain.Charges.Charge;

namespace GreenEnergyHub.Charges.IntegrationTests.IntegrationTests.WebApi.QueryServices
{
    [IntegrationTest]
    public class ChargeMessagesQueryServiceTests : IClassFixture<ChargesDatabaseFixture>
    {
        private const string MarketParticipantId = "MarketParticipantId";
        private readonly ChargesDatabaseManager _databaseManager;
        private readonly Instant _now = SystemClock.Instance.GetCurrentInstant();

        public ChargeMessagesQueryServiceTests(ChargesDatabaseFixture fixture)
        {
            _databaseManager = fixture.DatabaseManager;
        }

        [Fact]
        public async Task SearchAsync_WhenCalled_ReturnsMessagesBasedOnSearchCriteria()
        {
            // Arrange
            await using var chargesDatabaseWriteContext = _databaseManager.CreateDbContext();
            var expectedCharge = await CreateValidCharge(chargesDatabaseWriteContext);
            var chargeMessages = await CreateChargeMessages(chargesDatabaseWriteContext, expectedCharge, _now);
            await chargesDatabaseWriteContext.SaveChangesAsync();

            await using var chargesDatabaseQueryContext = _databaseManager.CreateDbQueryContext();
            var sut = GetSut(chargesDatabaseQueryContext);

            var searchFromDateTime = _now.PlusSeconds(1);
            var searchToDateTime = _now.PlusDays(1);

            var searchCriteria = new ChargeMessagesSearchCriteriaV1DtoBuilder()
                .WithChargeId(expectedCharge.Id)
                .WithFromDateTime(searchFromDateTime.ToDateTimeOffset())
                .WithToDateTime(searchToDateTime.ToDateTimeOffset())
                .Build();

            // Act
            var actual = await sut.SearchAsync(searchCriteria);

            // Assert
            var expectedMessageIds = chargeMessages
                .Where(cm => cm.SenderProvidedChargeId == expectedCharge.SenderProvidedChargeId
                             && cm.Type == expectedCharge.Type
                             && cm.MarketParticipantId == MarketParticipantId)
                .Where(cm => cm.MessageDateTime >= searchFromDateTime
                             && cm.MessageDateTime < searchToDateTime.PlusDays(1))
                .Select(x => x.MessageId)
                .ToList();

            actual.TotalCount.Should().Be(3);
            actual.ChargeMessages.Select(x => x.MessageId).Should().ContainInOrder(expectedMessageIds);
        }

        [Fact]
        public async Task SearchAsync_WhenCalled_ReturnsNoMessagesBasedOnSearchCriteria()
        {
            // Arrange
            await using var chargesDatabaseWriteContext = _databaseManager.CreateDbContext();
            var expectedCharge = await CreateValidCharge(chargesDatabaseWriteContext);
            await chargesDatabaseWriteContext.SaveChangesAsync();

            await using var chargesDatabaseQueryContext = _databaseManager.CreateDbQueryContext();
            var sut = GetSut(chargesDatabaseQueryContext);
            var searchCriteria = new ChargeMessagesSearchCriteriaV1DtoBuilder()
                .WithChargeId(expectedCharge.Id)
                .WithFromDateTime(InstantHelper.GetTodayAtMidnightUtc().ToDateTimeOffset())
                .WithToDateTime(InstantHelper.GetTomorrowAtMidnightUtc().ToDateTimeOffset())
                .Build();

            // Act
            var actual = await sut.SearchAsync(searchCriteria);

            // Assert
            actual.TotalCount.Should().Be(0);
            actual.ChargeMessages.Select(x => x.MessageId).Should().ContainInOrder(new List<string>());
        }

        [Fact]
        public void SearchAsync_WhenSearchingHasSkip_ReturnsMessages()
        {
            // Arrange

            // Act

            // Assert
        }

        [Fact]
        public Task SearchAsync_WhenSearchingHasNoSkip_ReturnsMessages()
        {
            // Arrange

            // Act

            // Assert
            return Task.CompletedTask;
        }

        [Fact]
        public Task SearchAsync_WhenSortColumnNameIsNotValid_ReturnsMessagesSortedByFromDateTime()
        {
            // Arrange

            // Act

            // Assert
            return Task.CompletedTask;
        }

        [Fact]
        public Task SearchAsync_WhenSearchingIsDescendingOrderOnMessageDateTime_ReturnsMessagesInDescendingOrder()
        {
            // Arrange

            // Act

            // Assert
            return Task.CompletedTask;
        }

        [Fact]
        public Task SearchAsync_WhenSearchingIsAscendingOrderOnMessageDateTime_ReturnsMessagesInAscendingOrder()
        {
            // Arrange

            // Act

            // Assert
            return Task.CompletedTask;
        }

        [Fact]
        public Task SearchAsync_WhenSearchingIsAscendingOrderOnMessageId_ReturnsMessagesInAscendingOrder()
        {
            // Arrange

            // Act

            // Assert
            return Task.CompletedTask;
        }

        [Fact]
        public Task SearchAsync_WhenSearchingIsDescendingOrderOnMessageId_ReturnsMessagesInDescendingOrder()
        {
            // Arrange

            // Act

            // Assert
            return Task.CompletedTask;
        }

        [Fact]
        public Task SearchAsync_WhenSearchingIsAscendingOrderOnMessageType_ReturnsMessagesInAscendingOrder()
        {
            // Arrange

            // Act

            // Assert
            return Task.CompletedTask;
        }

        [Fact]
        public Task SearchAsync_WhenSearchingIsDescendingOrderOnMessageType_ReturnsMessagesInDescendingOrder()
        {
            // Arrange

            // Act

            // Assert
            return Task.CompletedTask;
        }

        [Fact]
        public Task SearchAsync_WhenSearching_ReturnsMessagesInsideSearchDateInterval()
        {
            // Arrange

            // Act

            // Assert
            return Task.CompletedTask;
        }

        [Fact]
        public async Task SearchAsync_WhenChargeDoesntExist_ThrowsArgumentException()
        {
            // Arrange
            await using var chargesDatabaseQueryContext = _databaseManager.CreateDbQueryContext();
            var sut = GetSut(chargesDatabaseQueryContext);
            var searchCriteria = new ChargeMessagesSearchCriteriaV1DtoBuilder()
                .WithChargeId(Guid.NewGuid())
                .Build();

            // Act / Assert
            await Assert.ThrowsAsync<ArgumentException>(() => sut.SearchAsync(searchCriteria));
        }

        private static async Task<Charge> CreateValidCharge(ChargesDatabaseContext chargesDatabaseWriteContext)
        {
            var marketParticipantId =
                await QueryServiceAutoMoqDataFixer.GetOrAddMarketParticipantAsync(chargesDatabaseWriteContext);

            var charge = new ChargeBuilder()
                .WithStartDate(InstantHelper.GetTodayAtMidnightUtc())
                .WithOwnerId(marketParticipantId)
                .WithMarketParticipantRole(MarketParticipantRole.SystemOperator)
                .WithSenderProvidedChargeId($"ChgId{Guid.NewGuid().ToString("n")[..10]}")
                .Build();

            chargesDatabaseWriteContext.Charges.Add(charge);
            return charge;
        }

        private static async Task<IList<Domain.Charges.ChargeMessage>> CreateChargeMessages(
            IChargesDatabaseContext chargesDatabaseContext, Charge charge, Instant now)
        {
            var chargeMessages = new List<Domain.Charges.ChargeMessage>
            {
                Domain.Charges.ChargeMessage.Create(
                    charge.SenderProvidedChargeId,
                    charge.Type,
                    MarketParticipantId,
                    "MessageId1",
                    DocumentType.RequestChangeBillingMasterData,
                    now),
                Domain.Charges.ChargeMessage.Create(
                    charge.SenderProvidedChargeId,
                    charge.Type,
                    MarketParticipantId,
                    "MessageId2",
                    DocumentType.RequestChangeBillingMasterData,
                    now.Plus(Duration.FromSeconds(1))),
                Domain.Charges.ChargeMessage.Create(
                    charge.SenderProvidedChargeId,
                    charge.Type,
                    MarketParticipantId,
                    "MessageId3",
                    DocumentType.RequestChangeOfPriceList,
                    now.Plus(Duration.FromDays(1))),
                Domain.Charges.ChargeMessage.Create(
                    charge.SenderProvidedChargeId,
                    charge.Type,
                    MarketParticipantId,
                    "MessageId4",
                    DocumentType.RequestChangeOfPriceList,
                    now.Plus(Duration.FromDays(2)).Plus(Duration.FromSeconds(-1))),
                Domain.Charges.ChargeMessage.Create(
                    charge.SenderProvidedChargeId,
                    charge.Type,
                    MarketParticipantId,
                    "MessageId5",
                    DocumentType.RequestChangeOfPriceList,
                    now.Plus(Duration.FromDays(2))),

                // Does not match SenderProvidedChargeId
                Domain.Charges.ChargeMessage.Create(
                    "TestFee",
                    charge.Type,
                    MarketParticipantId,
                    "MessageId6",
                    DocumentType.RequestChangeOfPriceList,
                    now.Plus(Duration.FromSeconds(3))),

                // Does not match charge type
                Domain.Charges.ChargeMessage.Create(
                    charge.SenderProvidedChargeId,
                    ChargeType.Fee,
                    MarketParticipantId,
                    "MessageId7",
                    DocumentType.RequestChangeOfPriceList,
                    now.Plus(Duration.FromSeconds(3))),

                // Does not match owner (MarketParticipantId)
                Domain.Charges.ChargeMessage.Create(
                    charge.SenderProvidedChargeId,
                    ChargeType.Fee,
                    "OtherMarketParticipantId",
                    "MessageId1",
                    DocumentType.RequestChangeOfPriceList,
                    now.Plus(Duration.FromSeconds(3))),

                // Other charge, other owner
                Domain.Charges.ChargeMessage.Create(
                    "40000",
                    ChargeType.Tariff,
                    SeededData.MarketParticipants.SystemOperator.Gln,
                    "MessageId1",
                    DocumentType.RequestChangeOfPriceList,
                    now.Plus(Duration.FromSeconds(1))),
            };

            await chargesDatabaseContext.ChargeMessages.AddRangeAsync(chargeMessages);

            return chargeMessages;
        }

        private static ChargeMessageQueryService GetSut(QueryDbContext chargesDatabaseQueryContext)
        {
            var data = new Data(chargesDatabaseQueryContext);
            var sut = new ChargeMessageQueryService(data);
            return sut;
        }
    }
}
