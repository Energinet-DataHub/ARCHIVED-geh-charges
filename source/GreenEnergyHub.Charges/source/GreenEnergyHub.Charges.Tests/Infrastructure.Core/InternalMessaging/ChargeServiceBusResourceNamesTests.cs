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
using GreenEnergyHub.Charges.FunctionHost.Common;
using GreenEnergyHub.Charges.Infrastructure.Core.InternalMessaging;
using GreenEnergyHub.Charges.Tests.TestHelpers;
using Microsoft.Extensions.Configuration;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Infrastructure.Core.InternalMessaging
{
    [UnitTest]
    public class ChargeServiceBusResourceNamesTests
    {
        public ChargeServiceBusResourceNamesTests(IConfiguration config)
        {
            FunctionHostEnvironmentSettingHelper.SetFunctionHostEnvironmentVariablesFromSampleSettingsFile(config);
        }

        [Fact]
        public void AllDomainEventChargesServiceBusResourceNames_ShouldHaveEquivalent_DomainEventSettingNames()
        {
            // Arrange
            var domainEventSettings = DomainEventSettingHelper.GetDomainEventSettings(typeof(EnvironmentSettingNames));
            var domainEventChargesServiceBusResourceNames =
                DomainEventSettingHelper.GetDomainEventSettings(typeof(ChargesServiceBusResourceNames));
            var settings = domainEventSettings
                .Select(domainEventEnvironmentSettingName =>
                    Environment.GetEnvironmentVariable(domainEventEnvironmentSettingName.Value)).ToList();

            // Act
            // Assert
            foreach (var domainEventChargesServiceBusResourceName in domainEventChargesServiceBusResourceNames)
                settings.Should().Contain(domainEventChargesServiceBusResourceName.Value);
        }
    }
}
