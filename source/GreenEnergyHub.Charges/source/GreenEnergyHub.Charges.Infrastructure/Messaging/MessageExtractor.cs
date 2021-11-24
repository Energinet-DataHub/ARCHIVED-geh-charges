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
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Energinet.DataHub.Core.Messaging.Transport;
using GreenEnergyHub.Charges.Infrastructure.Messaging.Serialization;

namespace GreenEnergyHub.Charges.Infrastructure.Messaging
{
    public class MessageExtractor<TInboundMessage> : MessageExtractor
    {
        public MessageExtractor([NotNull] MessageDeserializer<TInboundMessage> deserializer)
            : base(deserializer)
        {
        }

        public new async Task<IInboundMessage> ExtractAsync(byte[] data, CancellationToken cancellationToken = default)
        {
            return await base.ExtractAsync(data, cancellationToken).ConfigureAwait(false);
        }

        public async Task<IInboundMessage> ExtractAsync(Stream data, CancellationToken cancellationToken = default)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));

            var bytes = await GetBytesFromStreamAsync(data, cancellationToken).ConfigureAwait(false);

            return await ExtractAsync(bytes, cancellationToken).ConfigureAwait(false);
        }

        private static async Task<byte[]> GetBytesFromStreamAsync(Stream data, CancellationToken cancellationToken)
        {
            await using var stream = new MemoryStream();
            await data.CopyToAsync(stream, cancellationToken).ConfigureAwait(false);
            var bytes = stream.ToArray();
            return bytes;
        }
    }
}
