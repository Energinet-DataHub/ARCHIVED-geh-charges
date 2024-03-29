﻿// Copyright 2020 Energinet DataHub A/S
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
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Energinet.DataHub.Charges.Contracts.Charge;
using Energinet.DataHub.Charges.Contracts.ChargeLink;
using Energinet.DataHub.Charges.Contracts.ChargeMessage;
using Energinet.DataHub.Charges.Contracts.ChargePrice;

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
            var response = await _httpClient
                .GetAsync(ChargesRelativeUris.GetChargeLinks(meteringPointId))
                .ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
                return new List<ChargeLinkV1Dto>();

            var options = new JsonSerializerOptions(JsonSerializerDefaults.Web)
            {
                Converters = { new JsonStringEnumConverter() },
            };

            var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            return JsonSerializer.Deserialize<List<ChargeLinkV1Dto>>(content, options);
        }

        /// <summary>
        /// Returns charges based on the search criteria.
        /// </summary>
        /// <returns>A collection of charges(Dtos)</returns>
        public async Task<IList<ChargeV1Dto>> SearchChargesAsync(ChargeSearchCriteriaV1Dto searchCriteria)
        {
            var response = await _httpClient
                .PostAsJsonAsync(ChargesRelativeUris.SearchCharges(), searchCriteria)
                .ConfigureAwait(false);

            return await HandleResultAsync<IList<ChargeV1Dto>>(response).ConfigureAwait(false);
        }

        /// <summary>
        /// Returns charge messages based on the search criteria.
        /// </summary>
        /// <returns>Charge messages(Dto) with collection of messages</returns>
        public async Task<ChargeMessagesV1Dto> SearchChargeMessagesAsync(ChargeMessagesSearchCriteriaV1Dto searchCriteria)
        {
            var response = await _httpClient
                .PostAsJsonAsync(ChargesRelativeUris.SearchChargeMessages(), searchCriteria)
                .ConfigureAwait(false);

            return await HandleResultAsync<ChargeMessagesV1Dto>(response).ConfigureAwait(false);
        }

        /// <summary>
        /// Gets all market participants
        /// </summary>
        /// <returns>A collection of market participant(Dtos)</returns>
        public async Task<IList<MarketParticipantV1Dto>> GetMarketParticipantsAsync()
        {
            var response = await _httpClient
                .GetAsync(ChargesRelativeUris.GetMarketParticipants())
                .ConfigureAwait(false);

            return await HandleResultAsync<IList<MarketParticipantV1Dto>>(response).ConfigureAwait(false);
        }

        /// <summary>
        ///     Returns charge prices based on the search criteria.
        /// </summary>
        /// <returns>A collection of charge prices(Dtos)</returns>
        public async Task<ChargePricesV1Dto> SearchChargePricesAsync(ChargePricesSearchCriteriaV1Dto searchCriteria)
        {
            var response = await _httpClient
                .PostAsJsonAsync(ChargesRelativeUris.SearchChargePrices(), searchCriteria)
                .ConfigureAwait(false);

            return await HandleResultAsync<ChargePricesV1Dto>(response).ConfigureAwait(false);
        }

        /// <summary>
        /// Creates a 'ChargeInformationCommand' for charge information handling.
        /// </summary>
        public async Task CreateChargeAsync(CreateChargeV1Dto createChargeV1Dto)
        {
            var response = await _httpClient
                .PostAsJsonAsync(ChargesRelativeUris.CreateCharge(), createChargeV1Dto)
                .ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                var message = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                throw new Exception($"Charges backend returned HTTP status code {(int)response.StatusCode} with message {message}");
            }
        }

        private static async Task<TModel> HandleResultAsync<TModel>(HttpResponseMessage response)
        {
            if (!response.IsSuccessStatusCode)
            {
                var message = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                throw new Exception($"Charges backend returned HTTP status code {(int)response.StatusCode} with message {message}");
            }

            var options = new JsonSerializerOptions(JsonSerializerDefaults.Web)
            {
                Converters = { new JsonStringEnumConverter() },
            };

            var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            return JsonSerializer.Deserialize<TModel>(content, options);
        }
    }
}
