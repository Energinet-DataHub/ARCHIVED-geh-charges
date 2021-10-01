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

using GreenEnergyHub.PostOffice.Communicator.Exceptions;
using GreenEnergyHub.PostOffice.Communicator.Model;

namespace GreenEnergyHub.PostOffice.Communicator.DataAvailable
{
    /// <summary>
    /// Parses the DataAvailableNotification protobuf contract.
    /// </summary>
    public interface IDataAvailableNotificationParser
    {
        /// <summary>
        /// Parses the DataAvailableNotification protobuf contract.
        /// </summary>
        /// <param name="dataAvailableContract">A byte array containing the DataAvailable protobuf contract.</param>
        /// <returns>The parsed DataAvailableNotificationDto.</returns>
        /// <exception cref="PostOfficeCommunicatorException">
        /// Throws an exception if byte array cannot be parsed.
        /// </exception>
        DataAvailableNotificationDto Parse(byte[] dataAvailableContract);
    }
}
