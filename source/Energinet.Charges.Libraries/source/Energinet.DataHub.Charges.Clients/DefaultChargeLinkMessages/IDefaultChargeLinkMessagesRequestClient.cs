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
using System.Threading.Tasks;
using Energinet.DataHub.Charges.Libraries.Models;

namespace Energinet.DataHub.Charges.Libraries.DefaultChargeLinkMessages
{
    public interface IDefaultChargeLinkMessagesRequestClient
    {
        /// <summary>
        /// Request the Charges domain to create default charge links
        /// based on the supplied meteringPointIds entity's MeteringPointType.
        /// </summary>
        /// <param name="createDefaultChargeLinkMessagesDto"></param>
        /// <param name="correlationId"></param>
        Task CreateDefaultChargeLinkMessagesRequestAsync(
            [NotNull] CreateDefaultChargeLinkMessagesDto createDefaultChargeLinkMessagesDto,
            [NotNull] string correlationId);

        /// <summary>
        /// Used by the Charges domain to respond with failed to the request to create default charge links messages.
        /// </summary>
        /// <param name="createDefaultChargeLinkMessagesFailedDto"></param>
        /// <param name="correlationId"></param>
        /// <param name="replyQueueName"></param>
        public Task CreateDefaultChargeLinkMessagesFailedRequestAsync(
            [NotNull] CreateDefaultChargeLinkMessagesFailedDto createDefaultChargeLinkMessagesFailedDto,
            [NotNull] string correlationId,
            [NotNull] string replyQueueName);

        /// <summary>
        ///  Used by the Charges domain to respond with success to the request to create default charge links messages.
        /// </summary>
        /// <param name="createDefaultChargeLinkMessagesSucceededDto"></param>
        /// <param name="correlationId"></param>
        /// <param name="replyQueueName"></param>
        public Task CreateDefaultChargeLinkMessagesSucceededRequestAsync(
            [NotNull] CreateDefaultChargeLinkMessagesSucceededDto createDefaultChargeLinkMessagesSucceededDto,
            [NotNull] string correlationId,
            [NotNull] string replyQueueName);
    }
}
