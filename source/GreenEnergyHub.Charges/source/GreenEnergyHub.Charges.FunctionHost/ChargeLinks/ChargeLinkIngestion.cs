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
using GreenEnergyHub.Charges.Application;
using GreenEnergyHub.Charges.Application.ChargeLinks.Handlers;
using GreenEnergyHub.Charges.Application.ChargeLinks.Handlers.Message;
using GreenEnergyHub.Charges.Domain.ChargeLinkCommands;
using GreenEnergyHub.Charges.FunctionHost.Common;
using GreenEnergyHub.Charges.Infrastructure.Messaging;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace GreenEnergyHub.Charges.FunctionHost.ChargeLinks
{
    public class ChargeLinkIngestion
    {
        /// <summary>
        /// The name of the function.
        /// Function name affects the URL and thus possibly dependent infrastructure.
        /// </summary>
        private readonly MessageExtractor<ChargeLinkCommand> _messageExtractor;
        private readonly IChargeLinkCommandHandler _chargeLinkCommandHandler;
        private readonly ICorrelationContext _correlationContext;

        public ChargeLinkIngestion(
            IChargeLinkCommandHandler chargeLinkCommandHandler,
            MessageExtractor<ChargeLinkCommand> messageExtractor,
            ICorrelationContext correlationContext)
        {
            _messageExtractor = messageExtractor;
            _correlationContext = correlationContext;
            _chargeLinkCommandHandler = chargeLinkCommandHandler;
        }

        [Function(IngestionFunctionNames.ChargeLinkIngestion)]
        public async Task<HttpResponseData> RunAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)]
            [NotNull] HttpRequestData req)
        {
            var command = await GetChargeLinkCommandAsync(req.Body).ConfigureAwait(false);

            var chargeLinksMessageResult = await _chargeLinkCommandHandler.HandleAsync(command).ConfigureAwait(false);

            return await CreateResponseAsync(req, chargeLinksMessageResult).ConfigureAwait(false);
        }

        private async Task<HttpResponseData> CreateResponseAsync(HttpRequestData req, ChargeLinksMessageResult? chargeLinksMessageResult)
        {
            var response = req.CreateResponse();
            await response.WriteAsJsonAsync(chargeLinksMessageResult);
            response.Headers.Add(HttpResponseHeaders.CorrelationId, _correlationContext.Id);
            return response;
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
    }
}
