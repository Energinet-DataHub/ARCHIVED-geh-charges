﻿// Copyright 2020 Energinet DataHub A/S
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
using GreenEnergyHub.Charges.FunctionHost;
using GreenEnergyHub.Charges.TestCore.TestHelpers;
using GreenEnergyHub.Charges.Tests.TestHelpers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.FunctionHost
{
    [UnitTest]
    public class FunctionAppServiceRegistrationTests
    {
        public FunctionAppServiceRegistrationTests(IConfiguration config)
        {
            FunctionHostEnvironmentSettingHelper.SetFunctionHostEnvironmentVariablesFromSampleSettingsFile(config);
        }

        [Fact]
        public void WhenApplicationIsConfigured_ThenAllApplicationDependenciesMustBeRegisteredOnlyOnce()
        {
            // Arrange
            var host = ChargesFunctionApp.ConfigureApplication();
            var functionAppDependencies = DependencyHelper.FindDependenciesForType(typeof(ChargesFunctionApp));
            var typesToResolve = new List<Type>();

            // Act
            foreach (var dependency in functionAppDependencies)
                typesToResolve.AddRange(DependencyHelper.GetConstructorParametersForDependency(dependency));

            // Assert
            foreach (var type in typesToResolve.Distinct())
            {
                var services = host.Services.GetServices(type);
                services.Count().Should().Be(1, $"Found more than one registered service of type {type}");
            }
        }
    }
}
