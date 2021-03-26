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
using System.Threading.Tasks;
using FluentAssertions;
using GreenEnergyHub.Messaging.MessageTypes.Common;
using GreenEnergyHub.Messaging.Protobuf.Tests.Assets.Messages;
using GreenEnergyHub.Messaging.Protobuf.Tests.Implementations.Send;
using GreenEnergyHub.Messaging.Transport;
using Microsoft.Extensions.DependencyInjection;
using NodaTime;
using Test.Assets;
using Xunit;

namespace GreenEnergyHub.Messaging.Protobuf.Tests
{
    [Trait("Category", "Component")]
    public class ProtobufTests
    {
        [Fact]
        public async Task Send_and_receive_must_result_in_same_transmitted_values()
        {
            var expectedMrid = "123";

            // Send
            var sendingServiceCollection = new ServiceCollection();
            sendingServiceCollection.AddSingleton<InProcessChannel>();
            sendingServiceCollection.AddScoped<Dispatcher>();
            sendingServiceCollection.SendProtobuf<TestEnvelope>();
            var sendingServiceProvider = sendingServiceCollection.BuildServiceProvider();

            var messageDispatcher = sendingServiceProvider.GetRequiredService<Dispatcher>();
            var outboundMessage = new SayHelloMessage
            {
                Transaction = new Transaction(expectedMrid),
            };
            await messageDispatcher.DispatchAsync(outboundMessage).ConfigureAwait(false);
            var channel = sendingServiceProvider.GetRequiredService<InProcessChannel>();

            // The wire
            var bytes = channel.GetWrittenBytes();

            // Receive
            var receivingServiceCollection = new ServiceCollection();
            receivingServiceCollection.ReceiveProtobuf<TestEnvelope>(
                config => config
                    .FromOneOf(envelope => envelope.TestsCase)
                    .WithParser(() => TestEnvelope.Parser));

            var receivingServiceProvider = receivingServiceCollection.BuildServiceProvider();
            var messageExtractor = receivingServiceProvider.GetRequiredService<MessageExtractor>();

            var message = await messageExtractor.ExtractAsync(bytes).ConfigureAwait(false);

            message.Transaction.MRID.Should().Be(expectedMrid);
        }
    }
}
