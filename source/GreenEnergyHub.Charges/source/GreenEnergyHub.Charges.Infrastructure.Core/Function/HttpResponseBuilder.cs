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

using System.Net;
using System.Threading.Tasks;
using Energinet.DataHub.Core.SchemaValidation.Errors;
using Energinet.DataHub.Core.SchemaValidation.Extensions;
using Microsoft.Azure.Functions.Worker.Http;

namespace GreenEnergyHub.Charges.Infrastructure.Core.Function
{
    public sealed class HttpResponseBuilder : IHttpResponseBuilder
    {
        public HttpResponseData CreateAcceptedResponse(HttpRequestData request)
        {
            return request.CreateResponse(HttpStatusCode.Accepted);
        }

        public async Task<HttpResponseData> CreateBadRequestResponseAsync(HttpRequestData request, ErrorResponse response)
        {
            var httpResponse = request.CreateResponse(HttpStatusCode.BadRequest);
            await response.WriteAsXmlAsync(httpResponse.Body).ConfigureAwait(false);
            return httpResponse;
        }
    }
}
