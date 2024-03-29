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

using System;
using System.Threading.Tasks;
using Energinet.DataHub.Core.App.FunctionApp.Middleware.CorrelationId;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.Logging;

namespace GreenEnergyHub.Charges.Infrastructure.Core.Function
{
    public class FunctionInvocationLoggingMiddleware : IFunctionsWorkerMiddleware
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly ICorrelationContext _correlationContext;

        public FunctionInvocationLoggingMiddleware(ILoggerFactory loggerFactory, ICorrelationContext correlationContext)
        {
            _loggerFactory = loggerFactory;
            _correlationContext = correlationContext;
        }

        public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
        {
            var functionEndpointName = context.FunctionDefinition.Name;
            var logger = _loggerFactory.CreateLogger(functionEndpointName);

            logger.LogInformation(
                "Function {FunctionName} started to process a request with invocation ID {InvocationId}",
                functionEndpointName,
                context.InvocationId);

            await next(context).ConfigureAwait(false);

            // Do not log CorrelationId when running Outbox as outbox might process multiple items with different CorrelationId
            if (functionEndpointName != "OutboxMessageProcessorEndpoint")
            {
                var correlationId = string.Empty;
                try
                {
                    correlationId = _correlationContext.Id;
                }
                catch (Exception e)
                {
                    logger.LogError(e, "Failed reading Correlation Id");
                }

                logger.LogInformation(
                    "Function {FunctionName} ended to process a request with invocation ID {InvocationId} and correlation ID {CorrelationId}",
                    functionEndpointName,
                    context.InvocationId,
                    correlationId);
            }
            else
            {
                logger.LogInformation(
                    "Function {FunctionName} ended to process a request with invocation ID {InvocationId}",
                    functionEndpointName,
                    context.InvocationId);
            }
        }
    }
}
