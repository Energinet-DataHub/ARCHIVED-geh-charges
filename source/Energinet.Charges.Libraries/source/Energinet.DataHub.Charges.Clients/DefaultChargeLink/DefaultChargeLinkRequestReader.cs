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

using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Energinet.Charges.Contracts;
using Energinet.DataHub.Charges.Libraries.Protobuf;

namespace Energinet.DataHub.Charges.Libraries.DefaultChargeLink
{
    public sealed class DefaultChargeLinkRequestReader : DefaultChargeLinkRequestReaderBase
    {
        private readonly OnDataAvailable _handleData;

        /// <summary>
        /// Provides functionality to read and map data published as
        /// a <see cref="CreateDefaultChargeLinks" /> request.
        /// </summary>
        /// <param name="handleData">Delegate to handle <see cref="CreateDefaultChargeLinks" /> request data</param>
        public DefaultChargeLinkRequestReader([NotNull] OnDataAvailable handleData)
        {
            _handleData = handleData;
        }

        /// <summary>
        /// Read and map data to be handled by the provided delegate.
        /// </summary>
        /// <param name="data">Data request to deserialize</param>
        public override async Task ReadAsync([NotNull] byte[] data)
        {
            var requestParser = CreateDefaultChargeLinks.Parser;
            var createDefaultChargeLinks = requestParser.ParseFrom(data);

            var chargeLinks = CreateDefaultChargeLinksInboundMapper
                .Convert(createDefaultChargeLinks);

            await _handleData(chargeLinks).ConfigureAwait(false);
        }
    }
}
