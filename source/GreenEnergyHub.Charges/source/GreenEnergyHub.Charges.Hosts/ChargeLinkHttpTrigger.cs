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
using System.IO;
using System.Threading.Tasks;
using GreenEnergyHub.Charges.Application.ChargeLinks;
using GreenEnergyHub.Charges.Application.ChargeLinks.Handlers;
using GreenEnergyHub.Charges.Domain.ChargeLinks;
using GreenEnergyHub.Charges.Domain.ChargeLinks.Command;
using GreenEnergyHub.Charges.Domain.ChargeLinks.Events.Local;
using GreenEnergyHub.Charges.Infrastructure.Messaging;
using GreenEnergyHub.Messaging.Transport;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace GreenEnergyHub.Charges.Hosts
{
    public class ChargeLinkHttpTrigger
    {
        /// <summary>
        /// The name of the function.
        /// Function name affects the URL and thus possibly dependent infrastructure.
        /// </summary>
        private const string FunctionName = "ChargeLinkHttpTrigger";
        private readonly ICorrelationContext _correlationContext;
        private readonly MessageExtractor _messageExtractor;
        private readonly IChargeLinkCommandHandler _chargeLinkCommandHandler;
        private readonly ILogger _log;

        public ChargeLinkHttpTrigger(
            ICorrelationContext correlationContext,
            IChargeLinkCommandHandler chargeLinkCommandHandler,
            MessageExtractor messageExtractor,
            [NotNull] ILoggerFactory loggerFactory)
        {
            _correlationContext = correlationContext;

            _messageExtractor = messageExtractor;

            _chargeLinkCommandHandler = chargeLinkCommandHandler;
            _log = loggerFactory.CreateLogger(nameof(ChargeLinkHttpTrigger));
        }

        [Function(FunctionName)]
        public async Task<IActionResult> RunAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)]
            [NotNull] HttpRequestData req,
            [NotNull] FunctionContext context)
        {
            _log.LogInformation("Function {FunctionName} started to process a request", FunctionName);

            SetupCorrelationContext(context);

            var command = await GetChargeLinkCommandAsync(req.Body).ConfigureAwait(false);

            var chargeLinksMessageResult = await _chargeLinkCommandHandler.HandleAsync(command).ConfigureAwait(false);

            return new OkObjectResult(chargeLinksMessageResult);
        }

        private async Task<ChargeLinkCommand> GetChargeLinkCommandAsync(Stream stream)
        {
            var message = await ConvertStreamToBytesAsync(stream).ConfigureAwait(false);
            var command = (ChargeLinkCommand)await _messageExtractor.ExtractAsync(message).ConfigureAwait(false);

            return command;
        }

        private static async Task<byte[]> ConvertStreamToBytesAsync(Stream stream)
        {
            await using var ms = new MemoryStream();
            await stream.CopyToAsync(ms).ConfigureAwait(false);
            return ms.ToArray();
        }

        private void SetupCorrelationContext(FunctionContext context)
        {
            _correlationContext.CorrelationId = context.InvocationId;
        }
    }
}
