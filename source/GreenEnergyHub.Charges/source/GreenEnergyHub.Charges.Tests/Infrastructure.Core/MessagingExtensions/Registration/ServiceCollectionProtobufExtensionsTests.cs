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

using Energinet.Charges.Contracts;
using Energinet.DataHub.Core.Messaging.Protobuf;
using GreenEnergyHub.Charges.Infrastructure.Core.MessagingExtensions;
using GreenEnergyHub.Charges.Infrastructure.Core.MessagingExtensions.Registration;
using GreenEnergyHub.Charges.Infrastructure.Core.MessagingExtensions.Serialization;
using GreenEnergyHub.TestHelpers;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Infrastructure.Core.MessagingExtensions.Registration
{
    [UnitTest]
    public class ServiceCollectionProtobufExtensionsTests
    {
        [Theory]
        [InlineAutoDomainData]
        public void ConfigureProtobufReception_WhenCalled_NeededTypesCanBeResolved(
            ServiceCollection serviceCollection)
        {
            // Act
            serviceCollection.ConfigureProtobufReception();

            // Assert
            var provider = serviceCollection.BuildServiceProvider();
            var actual = provider.GetRequiredService<ProtobufInboundMapperFactory>();
            Assert.NotNull(actual);
        }

        [Theory]
        [InlineAutoDomainData]
        public void ReceiveProtobufMessage_WhenCalled_NeededTypesCanBeResolved(
            ServiceCollection serviceCollection)
        {
            // Act
            serviceCollection.ConfigureProtobufReception();
            serviceCollection.ReceiveProtobufMessage<CreateDefaultChargeLinks>(
                configuration => configuration.WithParser(() => CreateDefaultChargeLinks.Parser));

            // Assert
            var provider = serviceCollection.BuildServiceProvider();
            var extractor = provider.GetRequiredService<MessageExtractor<CreateDefaultChargeLinks>>();
            var deserializer = provider.GetRequiredService<MessageDeserializer<CreateDefaultChargeLinks>>();
            var mapper = provider.GetRequiredService<ProtobufInboundMapper<CreateDefaultChargeLinks>>();
            var parser = provider.GetRequiredService<ProtobufParser<CreateDefaultChargeLinks>>();
            Assert.NotNull(extractor);
            Assert.NotNull(deserializer);
            Assert.NotNull(mapper);
            Assert.NotNull(parser);
        }
    }
}
