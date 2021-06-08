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

using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using GreenEnergyHub.Charges.Application;
using GreenEnergyHub.Charges.Domain.Events.Local;
using GreenEnergyHub.Charges.Infrastructure.Messaging;
using GreenEnergyHub.Charges.Infrastructure.Messaging.Registration;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.DependencyInjection;

namespace GreenEnergyHub.Charges.IntegrationTests.TestHelpers
{
    public class ChargeCommandReceiverConfiguration : ChargeCommandReceiver.Startup
    {
        private readonly ITopicClient? _topicClient;

        public ChargeCommandReceiverConfiguration()
        {
        }

        public ChargeCommandReceiverConfiguration(ITopicClient topicClient)
        {
            _topicClient = topicClient;
        }

        protected override void ConfigureMessaging([NotNull] IFunctionsHostBuilder builder)
        {
            builder.Services
                .AddMessaging()
                .AddMessageExtractor<ChargeCommandReceivedEvent>();

            builder.Services
                .AddSingleton(_ => _topicClient)
                .AddScoped<IMessageDispatcher<ChargeCommandAcceptedEvent>, MessageDispatcher<ChargeCommandAcceptedEvent>>()
                .AddScoped<IMessageDispatcher<ChargeCommandRejectedEvent>, MessageDispatcher<ChargeCommandRejectedEvent>>()
                .AddScoped<Channel<ChargeCommandAcceptedEvent>, TopicChannelCommandAccepted>()
                .AddScoped<Channel<ChargeCommandRejectedEvent>, TopicChannelCommandRejected>();
        }

        private class TopicChannelCommandAccepted : Channel<ChargeCommandAcceptedEvent>
        {
            private readonly ITopicClient _topicClient;

            public TopicChannelCommandAccepted(ITopicClient topicClient)
            {
                _topicClient = topicClient;
            }

            protected override Task WriteAsync(byte[] data, CancellationToken cancellationToken = default)
            {
                return _topicClient.SendAsync(new Message(data));
            }
        }

        private class TopicChannelCommandRejected : Channel<ChargeCommandRejectedEvent>
        {
            private readonly ITopicClient _topicClient;

            public TopicChannelCommandRejected(ITopicClient topicClient)
            {
                _topicClient = topicClient;
            }

            protected override Task WriteAsync(byte[] data, CancellationToken cancellationToken = default)
            {
                return _topicClient.SendAsync(new Message(data));
            }
        }
    }
}
