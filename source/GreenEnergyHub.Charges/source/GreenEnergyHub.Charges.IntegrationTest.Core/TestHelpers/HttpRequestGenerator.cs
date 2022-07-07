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

using System.Net.Http;
using System.Text;
using GreenEnergyHub.Charges.TestCore;
using NodaTime;

namespace GreenEnergyHub.Charges.IntegrationTest.Core.TestHelpers
{
    public static class HttpRequestGenerator
    {
        public static (HttpRequestMessage Request, string CorrelationId) CreateHttpPostRequest(
            string endpointUrl, string testFilePath)
        {
            var (request, correlationId) = CreateHttpRequest(HttpMethod.Post, endpointUrl);

            var currentInstant = InstantHelper.GetTodayAtMidnightUtc();
            var chargeXml = EmbeddedResourceHelper.GetEmbeddedFile(testFilePath, currentInstant);
            request.Content = new StringContent(chargeXml, Encoding.UTF8, "application/xml");

            return (request, correlationId);
        }

        public static (HttpRequestMessage Request, string CorrelationId) CreateHttpGetRequest(string endpointUrl)
        {
            return CreateHttpRequest(HttpMethod.Get, endpointUrl);
        }

        public static (HttpRequestMessage Request, string CorrelationId) CreateHttpPutRequest(string endpointUrl)
        {
            return CreateHttpRequest(HttpMethod.Put, endpointUrl);
        }

        private static (HttpRequestMessage Request, string CorrelationId) CreateHttpRequest(
            HttpMethod httpMethod,
            string endpointUrl)
        {
            var request = new HttpRequestMessage(httpMethod, endpointUrl);

            var correlationId = CorrelationIdGenerator.Create();
            request.ConfigureTraceContext(correlationId);

            return (request, correlationId);
        }
    }
}
