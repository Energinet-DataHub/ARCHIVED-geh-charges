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
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Energinet.Charges.Contracts.Charge;
using Energinet.Charges.Contracts.ChargeLink;

namespace Energinet.DataHub.Charges.Clients.Charges
{
    public sealed class ChargesClient : IChargesClient
    {
        private readonly HttpClient _httpClient;

        internal ChargesClient(HttpClient httpClient)
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
        public async Task<IList<ChargeLinkV1Dto>> GetChargeLinksAsync(string meteringPointId)
        {
            var list = new List<ChargeLinkV1Dto>();
            var response = await _httpClient
                .GetAsync(ChargesRelativeUris.GetChargeLinks(meteringPointId))
                .ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
                return list;

            var options = new JsonSerializerOptions(JsonSerializerDefaults.Web)
            {
                Converters = { new JsonStringEnumConverter() },
            };

            var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            var result = JsonSerializer.Deserialize<List<ChargeLinkV1Dto>>(content, options);

            if (result != null)
                list.AddRange(result);

            return list;
        }

        /// <summary>
        /// Gets all charges.
        /// </summary>
        /// <returns>A collection of charges(Dtos)</returns>
        public async Task<IList<ChargeV1Dto>> GetChargesAsync()
        {
            var list = new List<ChargeV1Dto>();
            var response = await _httpClient
                .GetAsync(ChargesRelativeUris.GetCharges())
                .ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
                return list;

            var options = new JsonSerializerOptions(JsonSerializerDefaults.Web)
            {
                Converters = { new JsonStringEnumConverter() },
            };

            var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            var result = JsonSerializer.Deserialize<List<ChargeV1Dto>>(content, options);

            if (result != null)
                list.AddRange(result);

            return list;
        }

        /// <summary>
        /// Returns charges based on the search criteria.
        /// </summary>
        /// <returns>A collection of charges(Dtos)</returns>
        public async Task<IList<ChargeV1Dto>> SearchChargesAsync(SearchCriteriaDto searchCriteria)
        {
            var list = new List<ChargeV1Dto>();
            var response = await _httpClient
                .PostAsJsonAsync(ChargesRelativeUris.SearchCharges(), searchCriteria)
                .ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
                return list;

            var options = new JsonSerializerOptions(JsonSerializerDefaults.Web)
            {
                Converters = { new JsonStringEnumConverter() },
            };

            var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            var result = JsonSerializer.Deserialize<List<ChargeV1Dto>>(content, options);

            if (result != null)
                list.AddRange(result);

            return list;
        }
    }
}
