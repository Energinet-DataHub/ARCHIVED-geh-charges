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
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using GreenEnergyHub.Charges.Core.DateTime;
using GreenEnergyHub.Iso8601;
using Microsoft.Identity.Client;
using NodaTime.Testing;
using SystemClock = NodaTime.SystemClock;

namespace GreenEnergyHub.Charges.IntegrationTest.Core.TestHelpers
{
    public class AuthorizedHttpRequestGenerator
    {
        private readonly string _localTimeZoneName;

        public TestClient TestClient { get; }

        private AuthenticationResult? AuthenticationResult { get; set; }

        public AuthorizedHttpRequestGenerator(TestClient testClient, string localTimeZoneName)
        {
            TestClient = testClient;
            _localTimeZoneName = localTimeZoneName;
        }

        public async Task AddAuthenticationAsync(IEnumerable<string> backendAppScope, string b2CTenantId)
        {
            var backendAuthenticationClient = new BackendAuthenticationClient(
                backendAppScope,
                TestClient.ClientCredentialsSettings,
                b2CTenantId);
            AuthenticationResult = await backendAuthenticationClient.GetAuthenticationTokenAsync();
        }

        public (HttpRequestMessage Request, string CorrelationId) CreateAuthorizedHttpPostRequest(
            string endpointUrl, string testFilePath)
        {
            ArgumentNullException.ThrowIfNull(AuthenticationResult);

            var clock = new FakeClock(SystemClock.Instance.GetCurrentInstant());
            var zonedDateTimeService = new ZonedDateTimeService(clock, new Iso8601ConversionConfiguration(_localTimeZoneName));
            var (request, correlationId) = HttpRequestGenerator.CreateHttpPostRequest(endpointUrl, testFilePath, zonedDateTimeService);

            request.Headers.Add("Authorization", $"Bearer {AuthenticationResult.AccessToken}");

            return (request, correlationId);
        }
    }
}
