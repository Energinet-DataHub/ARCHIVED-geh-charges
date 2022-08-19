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
using System.Net.Http;
using System.Text;
using GreenEnergyHub.Charges.Core.DateTime;
using GreenEnergyHub.Iso8601;
using NodaTime;
using NodaTime.Testing;

namespace GreenEnergyHub.Charges.IntegrationTest.Core.TestHelpers
{
    public static class HttpRequestGenerator
    {
        public static (HttpRequestMessage Request, string CorrelationId) CreateHttpPostRequestWithAuthorization(
            string endpointUrl,
            string testFilePath,
            string accessToken,
            string localTimeZoneName)
        {
            ArgumentNullException.ThrowIfNull(endpointUrl);
            ArgumentNullException.ThrowIfNull(testFilePath);
            ArgumentNullException.ThrowIfNull(accessToken);
            ArgumentNullException.ThrowIfNull(localTimeZoneName);

            var clock = new FakeClock(SystemClock.Instance.GetCurrentInstant());
            var zonedDateTimeService = new ZonedDateTimeService(clock, new Iso8601ConversionConfiguration(localTimeZoneName));
            var (request, correlationId) = CreateHttpPostRequest(endpointUrl, testFilePath, zonedDateTimeService);

            request.Headers.Add("Authorization", $"Bearer {accessToken}");

            return (request, correlationId);
        }

        public static (HttpRequestMessage Request, string CorrelationId) CreateHttpPostRequest(
            string endpointUrl, string testFilePath, IZonedDateTimeService zonedDateTimeService)
        {
            var request = CreateHttpRequest(HttpMethod.Post, endpointUrl);

            var currentInstant = SystemClock.Instance.GetCurrentInstant();
            var chargeXml = EmbeddedResourceHelper.GetEmbeddedFile(testFilePath, currentInstant, zonedDateTimeService);
            request.Content = new StringContent(chargeXml, Encoding.UTF8, "application/xml");

            return request;
        }

        public static HttpRequestMessage CreateHttpGetRequest(string endpointUrl)
        {
            return CreateHttpRequest(HttpMethod.Get, endpointUrl);
        }

        public static HttpRequestMessage CreateHttpPutRequest(string endpointUrl)
        {
            return CreateHttpRequest(HttpMethod.Put, endpointUrl);
        }

        private static HttpRequestMessage CreateHttpRequest(HttpMethod httpMethod, string endpointUrl)
        {
            var request = new HttpRequestMessage(httpMethod, endpointUrl);
            return request;
        }
    }
}
