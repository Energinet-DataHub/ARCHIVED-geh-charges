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

using System.Threading.Tasks;
using FluentAssertions;
using GreenEnergyHub.Charges.Core;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommands.Validation.BusinessValidation.ValidationRules;
using GreenEnergyHub.Charges.Infrastructure.Persistence.Repositories;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.IntegrationTests.IntegrationTests.Repositories
{
    /// <summary>
    /// Tests <see cref="RulesConfigurationRepository"/>
    /// - this repository is not (yet) using a database.
    /// </summary>
    [UnitTest]
    public class RulesConfigurationRepositoryTests
    {
        [Fact]
        public async Task GetConfigurationAsync_WhenCalled_ReturnsExpectedStartDateValidationRuleConfiguration()
        {
            // Arrange
            var sut = new RulesConfigurationRepository();
            var expected = new StartDateValidationRuleConfiguration(new Interval<int>(-720, 1095));

            // Act
            var actual = await sut.GetConfigurationAsync().ConfigureAwait(false);

            // Assert
            actual.StartDateValidationRuleConfiguration.Should().BeEquivalentTo(expected);
        }
    }
}
