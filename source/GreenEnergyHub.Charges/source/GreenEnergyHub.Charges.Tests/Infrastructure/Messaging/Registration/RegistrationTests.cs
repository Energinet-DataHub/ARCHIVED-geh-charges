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

using GreenEnergyHub.Charges.Application;
using GreenEnergyHub.Charges.Infrastructure.Messaging;
using GreenEnergyHub.Charges.Infrastructure.Messaging.Registration;
using GreenEnergyHub.TestHelpers;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Infrastructure.Messaging.Registration
{
    [UnitTest]
    public class RegistrationTests
    {
        [Fact]
        public void AddMessageExtractor_AllowsResolvingAMessageExtractor()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            services
                .AddMessaging()
                .AddMessageExtractor<TestMessage>();

            // Assert
            var provider = services.BuildServiceProvider();
            var dispatcher = provider.GetService<MessageExtractor<TestMessage>>();
            Assert.NotNull(dispatcher);
        }
    }
}
