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
    public class EnvironmentFilterTests
    {
        [Theory]
        [InlineData(1, "")]
        [InlineData(1, "hereGoesNothing")]
        [InlineData(2, "includeSeedData")]
        [InlineData(2, "includeTestData")]
        [InlineData(3, "includeTestData", "includeSeedData")]
        public void Scripts_are_included_when_parameter_is_added(int expectedNumberOfScriptsIncluded, params string[] args)
        {
            // Arrange
            var filter = EnvironmentFilter.GetFilter(args);
            var scripts = new[]
            {
                "Energinet.DataHub.MarketData.ApplyDBMigrationsApp.Scripts.Model.Script 1.sql",
                "Energinet.DataHub.MarketData.ApplyDBMigrationsApp.Scripts.Seed.Script 2.sql",
                "Energinet.DataHub.MarketData.ApplyDBMigrationsApp.Scripts.Test.Script 3.sql",
            };

            // Act
            var actualNumberOfScriptsIncluded = scripts.Select(script => filter.Invoke(script))
                .Where(filtered => filtered)
                .ToList();

            // Assert
            actualNumberOfScriptsIncluded.Should().HaveCount(expectedNumberOfScriptsIncluded);
        }

        [Theory]
        [InlineData(true, "Energinet.DataHub.MarketData.ApplyDBMigrationsApp.Scripts.Model.Script 1.sql", "")]
        [InlineData(true, "Energinet.DataHub.MarketData.ApplyDBMigrationsApp.Scripts.Seed.Script 2.sql", "includeSeedData")]
        [InlineData(true, "Energinet.DataHub.MarketData.ApplyDBMigrationsApp.Scripts.Test.Script 3.sql", "includeTestData")]
        [InlineData(false, "Energinet.DataHub.MarketData.ApplyDBMigrationsApp.Scripts.Seed.Script 2.sql", "includeTestData")]
        [InlineData(false, "Energinet.DataHub.MarketData.ApplyDBMigrationsApp.Scripts.Test.Script 3.sql", "includeSeedData")]
        [InlineData(true, "Energinet.DataHub.MarketData.ApplyDBMigrationsApp.Scripts.Seed.Script 2.sql", "includeSeedData", "includeTestData")]
        [InlineData(true, "Energinet.DataHub.MarketData.ApplyDBMigrationsApp.Scripts.Test.Script 3.sql", "includeSeedData", "includeTestData")]
        [InlineData(false, "Energinet.DataHub.MarketData.ApplyDBMigrationsApp.Scripts.Seed.Script 2.sql", "")]
        [InlineData(false, "Energinet.DataHub.MarketData.ApplyDBMigrationsApp.Scripts.Test.Script 3.sql", "")]
        [InlineData(true, "Energinet.DataHub.MarketData.ApplyDBMigrationsApp.Scripts.Model.Script 1.sql", "includeSeedData")]
        [InlineData(true, "Energinet.DataHub.MarketData.ApplyDBMigrationsApp.Scripts.Model.Script 1.sql", "includeSeedData", "includeTestData")]
        public void Script_is_included_when_parameters_match(bool expectedToRun, string scriptFile, params string[] args)
        {
            // Arrange
            var filter = EnvironmentFilter.GetFilter(args);

            // Act
            var actual = filter.Invoke(scriptFile);

            // Assert
            actual.Should().Be(expectedToRun);
        }
    }
}
