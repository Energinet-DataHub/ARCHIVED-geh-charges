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
using GreenEnergyHub.Charges.Application.ChargeLinks.Handlers;
using GreenEnergyHub.Charges.Application.ChargeLinks.Handlers.Message;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeLinksCommands;
using GreenEnergyHub.Charges.Infrastructure.Messaging;
using GreenEnergyHub.Charges.Infrastructure.Messaging.Serialization;
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
        private readonly MessageExtractor<ChargeLinksCommand> _messageExtractor;
        private readonly IChargeLinksCommandHandler _chargeLinksCommandHandler;

        public ChargeLinkIngestion(
            IChargeLinksCommandHandler chargeLinksCommandHandler,
            MessageExtractor<ChargeLinksCommand> messageExtractor)
        {
            _messageExtractor = messageExtractor;
            _chargeLinksCommandHandler = chargeLinksCommandHandler;
        }

        [Function(IngestionFunctionNames.ChargeLinkIngestion)]
        public async Task<HttpResponseData> RunAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)]
            HttpRequestData req)
        {
            var validatedInboundMessage = await ValidateInboundChargeLinksMessageAsync(req).ConfigureAwait(false);
            if (validatedInboundMessage.HasErrors)
            {
                return await CreateErrorResponseAsync(
                    req,
                    validatedInboundMessage.SchemaValidationError).ConfigureAwait(false);
            }

            var chargeLinksMessageResult = await _chargeLinksCommandHandler
                .HandleAsync(validatedInboundMessage.ValidatedMessage)
                .ConfigureAwait(false);

            return await CreateJsonResponseAsync(req, chargeLinksMessageResult).ConfigureAwait(false);
        }

        private async Task<SchemaValidatedInboundMessage<ChargeLinksCommand>>
            ValidateInboundChargeLinksMessageAsync(HttpRequestData req)
        {
            return (SchemaValidatedInboundMessage<ChargeLinksCommand>)await _messageExtractor
                .ExtractAsync(req.Body)
                .ConfigureAwait(false);
        }

        private static async Task<HttpResponseData> CreateErrorResponseAsync(HttpRequestData req, ErrorResponse errorResponse)
        {
            var response = req.CreateResponse(HttpStatusCode.BadRequest);
            await errorResponse.WriteAsXmlAsync(response.Body).ConfigureAwait(false);
            return response;
        }

        private async Task<HttpResponseData> CreateJsonResponseAsync(HttpRequestData req, ChargeLinksMessageResult? chargeLinksMessageResult)
        {
            var response = req.CreateResponse(HttpStatusCode.Accepted);
            await response.WriteAsJsonAsync(chargeLinksMessageResult);
            return response;
        }
    }
}
