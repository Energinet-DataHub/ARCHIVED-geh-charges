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

using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.AutoMoq;
using AutoFixture.Kernel;
using Energinet.DataHub.Core.Messaging.Transport;
using FluentAssertions;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeInformationCommands;
using GreenEnergyHub.Charges.Domain.Dtos.Messages;
using GreenEnergyHub.Charges.Infrastructure.Core.MessagingExtensions.Registration;
using GreenEnergyHub.Charges.Tests.TestCore;
using GreenEnergyHub.Json;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Infrastructure.Core.MessagingExtensions.Serialization
{
    [UnitTest]
    public class JsonMessageDeserializerTests
    {
        /// <summary>
        /// This is an effort to try to make sure that all <see cref="IInboundMessage"/> implementations can be
        /// deserialized by the messaging framework.
        ///
        /// The test suffers from a few weaknesses regarding the following assumptions about the messaging framework:
        /// * It is assumed that transport uses JSON serialized UTF8 strings converted to byte[]
        /// * It is assumed that it uses <see cref="GreenEnergyHub.Json.JsonSerializer.DeserializeAsync"/> for deserialization
        /// </summary>
        [Theory]
        [MemberData(nameof(Messages))]
        public async Task FromBytesAsync_CreatesMessage(IInboundMessage expected)
        {
            // Arrange
            if (expected is ChargeInformationCommandBundle)
            {
                Debugger.Break();
            }

            var jsonSerializer = GetMessagingDeserializer();
            await using var stream = GetStream(expected, jsonSerializer);

            // Act
            var actual = await jsonSerializer.DeserializeAsync(stream, expected.GetType()).ConfigureAwait(false);

            // Assert
            actual.Should().BeEquivalentTo(expected);
        }

        /// <summary>
        /// Get stream for the <paramref name="expected"/>.
        /// Assumes that this conversion corresponds with the expectations of the <see cref="MessageExtractor"/>
        /// implementation.
        /// </summary>
        private static MemoryStream GetStream(IInboundMessage expected, IJsonSerializer deserializer)
        {
            var jsonString = deserializer.Serialize(expected);
            var bytes = Encoding.UTF8.GetBytes(jsonString);
            return new MemoryStream(bytes);
        }

        /// <summary>
        /// Try to make sure that we use the same implementation of deserialization that is used in the messaging framework.
        /// </summary>
        private static IJsonSerializer GetMessagingDeserializer()
        {
            var services = new ServiceCollection();
            services.AddMessaging();
            return services
                .BuildServiceProvider()
                .GetRequiredService<IJsonSerializer>();
        }

        /// <summary>
        /// Return populated instances of all non-abstract implementations of <see cref="IInboundMessage"/>s
        /// from domain assembly.
        /// </summary>
        public static TheoryData<IInboundMessage> Messages
        {
            get
            {
                var data = new TheoryData<IInboundMessage>();
                var fixture = new Fixture().Customize(new AutoMoqCustomization());
                var domainAssembly = DomainAssemblyHelper.GetDomainAssembly();
                var messageTypes = domainAssembly
                    .GetTypes()
                    .Where(t => typeof(IMessage).IsAssignableFrom(t) && t.IsClass && !t.IsAbstract)
                    .ToList();
                var messages = messageTypes
                    .Select(t => (IMessage)fixture.Create(t, new SpecimenContext(fixture)))
                    .ToList();
                messages
                    .ForEach(m => data.Add(m));
                return data;
            }
        }
    }
}
