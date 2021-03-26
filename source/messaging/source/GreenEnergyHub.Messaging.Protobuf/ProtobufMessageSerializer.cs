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
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf;
using GreenEnergyHub.Messaging.Transport;

namespace GreenEnergyHub.Messaging.Protobuf
{
    /// <summary>
    /// Serialize a message in proto buf format
    /// </summary>
    public sealed class ProtobufMessageSerializer : MessageSerializer
    {
        private readonly ProtobufOutboundMapperFactory _outboundMapperFactory;

        /// <summary>
        /// Create a serializer
        /// </summary>
        /// <param name="outboundMapperFactory">Mapper factory</param>
        public ProtobufMessageSerializer(ProtobufOutboundMapperFactory outboundMapperFactory)
        {
            _outboundMapperFactory = outboundMapperFactory ?? throw new ArgumentNullException(nameof(outboundMapperFactory));
        }

        /// <inheritdoc cref="MessageSerializer"/>
        public override Task<byte[]> ToBytesAsync(IOutboundMessage message, CancellationToken cancellationToken = default)
        {
            var mapper = _outboundMapperFactory.GetMapper(message);
            var data = mapper.Convert(message).ToByteArray();
            return Task.FromResult(data);
        }
    }
}
