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
using System.Reflection;
using FluentAssertions;
using GreenEnergyHub.Charges.FunctionHost;
using GreenEnergyHub.Charges.Tests.TestHelpers;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.FunctionHost
{
    [UnitTest]
    public class ServiceCollectionRegistrationTests
    {
        public ServiceCollectionRegistrationTests(IConfiguration config)
        {
            FunctionHostEnvironmentSettingHelper.SetFunctionHostEnvironmentVariablesFromSampleSettingsFile(config);
        }

        [Fact]
        public void WhenApplicationIsConfigured_ThenAllApplicationDependenciesMustBeRegistered()
        {
            // Arrange
            var program = new Program();

            // Act
            var host = program.ConfigureApplication();

            var assembly = typeof(Program).Assembly;
            var types = assembly.GetTypes();
            var functions = types.Where(type =>
                type.GetMethods().Any(method =>
                    method.IsDefined(typeof(FunctionAttribute))));

            foreach (var function in functions)
            {
                var constructorTypes = function
                    .GetConstructors()
                    .Single()
                    .GetParameters()
                    .Select(p => p.ParameterType);

                foreach (var constructorType in constructorTypes)
                {
                    // Assert
                    host.Services.GetService(constructorType).Should().NotBeNull();
                }
            }
        }
    }
}
