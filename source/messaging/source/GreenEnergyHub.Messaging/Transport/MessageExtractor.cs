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
    /// Extract a message from a data sequence
    /// </summary>
    public class MessageExtractor
    {
        private readonly MessageDeserializer _deserializer;

        /// <summary>
        /// Create a <see cref="MessageExtractor"/>
        /// </summary>
        /// <param name="deserializer">deserializer to use</param>
        public MessageExtractor(MessageDeserializer deserializer)
        {
            _deserializer = deserializer ?? throw new ArgumentNullException(nameof(deserializer));
        }

        /// <summary>
        /// Extract a message from the data sequence
        /// </summary>
        /// <param name="data">data to get the message from</param>
        /// <param name="cancellationToken">Cancellation token for the operation</param>
        /// <returns>The message from the payload</returns>
        /// <exception cref="ArgumentNullException"><paramref name="data"/> is <c>null</c></exception>
        public async Task<IInboundMessage> ExtractAsync(byte[] data, CancellationToken cancellationToken = default)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            return await _deserializer.FromBytesAsync(data, cancellationToken).ConfigureAwait(false);
        }
    }
}
