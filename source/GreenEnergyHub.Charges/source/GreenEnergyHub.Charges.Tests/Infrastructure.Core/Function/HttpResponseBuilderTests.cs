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
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Energinet.DataHub.Core.App.FunctionApp.Middleware.CorrelationId;
using Energinet.DataHub.Core.SchemaValidation;
using Energinet.DataHub.Core.SchemaValidation.Errors;
using FluentAssertions;
using GreenEnergyHub.Charges.Infrastructure.Core.Function;
using GreenEnergyHub.Charges.TestCore.Attributes;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Moq;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Infrastructure.Core.Function
{
    [UnitTest]
    public class HttpResponseBuilderTests
    {
        [Theory]
        [InlineAutoMoqData]
        public void CreateAcceptedResponse_Creates_AcceptedResponseWithCorrelationId(FunctionContext executionContext)
        {
            // Arrange
            var correlationContext = CreateCorrelationContext();
            var sut = new HttpResponseBuilder(correlationContext);
            var httpRequestData = CreateHttpRequestData(executionContext, "GET", "test", "http://localhost?Id=1");

            // Act
            var responseData = sut.CreateAcceptedResponse(httpRequestData);

            // Assert
            const string correlationIdKey = "CorrelationId";
            responseData.Headers.Should().ContainKey(correlationIdKey);
            responseData.Headers.First(x => x.Key == correlationIdKey).Value.First().Should().Be(correlationContext.Id);
            responseData.StatusCode.Should().Be(HttpStatusCode.Accepted);
        }

        [Theory]
        [InlineAutoMoqData]
        public async Task CreateBadRequestResponseAsync_Creates_BadRequestResponseWithCorrelationId(
            FunctionContext executionContext)
        {
            // Arrange
            var correlationContext = CreateCorrelationContext();
            var sut = new HttpResponseBuilder(correlationContext);
            var httpRequestData = CreateHttpRequestData(executionContext, "GET", "error", "http://localhost?Id=2");
            var schemaValidationError = new SchemaValidationError(1, 1, "some error");
            var errorResponse = new ErrorResponse(new List<SchemaValidationError> { schemaValidationError });

            // Act
            var responseData = await sut.CreateBadRequestResponseAsync(httpRequestData, errorResponse);

            // Assert
            const string correlationIdKey = "CorrelationId";
            responseData.Headers.Should().ContainKey(correlationIdKey);
            responseData.Headers.First(x => x.Key == correlationIdKey).Value.First().Should().Be(correlationContext.Id);
            responseData.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        private static ICorrelationContext CreateCorrelationContext()
        {
            var correlationContext = new CorrelationContext();
            correlationContext.SetParentId("parentId");
            correlationContext.SetId("id");
            return correlationContext;
        }

        private static HttpRequestData CreateHttpRequestData(
            FunctionContext executionContext, string method, string content, string url)
        {
            var httpRequestData = new Mock<HttpRequestData>(executionContext);
            httpRequestData.Setup(r => r.Method).Returns(method);

            var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));
            httpRequestData.Setup(r => r.Body).Returns(stream);

            httpRequestData.Setup(r => r.Url).Returns(new Uri(url));
            httpRequestData.Setup(r => r.CreateResponse()).Returns(() =>
            {
                var httpResponseData = new Mock<HttpResponseData>(executionContext);
                httpResponseData.SetupProperty(r => r.Headers, new HttpHeadersCollection());
                httpResponseData.SetupProperty(r => r.StatusCode);
                httpResponseData.SetupProperty(r => r.Body, new MemoryStream());
                return httpResponseData.Object;
            });

            return httpRequestData.Object;
        }
    }
}
