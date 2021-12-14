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
using System.Threading;
using System.Threading.Tasks;
using Energinet.DataHub.Core.Messaging.Transport;
using Energinet.DataHub.Core.SchemaValidation;
using Energinet.DataHub.Core.SchemaValidation.Extensions;

namespace GreenEnergyHub.Charges.Infrastructure.Messaging.Serialization
{
    public abstract class SchemaValidatingMessageDeserializer<TInboundMessage> : MessageDeserializer<TInboundMessage>
        where TInboundMessage : IInboundMessage
    {
        private readonly IXmlSchema _targetSchema;

        protected SchemaValidatingMessageDeserializer(IXmlSchema targetSchema)
        {
            _targetSchema = targetSchema;
        }

        public sealed override async Task<IInboundMessage> FromBytesAsync(byte[] data, CancellationToken cancellationToken = default)
        {
            await using var inputStream = new MemoryStream(data);

            var reader = new SchemaValidatingReader(inputStream, _targetSchema);
            var message = await ConvertAsync(reader).ConfigureAwait(false);

            if (reader.HasErrors)
            {
                return new SchemaValidatedInboundMessage<TInboundMessage>(reader.CreateErrorResponse());
            }

            return new SchemaValidatedInboundMessage<TInboundMessage>(message);
        }

        protected abstract Task<TInboundMessage> ConvertAsync(SchemaValidatingReader reader);
    }
}
