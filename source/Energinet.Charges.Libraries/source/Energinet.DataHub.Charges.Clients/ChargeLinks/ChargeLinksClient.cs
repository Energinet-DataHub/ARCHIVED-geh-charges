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
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Energinet.Charges.Contracts;

namespace Energinet.DataHub.Charges.Clients.ChargeLinks
{
    public sealed class ChargeLinksClient : IChargeLinksClient
    {
        private readonly HttpClient _httpClient;

        internal ChargeLinksClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        /// <summary>
        /// Gets all charge links data for a given metering point. Currently it returns mocked data.
        /// </summary>
        /// <param name="meteringPointId">The 18-digits metering point identifier used by the Danish version of Green Energy Hub.
        /// Use 404 to get a "404 Not Found" response.
        /// Empty input will result in a "400 Bad Request" response</param>
        /// <returns>A collection of mocked charge links data (Dtos)</returns>
        public async Task<IList<ChargeLinkDto>> GetAsync(string meteringPointId)
        {
            var list = new List<ChargeLinkDto>();
            var response = await _httpClient.GetAsync(ChargesRelativeUris.GetChargeLinks(meteringPointId))
                .ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
                return list;

            var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            var result = JsonSerializer.Deserialize<List<ChargeLinkDto>>(content);

            if (result != null)
                list.AddRange(result);

            return list;
        }
    }
}
