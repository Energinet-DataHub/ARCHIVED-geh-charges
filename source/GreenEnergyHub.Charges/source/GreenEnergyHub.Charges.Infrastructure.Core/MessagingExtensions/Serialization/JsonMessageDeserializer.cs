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

using System.IO;
using System.Threading.Tasks;
using Energinet.DataHub.Core.JsonSerialization;
using GreenEnergyHub.Charges.Domain.Dtos.Messages.Events;

namespace GreenEnergyHub.Charges.Infrastructure.Core.MessagingExtensions.Serialization
{
    public class JsonMessageDeserializer<TInternalEvent>
      where TInternalEvent : InternalEvent
    {
        private readonly IJsonSerializer _jsonSerializer;

        public JsonMessageDeserializer(IJsonSerializer jsonSerializer)
        {
            _jsonSerializer = jsonSerializer;
        }

        public async Task<InternalEvent> FromBytesAsync(byte[] data)
        {
            var stream = new MemoryStream(data);
            await using (stream.ConfigureAwait(false))
            {
                return (TInternalEvent)await _jsonSerializer.DeserializeAsync(
                    stream, typeof(TInternalEvent)).ConfigureAwait(false);
            }
        }
    }
}
