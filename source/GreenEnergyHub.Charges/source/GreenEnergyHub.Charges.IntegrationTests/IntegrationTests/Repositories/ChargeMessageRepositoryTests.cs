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
using System.Threading.Tasks;
using FluentAssertions;
using GreenEnergyHub.Charges.Domain.Charges;
using GreenEnergyHub.Charges.Infrastructure.Persistence;
using GreenEnergyHub.Charges.Infrastructure.Persistence.Repositories;
using GreenEnergyHub.Charges.IntegrationTest.Core.Fixtures.Database;
using GreenEnergyHub.Charges.TestCore;
using GreenEnergyHub.Charges.TestCore.Attributes;
using GreenEnergyHub.Charges.TestCore.Builders.Command;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.IntegrationTests.IntegrationTests.Repositories
{
    /// <summary>
    /// Tests <see cref="ChargeMessageRepository"/> using a database.
    /// </summary>
    [IntegrationTest]
    public class ChargeMessageRepositoryTests : IClassFixture<ChargesDatabaseFixture>
    {
        private readonly ChargesDatabaseManager _databaseManager;

        public ChargeMessageRepositoryTests(ChargesDatabaseFixture fixture)
        {
            _databaseManager = fixture.DatabaseManager;
        }

        [Theory]
        [InlineAutoMoqData]
        public async Task AddAsync_WhenChargeMessageIsValid_ChargeMessageIsAdded(
            ChargeMessageBuilder chargeMessageBuilder)
        {
            // Arrange
            await using var chargesDatabaseWriteContext = _databaseManager.CreateDbContext();
            var charge = await chargesDatabaseWriteContext.Charges.FirstAsync();
            var chargeIdentifier = await GetChargeIdentifier(
                chargesDatabaseWriteContext,
                charge,
                SeededData.MarketParticipants.SystemOperator.Gln);
            var chargeMessage = chargeMessageBuilder
                .WithSenderProvidedChargeId(chargeIdentifier.SenderProvidedChargeId)
                .WithChargeType(chargeIdentifier.ChargeType)
                .WithMarketParticipantId(SeededData.MarketParticipants.SystemOperator.Gln)
                .Build();
            var sut = new ChargeMessageRepository(chargesDatabaseWriteContext);

            // Act
            await sut.AddAsync(chargeMessage);
            await chargesDatabaseWriteContext.SaveChangesAsync();

            // Assert
            await using var chargesDatabaseReadContext = _databaseManager.CreateDbContext();
            var actual = await chargesDatabaseReadContext.ChargeMessages.SingleAsync(x => x.Id == chargeMessage.Id);
            actual.Should().BeEquivalentTo(chargeMessage);
        }

        [Theory]
        [InlineAutoMoqData]
        public async Task AddAsync_WhenChargeIsNull_ThrowsArgumentNullException(ChargeMessageRepository sut)
        {
            // Arrange
            ChargeMessage? chargeMessage = null;

            // Act / Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => sut.AddAsync(chargeMessage!));
        }

        private static async Task<ChargeIdentifier> GetChargeIdentifier(
            IChargesDatabaseContext chargesDatabaseWriteContext,
            Charge charge,
            string chargeOwner)
        {
            var marketParticipantRepository = new MarketParticipantRepository(chargesDatabaseWriteContext);
            var chargeIdentifierFactory = new ChargeIdentifierFactory(marketParticipantRepository);
            var chargeIdentifier = await chargeIdentifierFactory.CreateAsync(
                charge.SenderProvidedChargeId, charge.Type, chargeOwner).ConfigureAwait(false);
            return chargeIdentifier;
        }
    }
}
