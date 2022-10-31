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

using Azure.Messaging.ServiceBus;
using Energinet.DataHub.Core.Messaging.Protobuf;
using GreenEnergyHub.Charges.Application.Messaging;
using GreenEnergyHub.Charges.Infrastructure.Core.MessagingExtensions.Factories;
using GreenEnergyHub.Charges.Infrastructure.Core.MessagingExtensions.Registration;
using GreenEnergyHub.Charges.Tests.Infrastructure.Messaging;
using GreenEnergyHub.TestHelpers;
using Microsoft.Extensions.DependencyInjection;
using NodaTime;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Infrastructure.Core.MessagingExtensions.Registration
{
    [UnitTest]
    public class RegistrationTests
    {
        [Theory]
        [InlineAutoDomainData]
        public void AddMessageDispatcher_AllowsResolvingAMessageDispatcher(string anyTopicName)
        {
            // Arrange
            var anyValidConnectionString = "Endpoint=foo/;SharedAccessKeyName=foo;SharedAccessKey=foo";
            var services = new ServiceCollection();
            services.AddScoped<IClock>(_ => SystemClock.Instance);
            services.AddSingleton(_ => new ServiceBusClient(anyValidConnectionString));

            // Act
            services.AddScoped<IServiceBusMessageFactory, ServiceBusMessageFactory>();
            services.SendProtobuf<TestMessageContract>();
            services.AddMessagingProtobuf()
                .AddExternalMessageDispatcher<TestMessage>(anyTopicName);

            // Assert
            var provider = services.BuildServiceProvider();
            var dispatcher = provider.GetService<IMessageDispatcher<TestMessage>>();
            Assert.NotNull(dispatcher);
        }
    }
}
