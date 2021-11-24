using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using Energinet.DataHub.Charges.Clients.Abstractions;

namespace Energinet.DataHub.Charges.Clients.Bff
{
    public sealed class ChargeLinksClient : IChargeLinksClient
    {
        private readonly HttpClient _httpClient;

        internal ChargeLinksClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<ChargeLinkDto?> GetChargeLinksByMeteringPointIdAsync(string meteringPointId)
        {
            var response = await _httpClient.GetAsync(new Uri($"ChargeLinks/GetChargeLinksByMeteringPointIdAsync/?meteringPointId={meteringPointId}", UriKind.Relative))
                .ConfigureAwait(false);

            return await response.Content.ReadFromJsonAsync<ChargeLinkDto>().ConfigureAwait(false);
        }
    }
}
