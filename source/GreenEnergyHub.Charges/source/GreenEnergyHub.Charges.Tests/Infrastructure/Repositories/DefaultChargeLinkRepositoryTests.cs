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

using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using GreenEnergyHub.Charges.Domain.Charges;
using GreenEnergyHub.Charges.Domain.MeteringPoints;
using GreenEnergyHub.Charges.Infrastructure.Repositories;
using GreenEnergyHub.Charges.TestCore.Squadron;
using Squadron;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Infrastructure.Repositories
{
    /// <summary>
    /// Tests <see cref="DefaultChargeLinkRepository"/> using an SQLite in-memory database.
    /// </summary>
    [UnitTest]
    public class DefaultChargeLinkRepositoryTests : IClassFixture<SqlServerResource<SqlServerOptions>>
    {
        private readonly SqlServerResource<SqlServerOptions> _resource;

        public DefaultChargeLinkRepositoryTests(SqlServerResource<SqlServerOptions> resource)
        {
            _resource = resource;
        }

        [Fact]
        public async Task GetDefaultChargeLinks_WhenCalledWithMeteringPointType_ReturnsDefaultCharges()
        {
            await using var chargesDatabaseContext = await SquadronContextFactory
                .GetDatabaseContextAsync(_resource)
                .ConfigureAwait(false);

            // Arrange
            var sut = new DefaultChargeLinkRepository(chargesDatabaseContext);
            var x = chargesDatabaseContext.DefaultChargeLinks.ToList();

            // Act
            var actual = await
                sut.GetAsync(MeteringPointType.Consumption).ConfigureAwait(false);

            // Assert
            var actualDefaultChargeLinkSettings =
                actual as Charges.Domain.DefaultChargeLinks.DefaultChargeLink[] ?? actual.ToArray();

            actualDefaultChargeLinkSettings.Should().NotBeNullOrEmpty();
        }
    }
}
