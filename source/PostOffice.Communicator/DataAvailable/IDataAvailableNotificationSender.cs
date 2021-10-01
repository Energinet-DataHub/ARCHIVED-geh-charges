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

namespace GreenEnergyHub.PostOffice.Communicator.DataAvailable
{
    /// <summary>
    /// Communicates DataAvailableNotifications to the post office.
    /// </summary>
    public interface IDataAvailableNotificationSender
    {
        /// <summary>
        /// Encodes the specified DataAvailableNotification and places it onto the post office DataAvailable queue.
        /// </summary>
        /// <param name="dataAvailableNotificationDto">The notification to send to the post office.</param>
        Task SendAsync(DataAvailableNotificationDto dataAvailableNotificationDto);
    }
}
