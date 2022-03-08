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
using Energinet.DataHub.Core.Messaging.Transport;

namespace GreenEnergyHub.Charges.Infrastructure.Core.MessagingExtensions
{
    public interface IMessageDispatcher<in TOutboundMessage>
        where TOutboundMessage : IOutboundMessage
    {
        /// <summary>
        /// Dispatch a <see cref="IOutboundMessage"/>
        /// </summary>
        /// <param name="message"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        public Task DispatchAsync(TOutboundMessage message, CancellationToken cancellationToken = default);

        /// <summary>
        /// Dispatch a <see cref="IOutboundMessage"/> with sessionId
        /// </summary>
        /// <param name="message"></param>
        /// <param name="sessionId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        public Task DispatchAsync(TOutboundMessage message, string sessionId, CancellationToken cancellationToken = default);
    }
}
