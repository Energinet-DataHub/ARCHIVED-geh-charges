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
using System.Linq;
using System.Threading.Tasks;
using Energinet.DataHub.Charges.Contracts.ChargeMessage;
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
using GreenEnergyHub.Iso8601;
using NodaTime;
using Xunit;
using Xunit.Categories;
using Charge = GreenEnergyHub.Charges.Domain.Charges.Charge;

namespace GreenEnergyHub.Charges.IntegrationTests.IntegrationTests.WebApi.QueryServices
{
    [IntegrationTest]
    public class ChargeMessagesQueryServiceTests : IClassFixture<ChargesDatabaseFixture>
    {
        private readonly ChargesDatabaseManager _databaseManager;

        public ChargeMessagesQueryServiceTests(ChargesDatabaseFixture fixture)
        {
            _databaseManager = fixture.DatabaseManager;
        }

        [Fact]
        public async Task GetAsync_WhenCalled_ReturnsMessagesBasedOnSearchCriteria()
        {
            // Arrange
            await using var chargesDatabaseWriteContext = _databaseManager.CreateDbContext();
            var expectedCharge = await CreateValidCharge(chargesDatabaseWriteContext);
            var chargeMessages = await CreateChargeMessages(chargesDatabaseWriteContext, expectedCharge);
            await chargesDatabaseWriteContext.SaveChangesAsync();

            await using var chargesDatabaseQueryContext = _databaseManager.CreateDbQueryContext();
            var sut = GetSut(chargesDatabaseQueryContext);
            var searchCriteria = new ChargeMessagesSearchCriteriaV1DtoBuilder()
                .WithChargeId(expectedCharge.Id)
                .WithFromDateTime(InstantHelper.GetTodayAtMidnightUtc().ToDateTimeOffset())
                .WithToDateTime(InstantHelper.GetTomorrowAtMidnightUtc().ToDateTimeOffset())
                .Build();

            // Act
            var actual = await sut.GetAsync(searchCriteria);

            // Assert
            var expectedMessageIds = chargeMessages
                .Where(cm => cm.SenderProvidedChargeId == expectedCharge.SenderProvidedChargeId)
                .Select(x => x.MessageId)
                .ToList();

            actual.MessageIds.Should().BeEquivalentTo(expectedMessageIds);
            actual.MessageIds.Should().NotContain("MessageId4");
        }

        [Fact]
        public void GetAsync_WhenSearchingHasSkip_ReturnsPrices()
        {
            // Arrange

            // Act

            // Assert
        }

        [Fact]
        public Task GetAsync_WhenSearchingHasNoSkip_ReturnsPrices()
        {
            // Arrange

            // Act

            // Assert
            return Task.CompletedTask;
        }

        [Fact]
        public Task GetAsync_WhenSortColumnNameIsNotValid_ReturnsPricesSortedByFromDateTime()
        {
            // Arrange

            // Act

            // Assert
            return Task.CompletedTask;
        }

        [Fact]
        public Task GetAsync_WhenSearchingIsDescendingOrderOnFromDateTime_ReturnsPricesInDescendingOrder()
        {
            // Arrange

            // Act

            // Assert
            return Task.CompletedTask;
        }

        [Fact]
        public Task GetAsync_WhenSearchingIsAscendingOrderOnFromDateTime_ReturnsPricesInAscendingOrder()
        {
            // Arrange

            // Act

            // Assert
            return Task.CompletedTask;
        }

        [Fact]
        public Task GetAsync_WhenSearchingIsAscendingOrderOnPrice_ReturnsPricesInAscendingOrder()
        {
            // Arrange

            // Act

            // Assert
            return Task.CompletedTask;
        }

        [Fact]
        public Task GetAsync_WhenSearchingIsDescendingOrderOnPrice_ReturnsPricesInDescendingOrder()
        {
            // Arrange

            // Act

            // Assert
            return Task.CompletedTask;
        }

        [Fact]
        public Task GetAsync_WhenSearching_ReturnsPricesInsideSearchDateInterval()
        {
            // Arrange

            // Act

            // Assert
            return Task.CompletedTask;
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
            IChargesDatabaseContext chargesDatabaseContext, Charge charge)
        {
            var chargeMessages = new List<Domain.Charges.ChargeMessage>
            {
                Domain.Charges.ChargeMessage.Create(
                    charge.SenderProvidedChargeId,
                    charge.Type,
                    "MarketParticipantId",
                    "MessageId1",
                    DocumentType.RequestChangeBillingMasterData,
                    SystemClock.Instance.GetCurrentInstant()),
                Domain.Charges.ChargeMessage.Create(
                    charge.SenderProvidedChargeId,
                    charge.Type,
                    "MarketParticipantId",
                    "MessageId2",
                    DocumentType.RequestChangeBillingMasterData,
                    SystemClock.Instance.GetCurrentInstant()),
                Domain.Charges.ChargeMessage.Create(
                    charge.SenderProvidedChargeId,
                    charge.Type,
                    "MarketParticipantId",
                    "MessageId3",
                    DocumentType.RequestChangeOfPriceList,
                    SystemClock.Instance.GetCurrentInstant()),
                Domain.Charges.ChargeMessage.Create(
                    "40000",
                    ChargeType.Tariff,
                    SeededData.MarketParticipants.SystemOperator.Gln,
                    "MessageId4",
                    DocumentType.RequestChangeOfPriceList,
                    SystemClock.Instance.GetCurrentInstant()),
            };

            await chargesDatabaseContext.ChargeMessages.AddRangeAsync(chargeMessages);

            return chargeMessages;
        }

        private static ChargeMessageQueryService GetSut(QueryDbContext chargesDatabaseQueryContext)
        {
            var data = new Data(chargesDatabaseQueryContext);
            var configuration = new Iso8601ConversionConfiguration("Europe/Copenhagen");
            var iso8601Durations = new Iso8601Durations(configuration);
            var sut = new ChargeMessageQueryService(data, iso8601Durations);
            return sut;
        }
    }
}
