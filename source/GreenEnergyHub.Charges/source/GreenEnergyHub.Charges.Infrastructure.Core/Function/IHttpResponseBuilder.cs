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

using System.Threading.Tasks;
using Energinet.DataHub.Core.SchemaValidation.Errors;
using Microsoft.Azure.Functions.Worker.Http;

namespace GreenEnergyHub.Charges.Infrastructure.Core.Function
{
    /// <summary>
    /// Builds http responses
    /// </summary>
    public interface IHttpResponseBuilder
    {
        /// <summary>
        /// Creates an accepted response
        /// </summary>
        /// <param name="requestData"></param>
        /// <returns>202 Accepted response data</returns>
        HttpResponseData CreateAcceptedResponse(HttpRequestData requestData);

        /// <summary>
        /// Creates a bad request response
        /// </summary>
        /// <param name="requestData"></param>
        /// <returns>400 Bad request response data</returns>
        HttpResponseData CreateBadRequestResponse(HttpRequestData requestData);

        /// <summary>
        /// Creates a bad request response asynchronously
        /// </summary>
        /// <param name="request"></param>
        /// <param name="errorResponse"></param>
        /// <returns>A <see cref="Task{TResult}"/> with 400 Bad request response data.</returns>
        Task<HttpResponseData> CreateBadRequestResponseAsync(HttpRequestData request, ErrorResponse errorResponse);

        /// <summary>
        /// Creates a bad request response asynchronously
        /// </summary>
        /// <param name="request"></param>
        /// <param name="code"></param>
        /// <returns>400 B2B bad request response data</returns>
        HttpResponseData CreateBadRequestB2BResponse(HttpRequestData request, B2BErrorCode code);
    }
}
