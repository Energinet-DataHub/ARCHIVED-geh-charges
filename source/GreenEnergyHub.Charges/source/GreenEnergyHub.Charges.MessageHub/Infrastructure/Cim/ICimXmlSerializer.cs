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

using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using GreenEnergyHub.Charges.Domain.Dtos.SharedDtos;
using GreenEnergyHub.Charges.MessageHub.Models.AvailableData;

namespace GreenEnergyHub.Charges.MessageHub.Infrastructure.Cim
{
    /// <summary>
    /// Contract defining Xml serializer to be used for AvailableData
    /// </summary>
    /// <typeparam name="TAvailableData">Any AvailableData that should support Xml notifications</typeparam>
    public interface ICimXmlSerializer<in TAvailableData>
        where TAvailableData : AvailableDataBase
    {
        /// <summary>
        /// Serialize AvailableData to stream
        /// </summary>
        Task SerializeToStreamAsync(
            IEnumerable<TAvailableData> availableData,
            Stream stream,
            BusinessReasonCode businessReasonCode,
            string senderId,
            MarketParticipantRole senderRole,
            string recipientId,
            MarketParticipantRole recipientRole);
    }
}
