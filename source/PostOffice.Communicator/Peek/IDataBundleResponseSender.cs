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
    /// Communicates to the post office a response to a given request for bundle contents.
    /// </summary>
    public interface IDataBundleResponseSender
    {
        /// <summary>
        /// Responds to a post office request for bundle contents, specifying that the contents are ready and where they are located.
        /// </summary>
        /// <param name="requestDataBundleResponseDto">The response to the request.</param>
        /// <param name="sessionId">The ServiceBus session id received from the session-enabled ServiceBusTrigger.</param>
        Task SendAsync(RequestDataBundleResponseDto requestDataBundleResponseDto, string sessionId);
    }
}
