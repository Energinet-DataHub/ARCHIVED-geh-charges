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
using System.Text;
using System.Threading.Tasks;
using GreenEnergyHub.Charges.Infrastructure.Messaging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace GreenEnergyHub.Charges.MeteringPointCreatedReceiver
{
    public class MeteringPointCreatedHandler
    {
        /// <summary>
        /// The name of the function.
        /// Function name affects the URL and thus possibly dependent infrastructure.
        /// </summary>
        private const string FunctionName = "MeteringPointCreatedHandler";
        private readonly ICorrelationContext _correlationContext;
        private readonly ILogger _log;

        public MeteringPointCreatedHandler(
            ICorrelationContext correlationContext,
            [NotNull] ILoggerFactory loggerFactory)
        {
            _correlationContext = correlationContext;
            _log = loggerFactory.CreateLogger(nameof(MeteringPointCreatedHandler));
        }

        [Function(FunctionName)]
        public async Task<IActionResult> RunAsync(
            [QueueTrigger("%METERING_POINT_CREATED_QUEUE_NAME%", Connection = "METERING_POINT_CREATED_QUEUE_LISTENER_CONNECTION_STRING")]
            [NotNull] byte[] data,
            [NotNull] FunctionContext context)
        {
            var s = Encoding.Default.GetString(data);
            _log.LogInformation("Function {FunctionName} started to process a request, with content: {Data}", FunctionName, s);

            SetupCorrelationContext(context);

            return await Task.FromResult(new OkResult()).ConfigureAwait(false);
        }

        private void SetupCorrelationContext(FunctionContext context)
        {
            _correlationContext.CorrelationId = context.InvocationId;
        }
    }
}
