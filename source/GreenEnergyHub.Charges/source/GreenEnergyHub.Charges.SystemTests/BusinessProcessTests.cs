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
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Energinet.DataHub.Core.TestCommon;
using FluentAssertions;
using GreenEnergyHub.Charges.IntegrationTest.Core.TestFiles.Charges;
using GreenEnergyHub.Charges.IntegrationTest.Core.TestHelpers;
using GreenEnergyHub.Charges.SystemTests.Fixtures;
using NodaTime;
using Xunit;

namespace GreenEnergyHub.Charges.SystemTests
{
    /// <summary>
    /// Contains business process tests at a system level through API Management
    /// </summary>
    [Collection(nameof(SystemTestCollectionFixture))]
    public class BusinessProcessTests : IClassFixture<BusinessProcessConfiguration>
    {
        private readonly BackendAuthenticationClient _authenticationClient;

        public BusinessProcessTests()
        {
            Configuration = new BusinessProcessConfiguration();

            _authenticationClient = new BackendAuthenticationClient(
                Configuration.AuthorizationConfiguration.BackendAppScope,
                Configuration.AuthorizationConfiguration.ClientCredentialsSettings,
                Configuration.AuthorizationConfiguration.B2cTenantId);
        }

        private BusinessProcessConfiguration Configuration { get; }

        [SystemFact]
        public async Task When_SubmittingCreateSubscriptionDocument_Then_PeekReturnsCorrespondingConfirmation()
        {
            // Setup
            await FlushPostOfficeQueueAsync();

            // Arrange
            using var httpClient = await CreateConfidentialHttpClientAsync();
            var currentInstant = SystemClock.Instance.GetCurrentInstant();
            var expectedOpId = $"<cim:originalTransactionIDReference_MktActivityRecord.mRID>SysTestOpId{currentInstant}</cim:originalTransactionIDReference_MktActivityRecord.mRID>";
            var bundleId = Guid.NewGuid().ToString();

            var body = EmbeddedResourceHelper
                .GetEmbeddedFile(ChargeDocument.CreateSubscription, currentInstant)
                .Replace("{{GridAccessProvider}}", Configuration.GridAccessProvider);

            var submitRequest = new HttpRequestMessage(HttpMethod.Post, Configuration.ChargeIngestionEndpoint)
            {
                Content = new StringContent(body, Encoding.UTF8, "application/xml"),
            };

            using var actualResponse = await httpClient.SendAsync(submitRequest);
            actualResponse.StatusCode.Should().Be(HttpStatusCode.Accepted);

            // Peek
            HttpResponseMessage? peekResponse = null;
            await Awaiter.WaitUntilConditionAsync(
                async () =>
                {
                    peekResponse = await PeekAsync(httpClient, bundleId);
                    return peekResponse.StatusCode == HttpStatusCode.OK;
                },
                TimeSpan.FromMinutes(1),
                TimeSpan.FromSeconds(1));

            // Assert
            var messageType = peekResponse!.Headers.GetValues("MessageType").FirstOrDefault();
            messageType!.Should().Be(nameof(DocumentType.ConfirmRequestChangeOfPriceList));
            var content = await peekResponse!.Content.ReadAsStringAsync();
            content.Should().Contain("ConfirmRequestChangeOfPriceList_MarketDocument");
            content.Should().Contain(expectedOpId);

            // Dequeue - throws XUnitException if dequeue not succeeding
            await Awaiter.WaitUntilConditionAsync(
                async () =>
                {
                    var dequeueResponse = await DequeueOkAsync(httpClient, bundleId);
                    return dequeueResponse.StatusCode == HttpStatusCode.OK;
                },
                TimeSpan.FromMinutes(1),
                TimeSpan.FromSeconds(1));
        }

        private async Task FlushPostOfficeQueueAsync()
        {
            using var httpClient = await CreateConfidentialHttpClientAsync();
            await Awaiter.WaitUntilConditionAsync(
                async () =>
                {
                    var bundleId = Guid.NewGuid().ToString();
                    var response = await PeekAsync(httpClient, bundleId);
                    await HandleResponseAsync(httpClient, response, bundleId);
                    return response.StatusCode == HttpStatusCode.NoContent;
                },
                TimeSpan.FromMinutes(1),
                TimeSpan.FromSeconds(1));
        }

        private async Task HandleResponseAsync(HttpClient httpClient, HttpResponseMessage response, string bundleId)
        {
            switch (response.StatusCode)
            {
                case HttpStatusCode.BadRequest:
                    _ = await DequeueBadRequestAsync(httpClient, response);
                    break;
                case HttpStatusCode.OK:
                    _ = await DequeueOkAsync(httpClient, bundleId);
                    break;
                case HttpStatusCode.NoContent:
                    return;
                default:
                    throw new InvalidOperationException("Cannot handle response from Peek. StatusCode not handled.");
            }
        }

        private async Task<HttpResponseMessage> PeekAsync(HttpClient httpClient, string bundleId)
        {
            var peekEndpoint = Configuration.PeekEndpoint + bundleId;
            var peekRequest = new HttpRequestMessage(HttpMethod.Get, peekEndpoint);
            return await httpClient.SendAsync(peekRequest);
        }

        private async Task<HttpResponseMessage> DequeueOkAsync(HttpClient httpClient, string bundleId)
        {
            var dequeueUri = Configuration.DequeueEndpoint + bundleId;
            var dequeueRequest = new HttpRequestMessage(HttpMethod.Delete, dequeueUri);
            return await httpClient.SendAsync(dequeueRequest);
        }

        private async Task<HttpResponseMessage> DequeueBadRequestAsync(HttpClient httpClient, HttpResponseMessage response)
        {
            var content = await response.Content.ReadAsStringAsync();
            var bundleId = content.Substring(111, 37);
            var dequeueUri = Configuration.DequeueEndpoint + bundleId;
            var dequeueRequest = new HttpRequestMessage(HttpMethod.Delete, dequeueUri);
            return await httpClient.SendAsync(dequeueRequest);
        }

        private async Task<HttpClient> CreateConfidentialHttpClientAsync()
        {
            var authenticationResult = await _authenticationClient.GetAuthenticationTokenAsync();
            return new HttpClient
            {
                BaseAddress = Configuration.ApiManagementBaseAddress,
                DefaultRequestHeaders =
                {
                    Authorization = new AuthenticationHeaderValue("Bearer", authenticationResult.AccessToken),
                },
            };
        }
    }
}
