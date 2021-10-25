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

namespace Energinet.DataHub.Charges.Libraries.DefaultChargeLink
{
    public interface IDefaultChargeLinkClient
    {
        /// <summary>
        /// Request the Charges domain to create default charge links
        /// based on the supplied meteringPointIds entity's MeteringPointType.
        /// </summary>
        /// <param name="createDefaultChargeLinksDto">
        /// Contains data needed by the Charges Domain to create default charges links.</param>
        /// <param name="correlationId">CorrelationId specifies message context.</param>
        Task CreateDefaultChargeLinksRequestAsync(
            [NotNull] CreateDefaultChargeLinksDto createDefaultChargeLinksDto,
            [NotNull] string correlationId);

        /// <summary>
        /// The reply the Charges domain uses when creating default charge links were successful.
        /// </summary>
        /// <param name="createDefaultChargeLinksSucceededDto"></param>
        /// <param name="correlationId">CorrelationId specifies message context.</param>
        /// <param name="replyQueueName">The queue used to send the reply to.</param>
        Task CreateDefaultChargeLinksSucceededReplyAsync(
            [NotNull] CreateDefaultChargeLinksSucceededDto createDefaultChargeLinksSucceededDto,
            [NotNull] string correlationId,
            [NotNull] string replyQueueName);

        /// <summary>
        /// The reply the Charges domain uses when creating default charge links failed.
        /// </summary>
        /// <param name="createDefaultChargeLinksFailedDto"></param>
        /// <param name="correlationId">CorrelationId specifies message context.</param>
        /// <param name="replyQueueName">The queue used to send the reply to.</param>
        Task CreateDefaultChargeLinksFailedReplyAsync(
            [NotNull] CreateDefaultChargeLinksFailedDto createDefaultChargeLinksFailedDto,
            [NotNull] string correlationId,
            [NotNull] string replyQueueName);
    }
}
