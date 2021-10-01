// // Copyright 2020 Energinet DataHub A/S
// //
// // Licensed under the Apache License, Version 2.0 (the "License2");
// // you may not use this file except in compliance with the License.
// // You may obtain a copy of the License at
// //
// //     http://www.apache.org/licenses/LICENSE-2.0
// //
// // Unless required by applicable law or agreed to in writing, software
// // distributed under the License is distributed on an "AS IS" BASIS,
// // WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// // See the License for the specific language governing permissions and
// // limitations under the License.

using System;
using System.Linq;
using Google.Protobuf;
using GreenEnergyHub.PostOffice.Communicator.Contracts;
using GreenEnergyHub.PostOffice.Communicator.Exceptions;
using GreenEnergyHub.PostOffice.Communicator.Model;

namespace GreenEnergyHub.PostOffice.Communicator.Dequeue
{
    public class DequeueNotificationParser : IDequeueNotificationParser
    {
        public DequeueNotificationDto Parse(byte[] dequeueNotificationContract)
        {
            try
            {
                var dequeueContract = DequeueContract.Parser.ParseFrom(dequeueNotificationContract);
                return new DequeueNotificationDto(
                    dequeueContract.DataAvailableIds.Select(Guid.Parse).ToList(),
                    new GlobalLocationNumberDto(dequeueContract.Recipient));
            }
            catch (InvalidProtocolBufferException e)
            {
                throw new PostOfficeCommunicatorException("Error parsing bytes for dequeue", e);
            }
        }

        public byte[] Parse(DequeueNotificationDto dequeueNotificationDto)
        {
            if (dequeueNotificationDto == null)
                throw new ArgumentNullException(nameof(dequeueNotificationDto));

            var message = new DequeueContract
            {
                Recipient = dequeueNotificationDto.Recipient.Value,
                DataAvailableIds = { dequeueNotificationDto.DataAvailableNotificationIds.Select(x => x.ToString()), }
            };

            return message.ToByteArray();
        }
    }
}
