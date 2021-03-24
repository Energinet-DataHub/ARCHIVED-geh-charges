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

using System.Threading;
using System.Threading.Tasks;

namespace GreenEnergyHub.Messaging.Transport
{
    /// <summary>
    /// Abstract class for deserializing incoming messages.
    /// </summary>
    public abstract class MessageDeserializer
    {
        /// <summary>
        /// Deserialize <paramref name="data"/> to an inbound message
        /// </summary>
        /// <param name="data">Data to deserialize</param>
        /// <param name="cancellationToken">Cancellation token for the operation</param>
        /// <returns><see cref="IInboundMessage"/> extracted from <paramref name="data"/></returns>
        public abstract Task<IInboundMessage> FromBytesAsync(byte[] data, CancellationToken cancellationToken = default);
    }
}
