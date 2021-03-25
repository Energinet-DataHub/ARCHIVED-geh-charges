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
    /// Abstract class representing an out-of-process transport channel.
    /// </summary>
    public abstract class Channel
    {
        /// <summary>
        /// Handle the flow of how the data is written to the channel
        /// This can be extended to handle lifetime events for a channel, Open, Close etc.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="cancellationToken"></param>
        internal Task WriteToAsync(byte[] data, CancellationToken cancellationToken = default)
        {
            return WriteAsync(data, cancellationToken);
        }

        /// <summary>
        /// Write the <paramref name="data"/> to the channel
        /// </summary>
        /// <param name="data">data to write</param>
        /// <param name="cancellationToken">cancellation token</param>
        protected abstract Task WriteAsync(byte[] data, CancellationToken cancellationToken = default);
    }
}
