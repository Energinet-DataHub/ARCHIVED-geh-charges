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
using FluentAssertions;
using GreenEnergyHub.Charges.Domain.Dtos.Messages.Events;
using GreenEnergyHub.Charges.Tests.TestCore;
using GreenEnergyHub.Charges.WebApi;
using Xunit;
using Xunit.Abstractions;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Domain.Dtos.Messages.Events
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
        public void PresumedServiceBusFiltersAreNamedInInAccordanceWithInheritedClasses_Test()
        {
            // Arrange
            var presumedServiceBusFilters = GetOrderedServiceBusFiltersPresumedToBeUsedInInfrastructureAsCode();
            var classNames = GetOrderedDomainEventTypeNames();

            // Act
            // Assert
            try
            {
                presumedServiceBusFilters.Should().BeEquivalentTo(classNames);
            }
            catch (Exception)
            {
                _testOutputHelper.WriteLine(
                    "This test is failed due to a discrepancy between one or more names of classes inherited " +
                    "from {0}. Please ensure conformity in the following elements:",
                    nameof(DomainEvent));
                _testOutputHelper.WriteLine(" - 'func-functionhost.tf'");
                _testOutputHelper.WriteLine(" - 'sbt-charges-domain-events.tf'");
                _testOutputHelper.WriteLine(" - 'local.settings.sample.json'");
                _testOutputHelper.WriteLine(" - '{0}", nameof(EnvironmentSettingNames));
                _testOutputHelper.WriteLine(" - 'ChargesServiceBusResourceNames'");
                _testOutputHelper.WriteLine(" - '{0}", nameof(GetOrderedServiceBusFiltersPresumedToBeUsedInInfrastructureAsCode));

                throw;
            }
        }

        /// <summary>
        /// This method returns a list of names of all domain event filters we presume to to use in infrastructure-as-code:
        /// - func-functionhost.tf
        /// - sbt-charges-domain-events.tf
        /// </summary>
        /// <returns>List of all domain event filter names</returns>
        private static List<string> GetOrderedServiceBusFiltersPresumedToBeUsedInInfrastructureAsCode()
        {
            var presumedServiceBusFilters = new List<string>
            {
                "ChargePriceCommandReceivedEvent",
                "ChargeLinksRejectedEvent",
                "ChargeLinksReceivedEvent",
                "ChargeLinksDataAvailableNotifiedEvent",
                "ChargeLinksAcceptedEvent",
                "ChargeInformationCommandReceivedEvent",
                "ChargeInformationCommandRejectedEvent",
                "ChargeInformationCommandAcceptedEvent",
            };
            return presumedServiceBusFilters.OrderBy(x => x).ToList();
        }

        /// <summary>
        /// Return all non-abstract implementations of <see cref="DomainEvent"/> from domain assembly.
        /// </summary>
        private static IEnumerable<string> GetOrderedDomainEventTypeNames()
        {
            var domainAssembly = DomainAssemblyHelper.GetDomainAssembly();
            var messageTypes = domainAssembly
                .GetTypes()
                .Where(t => typeof(DomainEvent).IsAssignableFrom(t) && t.IsClass && !t.IsAbstract)
                .Select(t => t.Name)
                .ToList();
            return messageTypes.OrderBy(x => x).ToList();
        }
    }
}
