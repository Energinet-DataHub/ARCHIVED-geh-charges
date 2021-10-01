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

using System;
using System.Linq;
using Google.Protobuf;
using GreenEnergyHub.PostOffice.Communicator.Contracts;
using GreenEnergyHub.PostOffice.Communicator.Exceptions;
using GreenEnergyHub.PostOffice.Communicator.Model;

namespace GreenEnergyHub.PostOffice.Communicator.Peek
{
    public sealed class RequestBundleParser : IRequestBundleParser
    {
        public byte[] Parse(DataBundleRequestDto request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var message = new RequestBundleRequest
            {
                IdempotencyId = request.IdempotencyId,
                UUID = { request.DataAvailableNotificationIds.Select(x => x.ToString()), }
            };

            return message.ToByteArray();
        }

        public DataBundleRequestDto Parse(byte[] dataBundleRequestContract)
        {
            try
            {
                var bundleResponse = RequestBundleRequest.Parser.ParseFrom(dataBundleRequestContract);
                return new DataBundleRequestDto(bundleResponse.IdempotencyId, bundleResponse.UUID.Select(Guid.Parse).ToList());
            }
            catch (InvalidProtocolBufferException e)
            {
                throw new PostOfficeCommunicatorException("Error parsing bytes for DataBundleRequestDto", e);
            }
        }
    }
}
