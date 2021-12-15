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
using System.Net;
using System.Threading.Tasks;
using Energinet.DataHub.Core.Messaging.Transport.SchemaValidation;
using Energinet.DataHub.Core.SchemaValidation.Errors;
using Energinet.DataHub.Core.SchemaValidation.Extensions;
using GreenEnergyHub.Charges.Application.Charges.Handlers;
using GreenEnergyHub.Charges.Application.Charges.Handlers.Message;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommands;
using GreenEnergyHub.Charges.Infrastructure.Messaging;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace GreenEnergyHub.Charges.FunctionHost.Charges
{
    public class ChargeIngestion
    {
        /// <summary>
        /// The name of the function.
        /// Function name affects the URL and thus possibly dependent infrastructure.
        /// </summary>
        private readonly IChargesMessageHandler _chargesMessageHandler;
        private readonly InboundMessageExtractor<ChargeCommand> _messageExtractor;

        public ChargeIngestion(
            IChargesMessageHandler chargesMessageHandler,
            InboundMessageExtractor<ChargeCommand> messageExtractor)
        {
            _chargesMessageHandler = chargesMessageHandler;
            _messageExtractor = messageExtractor;
        }

        [Function(IngestionFunctionNames.ChargeIngestion)]
        public async Task<HttpResponseData> RunAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)]
            [NotNull] HttpRequestData req)
        {
            var validatedInboundMessage = await ValidateInboundChargesMessageAsync(req).ConfigureAwait(false);
            if (validatedInboundMessage.HasErrors)
            {
                return await CreateErrorResponseAsync(
                        req,
                        validatedInboundMessage.SchemaValidationError).ConfigureAwait(false);
            }

            var message = new ChargesMessage
            {
                Transactions = { validatedInboundMessage.ValidatedMessage },
            };

            foreach (var messageTransaction in message.Transactions)
            {
                ChargeCommandNullChecker.ThrowExceptionIfRequiredPropertyIsNull(messageTransaction);
            }

            var messageResult = await _chargesMessageHandler.HandleAsync(message)
                .ConfigureAwait(false);

            return await CreateJsonResponseAsync(req, messageResult).ConfigureAwait(false);
        }

        private async Task<SchemaValidatedInboundMessage<ChargeCommand>>
            ValidateInboundChargesMessageAsync(HttpRequestData req)
        {
            return (SchemaValidatedInboundMessage<ChargeCommand>)await _messageExtractor
                .ExtractAsync(req.Body)
                .ConfigureAwait(false);
        }

        private static async Task<HttpResponseData> CreateErrorResponseAsync(HttpRequestData req, ErrorResponse errorResponse)
        {
            var response = req.CreateResponse(HttpStatusCode.BadRequest);
            await errorResponse.WriteAsXmlAsync(response.Body).ConfigureAwait(false);
            return response;
        }

        private static async Task<HttpResponseData> CreateJsonResponseAsync(HttpRequestData req, ChargesMessageResult messageResult)
        {
            var response = req.CreateResponse(HttpStatusCode.Accepted);
            await response.WriteAsJsonAsync(messageResult).ConfigureAwait(false);
            return response;
        }
    }
}
