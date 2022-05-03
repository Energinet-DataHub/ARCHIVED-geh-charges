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

using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Energinet.DataHub.Core.App.FunctionApp.Middleware.CorrelationId;
using Energinet.DataHub.Core.SchemaValidation.Errors;
using Energinet.DataHub.Core.SchemaValidation.Extensions;
using Microsoft.Azure.Functions.Worker.Http;

namespace GreenEnergyHub.Charges.Infrastructure.Core.Function
{
    public sealed class HttpResponseBuilder : IHttpResponseBuilder
    {
        private readonly ICorrelationContext _correlationContext;

        public HttpResponseBuilder(ICorrelationContext correlationContext)
        {
            _correlationContext = correlationContext;
        }

        public HttpResponseData CreateAcceptedResponse(HttpRequestData request)
        {
            var httpResponseData = request.CreateResponse(HttpStatusCode.Accepted);
            AddHeaders(httpResponseData);
            return httpResponseData;
        }

        public async Task<HttpResponseData> CreateBadRequestResponseAsync(
            HttpRequestData request, ErrorResponse errorResponse)
        {
            var httpResponse = request.CreateResponse(HttpStatusCode.BadRequest);
            AddHeaders(httpResponse);
            await errorResponse.WriteAsXmlAsync(httpResponse.Body).ConfigureAwait(false);
            return httpResponse;
        }

        public HttpResponseData CreateBadRequestResponseWithText(HttpRequestData request, string errorText)
        {
            var httpResponse = request.CreateResponse(HttpStatusCode.BadRequest);
            var unauthorizedRequest = CreateErrorMessageAsXml(httpResponse, "BadRequest", errorText);
            httpResponse.WriteString(unauthorizedRequest, Encoding.UTF8);
            return httpResponse;
        }

        private string CreateErrorMessageAsXml(HttpResponseData httpResponse, string code, string errorText)
        {
            AddHeaders(httpResponse);
            var messageBody = new StringBuilder();
            var settings = new XmlWriterSettings() { OmitXmlDeclaration = true, Indent = true, };
            using var writer = XmlWriter.Create(messageBody, settings);
            writer.WriteStartElement("Error");
            writer.WriteElementString("Code", code);
            writer.WriteElementString("Message", errorText);
            writer.WriteEndElement();
            writer.Close();
            return messageBody.ToString();
        }

        private void AddHeaders(HttpResponseData httpResponseData)
        {
            httpResponseData.Headers.Add("CorrelationId", _correlationContext.Id);
        }
    }
}
