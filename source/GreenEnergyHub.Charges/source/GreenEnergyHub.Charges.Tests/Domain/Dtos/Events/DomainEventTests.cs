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
using FluentAssertions;
using GreenEnergyHub.Charges.Domain.Dtos.Events;
using GreenEnergyHub.Charges.Tests.TestHelpers;
using GreenEnergyHub.Charges.WebApi;
using Xunit;
using Xunit.Abstractions;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Domain.Dtos.Events
{
    [UnitTest]
    public class DomainEventTests
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public DomainEventTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        [Fact]
        public void InheritedClasses_AreNamedInInAccordanceWith_PresumedServiceBusFilters()
        {
            // Arrange
            var presumedServiceBusFilters = PresumedServiceBusFilterHelper
                .GetOrderedServiceBusFiltersPresumedToBeUsedInInfrastructureAsCode()
                .ToList();
            var classNames = DomainEventTypeHelper.GetOrderedDomainEventTypeNames().ToList();

            // Act
            // Assert
            try
            {
                presumedServiceBusFilters.Should().BeEquivalentTo(classNames);
            }
            catch (Exception)
            {
                _testOutputHelper.WriteLine(
                    "The failure of this test indicates a discrepancy between one or more names of classes " +
                    "inherited from {0}. Please ensure conformity in the following elements:",
                    nameof(DomainEvent));
                _testOutputHelper.WriteLine(" - 'func-functionhost.tf'");
                _testOutputHelper.WriteLine(" - 'sbt-charges-domain-events.tf'");
                _testOutputHelper.WriteLine(" - 'local.settings.sample.json'");
                _testOutputHelper.WriteLine(" - '{0}", nameof(EnvironmentSettingNames));
                _testOutputHelper.WriteLine(" - 'ChargesServiceBusResourceNames'");
                _testOutputHelper.WriteLine(" - '{0}", nameof(PresumedServiceBusFilterHelper
                        .GetOrderedServiceBusFiltersPresumedToBeUsedInInfrastructureAsCode));

                throw;
            }
        }
    }
}
