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
using Energinet.Charges.Contracts;
using Energinet.DataHub.Charges.Libraries.Models;

namespace Energinet.DataHub.Charges.Libraries.DefaultChargeLink
{
    /// <summary>
    /// Delegate that will be invoked by IDefaultChargeLinkReplyReader ReadAsync() when the
    /// serializedReplyMessageBody is a reply containing a CreateDefaultChargeLinksSucceeded.
    ///
    /// Consuming domain should implement this delegate to handle further processing following
    /// successful Default Charge Link creation.
    /// </summary>
    public delegate Task OnSuccess(CreateDefaultChargeLinksSucceededDto createDefaultChargeLinksSucceeded);

    /// <summary>
    /// Delegate that will be invoked by IDefaultChargeLinkReplyReader ReadAsync() when the
    /// serializedReplyMessageBody is a reply containing a CreateDefaultChargeLinksFailed.
    ///
    /// Consuming domain should implement this delegate to handle further processing following
    /// failed Default Charge Link creation.
    /// </summary>
    public delegate Task OnFailure(CreateDefaultChargeLinksFailedDto createDefaultChargeLinksSucceeded);

    /// <summary>
    /// Provides functionality to read and map data received from a reply to a
    /// <see cref="CreateDefaultChargeLinks" /> request. Caller must provide delegates
    /// intended to handle handle replies for successful and failed requests.
    /// </summary>
    public interface IDefaultChargeLinkReplyReader
    {
        /// <summary>
        /// Read and map data to be handled by provided delegates.
        ///
        /// ReadAsync method will invoke either OnSuccess or OnFailure delegate depending on the content
        /// of the serializedReplyMessageBody.
        /// <param name="serializedReplyMessageBody">Reply message to deserialize</param>
        /// </summary>
        Task ReadAsync([NotNull] byte[] serializedReplyMessageBody);
    }
}
