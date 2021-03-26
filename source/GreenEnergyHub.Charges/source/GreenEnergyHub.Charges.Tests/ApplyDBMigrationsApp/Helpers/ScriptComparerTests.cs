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
using FluentAssertions;
using GreenEnergyHub.Charges.ApplyDBMigrationsApp.Helpers;
using Xunit;

namespace GreenEnergyHub.Charges.Tests.ApplyDBMigrationsApp.Helpers
{
    [Trait("Category", "Unit")]
    public class ScriptComparerTests
    {
        [Fact]
        public void Scripts_should_be_grouped_by_time_and_ordered_by_time_and_then_type()
        {
            // Arrange
            var scripts = new[]
            {
                "Energinet.DataHub.MarketData.ApplyDBMigrationsApp.Scripts.Model.202012310001 First.sql",
                "Energinet.DataHub.MarketData.ApplyDBMigrationsApp.Scripts.Model.202012310002 Second.sql",
                "Energinet.DataHub.MarketData.ApplyDBMigrationsApp.Scripts.Seed.202012310002 Second.sql",
                "Energinet.DataHub.MarketData.ApplyDBMigrationsApp.Scripts.Test.202012310002 Third.sql",
                "Energinet.DataHub.MarketData.ApplyDBMigrationsApp.Scripts.Model.202012310009 A Last one.sql",
                "Energinet.DataHub.MarketData.ApplyDBMigrationsApp.Scripts.Model.202012310008 Oops.sql",
                "Energinet.DataHub.MarketData.ApplyDBMigrationsApp.Scripts.Seed.202012310008 Oops seed.sql",
                "Energinet.DataHub.MarketData.ApplyDBMigrationsApp.Scripts.Model.202012310008 A oops duplicate.sql",
            };

            // Act
            var ordered = scripts.OrderBy(s => s, new ScriptComparer());

            // Assert
            ordered.Should().ContainInOrder(
                "Energinet.DataHub.MarketData.ApplyDBMigrationsApp.Scripts.Model.202012310001 First.sql",
                "Energinet.DataHub.MarketData.ApplyDBMigrationsApp.Scripts.Model.202012310002 Second.sql",
                "Energinet.DataHub.MarketData.ApplyDBMigrationsApp.Scripts.Seed.202012310002 Second.sql",
                "Energinet.DataHub.MarketData.ApplyDBMigrationsApp.Scripts.Test.202012310002 Third.sql",
                "Energinet.DataHub.MarketData.ApplyDBMigrationsApp.Scripts.Model.202012310008 A oops duplicate.sql",
                "Energinet.DataHub.MarketData.ApplyDBMigrationsApp.Scripts.Model.202012310008 Oops.sql",
                "Energinet.DataHub.MarketData.ApplyDBMigrationsApp.Scripts.Seed.202012310008 Oops seed.sql",
                "Energinet.DataHub.MarketData.ApplyDBMigrationsApp.Scripts.Model.202012310009 A Last one.sql");
        }
    }
}
