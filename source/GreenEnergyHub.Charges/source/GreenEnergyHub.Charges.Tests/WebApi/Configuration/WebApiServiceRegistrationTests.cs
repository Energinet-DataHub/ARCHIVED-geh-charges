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
using GreenEnergyHub.Charges.Tests.TestHelpers;
using GreenEnergyHub.Charges.WebApi;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.WebApi.Configuration
{
    [UnitTest]
    public class WebApiServiceRegistrationTests
    {
        [Fact]
        public void WhenWebApiIsConfigured_ThenAllApplicationDependenciesMustBeRegisteredOnlyOnce()
        {
            // Arrange
            var host = Program.CreateHostBuilder(null).Build();
            var dependencies = DependencyHelper.FindDependenciesForType(typeof(Program));

            // Act
            foreach (var dependency in dependencies)
            {
                var typesToResolveForDependency = DependencyHelper.GetConstructorParametersForDependency(dependency);

                // Assert
                foreach (var type in typesToResolveForDependency)
                {
                    var services = host.Services.GetServices(type);
                    services.Count().Should().Be(1, $"expected single registered service of type {type}");
                }
            }
        }
    }
}
