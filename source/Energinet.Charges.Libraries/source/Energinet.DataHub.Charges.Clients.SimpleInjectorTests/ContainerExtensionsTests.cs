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
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Energinet.DataHub.Charges.Libraries.Clients.SimpleInjector;
using Energinet.DataHub.Charges.Libraries.DefaultChargeLink;
using Energinet.DataHub.Charges.Libraries.Models;
using Energinet.DataHub.Charges.Libraries.Providers;
using FluentAssertions;
using Moq;
using SimpleInjector;
using SimpleInjector.Lifestyles;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.Charges.Clients.SimpleInjectorTests
{
    [UnitTest]
    public class ContainerExtensionsTests
    {
        // The expected value becomes 'Async Scoped' since the container DefaultScopedLifestyle is AsyncScopedLifestyle.
        private const string ExpectedScopedName = "Async Scoped";
        private const string ExpectedSingleTonName = "Singleton";

        [Fact]
        public async Task Container_WhenCalledWithAddDefaultChargeLinkClient_RegistrationIsCorrect()
        {
            // Arrange
            var sut = new Container();
            sut.Options.DefaultScopedLifestyle = new AsyncScopedLifestyle();
            var serviceBusClient = new Mock<ServiceBusClient>();

            // Act
            sut.AddDefaultChargeLinkClient(
                serviceBusClient.Object,
                new ServiceBusRequestSenderTestConfiguration(
                    "anyReplyQueueName",
                    "AnyRequestQueueName"));

            // Assert
            var actualRegistrations = sut.Collection.Container.GetCurrentRegistrations();
            actualRegistrations.First(p =>
                p.ImplementationType == typeof(DefaultChargeLinkClient)).Lifestyle.Name.Should().Be(ExpectedScopedName);
            actualRegistrations.First(p =>
                p.ImplementationType == typeof(DefaultChargeLinkReplyReader)).Lifestyle.Name.Should().Be(ExpectedScopedName);
            actualRegistrations.First(p =>
                p.ImplementationType == typeof(IServiceBusRequestSenderProvider)).Lifestyle.Name.Should().Be(ExpectedSingleTonName);

            // Cleanup
            await sut.DisposeAsync().ConfigureAwait(false);
        }
    }
}
