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

using FluentAssertions;
using GreenEnergyHub.Charges.Tests.TestHelpers;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Domain.Dtos.Messages.Events
{
    [UnitTest]
    public class DomainEventTests
    {
        [Fact]
        public void InheritedClasses_AreNamedInInAccordanceWith_PresumedServiceBusFilters()
        {
            // Arrange
            var presumedServiceBusFilters = PresumedServiceBusFilterHelper
                .GetOrderedServiceBusFiltersPresumedToBeUsedInInfrastructureAsCode();
            var classNames = DomainEventTypeHelper.GetOrderedDomainEventTypeNames();

            // Act
            // Assert
            presumedServiceBusFilters.Should().BeEquivalentTo(
                classNames, because: CodeToInfrastructureCoherenceErrorMessageBuilder.CreateErrorMessage());
        }
    }
}
