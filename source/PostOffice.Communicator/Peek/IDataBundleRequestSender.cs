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

using System.Threading.Tasks;
using GreenEnergyHub.PostOffice.Communicator.Model;

namespace GreenEnergyHub.PostOffice.Communicator.Peek
{
    /// <summary>
    /// Communicates to the sub-domain a request for bundle contents.
    /// </summary>
    public interface IDataBundleRequestSender
    {
        /// <summary>
        /// Sends a request for bundle contents to the specified sub-domain.
        /// </summary>
        /// <param name="dataBundleRequestDto">The request to send.</param>
        /// <param name="domainOrigin">The sub-domain to send the request to.</param>
        /// <returns><see cref="RequestDataBundleResponseDto"/> containing the location of bundle contents; or null, if the request timed out.</returns>
        Task<RequestDataBundleResponseDto?> SendAsync(DataBundleRequestDto dataBundleRequestDto, DomainOrigin domainOrigin);
    }
}
