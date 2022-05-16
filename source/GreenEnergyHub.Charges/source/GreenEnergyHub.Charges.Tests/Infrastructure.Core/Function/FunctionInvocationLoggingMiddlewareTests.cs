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
using AutoFixture.Xunit2;
using GreenEnergyHub.Charges.Infrastructure.Core.Function;
using GreenEnergyHub.Charges.TestCore.Attributes;
using GreenEnergyHub.TestHelpers;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Infrastructure.Core.Function
{
    [UnitTest]
    public class FunctionInvocationLoggingMiddlewareTests
    {
        [Theory]
        [InlineAutoMoqData]
        public async Task Invoke_WhenCalled_ThenLoggingIsPerformed(
            [Frozen] Mock<FunctionContext> executionContextMock,
            [Frozen] Mock<ILoggerFactory> loggerFactory,
            [Frozen] Mock<ILogger> logger)
        {
            // Arrange
            executionContextMock.Setup(x => x.FunctionDefinition.Name).Returns("TestFunction");
            executionContextMock.Setup(x => x.InvocationId).Returns("TestInvocationId");
            var executionContext = executionContextMock.Object;
            loggerFactory.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(logger.Object);

            var sut = new FunctionInvocationLoggingMiddleware(loggerFactory.Object);

            // Act
            await sut.Invoke(executionContext, Next);

            // Assert
            var functionName = executionContext.FunctionDefinition.Name;
            var invocationId = executionContext.InvocationId;

            logger.VerifyLoggerWasCalled(
                $"Function {functionName} started to process a request with invocation ID {invocationId}",
                LogLevel.Information);

            logger.VerifyLoggerWasCalled(
                $"Function {functionName} ended to process a request with invocation ID {invocationId}",
                LogLevel.Information);
        }

        private static Task Next(FunctionContext context)
        {
            return Task.CompletedTask;
        }
    }
}
