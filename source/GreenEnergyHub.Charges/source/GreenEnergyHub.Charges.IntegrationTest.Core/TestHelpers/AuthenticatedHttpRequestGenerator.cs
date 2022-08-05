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

using System.Net.Http;
using System.Threading.Tasks;
using GreenEnergyHub.Charges.Core.DateTime;
using GreenEnergyHub.Charges.IntegrationTest.Core.Fixtures.FunctionApp;
using GreenEnergyHub.Iso8601;
using NodaTime;
using NodaTime.Testing;

namespace GreenEnergyHub.Charges.IntegrationTest.Core.TestHelpers
{
    public class AuthenticatedHttpRequestGenerator
    {
        private readonly ChargesFunctionAppFixture _fixture;
        private readonly string _localTimeZoneName;

        public AuthenticatedHttpRequestGenerator(ChargesFunctionAppFixture fixture, string localTimeZoneName)
        {
            _fixture = fixture;
            _localTimeZoneName = localTimeZoneName;
        }

        public async Task<(HttpRequestMessage Request, string CorrelationId)> CreateAuthenticatedHttpPostRequestAsync(
            string endpointUrl, string testFilePath, string clientName = AuthorizationConfigurationData.GridAccessProvider8100000000030)
        {
            _fixture.SetAuthorizationConfiguration(clientName);
            var clock = new FakeClock(SystemClock.Instance.GetCurrentInstant());
            var zonedDateTimeService = new ZonedDateTimeService(clock, new Iso8601ConversionConfiguration(_localTimeZoneName));
            var (request, correlationId) = HttpRequestGenerator.CreateHttpPostRequest(endpointUrl, testFilePath, zonedDateTimeService);

            await AddAuthenticationAsync(request);

            return (request, correlationId);
        }

        private async Task AddAuthenticationAsync(HttpRequestMessage request)
        {
            var backendAuthenticationClient = new BackendAuthenticationClient(
                _fixture.AuthorizationConfiguration.BackendAppScope,
                _fixture.AuthorizationConfiguration.ClientCredentialsSettings,
                _fixture.AuthorizationConfiguration.B2cTenantId);
            var authenticationResult = await backendAuthenticationClient.GetAuthenticationTokenAsync();
            request.Headers.Add("Authorization", $"Bearer {authenticationResult.AccessToken}");
        }
    }
}
