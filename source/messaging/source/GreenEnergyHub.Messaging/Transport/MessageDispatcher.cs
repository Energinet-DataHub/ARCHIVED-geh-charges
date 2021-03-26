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
using System.Threading;
using System.Threading.Tasks;

namespace GreenEnergyHub.Messaging.Transport
{
    /// <summary>
    /// A class that combines the serialized format, and the means of transport
    /// </summary>
    public class MessageDispatcher
    {
        private readonly MessageSerializer _serializer;
        private readonly Channel _channel;

        /// <summary>
        /// Construct a <see cref="MessageDispatcher"/>
        /// </summary>
        /// <param name="serializer">Serializer to use</param>
        /// <param name="channel">The channel where the data is sent to</param>
        public MessageDispatcher(MessageSerializer serializer, Channel channel)
        {
            _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
            _channel = channel ?? throw new ArgumentNullException(nameof(channel));
        }

        /// <summary>
        /// Send a <paramref name="message"/> to a channel
        /// </summary>
        /// <param name="message">Message to send</param>
        /// <param name="cancellationToken">Cancellation token for the operation</param>
        /// <exception cref="ArgumentNullException">message is <c>null</c></exception>
        public async Task DispatchAsync(IOutboundMessage message, CancellationToken cancellationToken = default)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));
            var data = await _serializer.ToBytesAsync(message, cancellationToken).ConfigureAwait(false);

            await _channel.WriteToAsync(data, cancellationToken).ConfigureAwait(false);
        }
    }
}
