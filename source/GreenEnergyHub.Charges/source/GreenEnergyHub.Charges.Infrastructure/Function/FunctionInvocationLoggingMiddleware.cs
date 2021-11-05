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

using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.Logging;

namespace GreenEnergyHub.Charges.Infrastructure.Function
{
    public class FunctionInvocationLoggingMiddleware : IFunctionsWorkerMiddleware
    {
        private readonly ILoggerFactory _loggerFactory;

        public FunctionInvocationLoggingMiddleware([NotNull] ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory;
        }

        public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
        {
            var functionEndpointName = context.FunctionDefinition.Name;
            var logger = _loggerFactory.CreateLogger(functionEndpointName);

            logger.LogInformation("Function {FunctionName} started to process a request with invocation ID {InvocationId}", functionEndpointName, context.InvocationId);
            await next(context).ConfigureAwait(false);
            logger.LogInformation("Function {FunctionName} ended to process a request with invocation ID {InvocationId}", functionEndpointName, context.InvocationId);
        }
    }
}
