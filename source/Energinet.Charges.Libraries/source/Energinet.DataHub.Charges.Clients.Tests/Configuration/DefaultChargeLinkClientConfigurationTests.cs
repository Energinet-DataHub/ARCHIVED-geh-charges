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
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Azure.Messaging.ServiceBus;
using Energinet.DataHub.Charges.Libraries.Configuration;
using Energinet.DataHub.Charges.Libraries.DefaultChargeLink;
using Energinet.DataHub.Charges.Libraries.Models;
using Energinet.DataHub.Charges.Libraries.Providers;
using FluentAssertions;
using GreenEnergyHub.TestHelpers;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.Charges.Clients.CreateDefaultChargeLink.Tests.Configuration
{
    [UnitTest]
    public class DefaultChargeLinkClientConfigurationTests
    {
        [Theory]
        [InlineAutoDomainData]
        public void WhenCalled_WithConfigureServices_RegistrationIsCorrect(
            [NotNull] string anyReplyQueueName,
            [NotNull] Mock<ServiceBusClient> serviceBusClient)
        {
            // Act
            var sut = new ServiceCollection();
            sut.ConfigureDefaultChargeLinkClientConfiguration(
                serviceBusClient.Object,
                new ServiceBusRequestSenderConfiguration(anyReplyQueueName));

            var defaultChargeLinkClientRegistration
                = sut.First(x => x.ServiceType == typeof(IDefaultChargeLinkClient));

            var serviceBusRequestSenderConfiguration
                 = sut.First(x => x.ServiceType == typeof(IServiceBusRequestSenderProvider));

            // Assert
            defaultChargeLinkClientRegistration.Lifetime.Should().Be(ServiceLifetime.Scoped);
            serviceBusRequestSenderConfiguration.Lifetime.Should().Be(ServiceLifetime.Singleton);
        }
    }
}
