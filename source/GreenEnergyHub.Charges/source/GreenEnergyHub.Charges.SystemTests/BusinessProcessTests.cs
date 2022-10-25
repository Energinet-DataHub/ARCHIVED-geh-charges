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
using Energinet.DataHub.Core.FunctionApp.TestCommon.Configuration.B2C;
using Energinet.DataHub.Core.TestCommon;
using FluentAssertions;
using GreenEnergyHub.Charges.Domain.Dtos.SharedDtos;
using GreenEnergyHub.Charges.IntegrationTest.Core.TestFiles.Charges;
using GreenEnergyHub.Charges.IntegrationTest.Core.TestHelpers;
using GreenEnergyHub.Charges.SystemTests.Fixtures;
using GreenEnergyHub.Charges.TestCore.TestHelpers;
using NodaTime;
using Xunit;

namespace GreenEnergyHub.Charges.SystemTests
{
    /// <summary>
    /// Contains business process tests at a system level through API Management
    /// </summary>
    [Collection(nameof(SystemTestCollectionFixture))]
    public class BusinessProcessTests : IClassFixture<BusinessProcessConfiguration>, IAsyncLifetime
    {
        private readonly B2CAppAuthenticationClient _gridAccessProviderAppAuthenticationClient;
        private readonly B2CAppAuthenticationClient _energySupplierAppAuthenticationClient;

        public BusinessProcessTests()
        {
            Configuration = new BusinessProcessConfiguration();

            _gridAccessProviderAppAuthenticationClient = new B2CAppAuthenticationClient(
                Configuration.AuthorizationConfiguration.TenantId,
                Configuration.AuthorizationConfiguration.BackendApp,
                Configuration.AuthorizationConfiguration.ClientApps[Configuration.GridAccessProviderClientName]);

            _energySupplierAppAuthenticationClient = new B2CAppAuthenticationClient(
                Configuration.AuthorizationConfiguration.TenantId,
                Configuration.AuthorizationConfiguration.BackendApp,
                Configuration.AuthorizationConfiguration.ClientApps[Configuration.EnergySupplierClientName]);

            GridAccessProviderClient = new HttpClient();
            EnergySupplierClient = new HttpClient();
        }

        private BusinessProcessConfiguration Configuration { get; }

        private HttpClient GridAccessProviderClient { get; }

        private HttpClient EnergySupplierClient { get; }

        public async Task InitializeAsync()
        {
            await AcquireTokensForClients();
            await EmptyMessageHubQueuesAsync();
        }

        public Task DisposeAsync()
        {
            GridAccessProviderClient.Dispose();
            EnergySupplierClient.Dispose();
            return Task.CompletedTask;
        }

        [SystemFact]
        public async Task When_SubmittingChargeInformationRequestWithNewSubscription_Then_PeekReturnsCorrespondingConfirmation()
        {
            // Arrange
            var currentInstant = SystemClock.Instance.GetCurrentInstant();
            var expectedConfirmedOperationId = $"<cim:originalTransactionIDReference_MktActivityRecord.mRID>SysTestOpId{currentInstant}</cim:originalTransactionIDReference_MktActivityRecord.mRID>";
            var request = PrepareRequest(ChargeInformationRequests.Subscription, currentInstant);

            var response = await GridAccessProviderClient.SendAsync(request);
            response.StatusCode.Should().Be(HttpStatusCode.Accepted);

            // Peek confirmation - as Grid Access Supplier
            var gridBundleId = Guid.NewGuid().ToString();
            var peekedConfirmation = await PeekAsync(GridAccessProviderClient, gridBundleId);

            // Assert confirmation
            peekedConfirmation!.Headers.GetValues("MessageType").FirstOrDefault()
                .Should().Be(nameof(DocumentType.ConfirmRequestChangeOfPriceList));
            var content = await peekedConfirmation.Content.ReadAsStringAsync();
            content.Should().Contain(CimMessageConstants.ConfirmRequestChangeOfPriceListRootElement);
            content.Should().Contain(expectedConfirmedOperationId);

            // Dequeue - throws XUnitException if dequeue not succeeding
            await DequeueAsync(GridAccessProviderClient, gridBundleId);

            // Peek notification - as Energy Supplier
            var supplierBundleId = Guid.NewGuid().ToString();
            var peekedNotification = await PeekAsync(EnergySupplierClient, supplierBundleId);

            // Assert notification
            peekedNotification!.Headers.GetValues("MessageType").FirstOrDefault()
                .Should().Be(nameof(DocumentType.NotifyPriceList));
            var notificationContent = await peekedNotification.Content.ReadAsStringAsync();
            notificationContent.Should().Contain(CimMessageConstants.NotifyPriceListRootElement);

            // Dequeue - as Energy Supplier - throws XUnitException if dequeue not succeeding
            await DequeueAsync(EnergySupplierClient, supplierBundleId);
        }

        private async Task AcquireTokensForClients()
        {
            await AddAuthenticationTokenAsync(GridAccessProviderClient, _gridAccessProviderAppAuthenticationClient);
            await AddAuthenticationTokenAsync(EnergySupplierClient, _energySupplierAppAuthenticationClient);
        }

        private async Task AddAuthenticationTokenAsync(HttpClient httpClient, B2CAppAuthenticationClient client)
        {
            var authenticationResult = await client.GetAuthenticationTokenAsync();
            httpClient.BaseAddress = Configuration.AuthorizationConfiguration.ApiManagementBaseAddress;
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authenticationResult.AccessToken);
        }

        private async Task EmptyMessageHubQueuesAsync()
        {
            await EmptyMessageHubQueueAsync(GridAccessProviderClient);
            await EmptyMessageHubQueueAsync(EnergySupplierClient);
        }

        private HttpRequestMessage PrepareRequest(string testFilePath, Instant currentInstant)
        {
            var body = EmbeddedResourceHelper
                .GetEmbeddedFile(testFilePath, currentInstant, ZonedDateTimeServiceHelper.GetZonedDateTimeService(currentInstant))
                .Replace("{{GridAccessProvider}}", Configuration.GridAccessProvider);

            return new HttpRequestMessage(HttpMethod.Post, Configuration.ChargeIngestionEndpoint)
            {
                Content = new StringContent(body, Encoding.UTF8, "application/xml"),
            };
        }

        private async Task<HttpResponseMessage?> PeekAsync(HttpClient gridAccessProviderClient, string bundleId)
        {
            HttpResponseMessage? peekResponse = null;
            await Awaiter.WaitUntilConditionAsync(
                async () =>
                {
                    peekResponse = await SendPeekAsync(gridAccessProviderClient, bundleId);
                    return peekResponse.StatusCode == HttpStatusCode.OK;
                },
                TimeSpan.FromMinutes(1),
                TimeSpan.FromSeconds(1));

            return peekResponse!;
        }

        private async Task DequeueAsync(HttpClient httpClient, string bundleId)
        {
            await Awaiter.WaitUntilConditionAsync(
                async () =>
                {
                    var dequeueResponse = await DequeueOkAsync(httpClient, bundleId);
                    return dequeueResponse.StatusCode == HttpStatusCode.OK;
                },
                TimeSpan.FromMinutes(1),
                TimeSpan.FromSeconds(1));
        }

        private async Task EmptyMessageHubQueueAsync(HttpClient httpClient)
        {
            await Awaiter.WaitUntilConditionAsync(
                async () =>
                {
                    var bundleId = Guid.NewGuid().ToString();
                    var response = await SendPeekAsync(httpClient, bundleId);
                    await HandleResponseAsync(httpClient, response, bundleId);
                    return response.StatusCode == HttpStatusCode.NoContent;
                },
                TimeSpan.FromMinutes(1),
                TimeSpan.FromSeconds(1));
        }

        private async Task<HttpResponseMessage> SendPeekAsync(HttpClient httpClient, string bundleId)
        {
            var peekEndpoint = Configuration.PeekEndpoint + bundleId;
            var peekRequest = new HttpRequestMessage(HttpMethod.Get, peekEndpoint);
            return await httpClient.SendAsync(peekRequest);
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
                    throw new InvalidOperationException($"Cannot handle response from Peek. StatusCode {response.StatusCode} not handled.");
            }
        }

        private async Task<HttpResponseMessage> DequeueBadRequestAsync(HttpClient httpClient, HttpResponseMessage response)
        {
            var content = await response.Content.ReadAsStringAsync();
            var bundleId = content.Substring(111, 37);
            var dequeueUri = Configuration.DequeueEndpoint + bundleId;
            var dequeueRequest = new HttpRequestMessage(HttpMethod.Delete, dequeueUri);
            return await httpClient.SendAsync(dequeueRequest);
        }

        private async Task<HttpResponseMessage> DequeueOkAsync(HttpClient httpClient, string bundleId)
        {
            var dequeueUri = Configuration.DequeueEndpoint + bundleId;
            var dequeueRequest = new HttpRequestMessage(HttpMethod.Delete, dequeueUri);
            return await httpClient.SendAsync(dequeueRequest);
        }
    }
}
