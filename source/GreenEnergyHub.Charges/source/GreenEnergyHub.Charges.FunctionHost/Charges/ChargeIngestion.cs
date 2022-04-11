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

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Energinet.DataHub.Core.App.Common.Abstractions.Actor;
using Energinet.DataHub.Core.Messaging.Transport.SchemaValidation;
using Energinet.DataHub.Core.SchemaValidation;
using Energinet.DataHub.Core.SchemaValidation.Errors;
using GreenEnergyHub.Charges.Application.Charges.Handlers;
using GreenEnergyHub.Charges.Application.Charges.Handlers.Message;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommands;
using GreenEnergyHub.Charges.Infrastructure.Core.Function;
using GreenEnergyHub.Charges.Infrastructure.Core.MessagingExtensions;
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
        private readonly IHttpResponseBuilder _httpResponseBuilder;
        private readonly ValidatingMessageExtractor<ChargeCommandBundle> _messageExtractor;
        private readonly IActorContext _actorContext;

        public ChargeIngestion(
            IChargesMessageHandler chargesMessageHandler,
            IHttpResponseBuilder httpResponseBuilder,
            ValidatingMessageExtractor<ChargeCommandBundle> messageExtractor,
            IActorContext actorContext)
        {
            _chargesMessageHandler = chargesMessageHandler;
            _httpResponseBuilder = httpResponseBuilder;
            _messageExtractor = messageExtractor;
            _actorContext = actorContext;
        }

        [Function(IngestionFunctionNames.ChargeIngestion)]
        public async Task<HttpResponseData> RunAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)]
            HttpRequestData req)
        {
            var inboundMessage = await ValidateMessageAsync(req).ConfigureAwait(false);

            var validateAuthenticatedUserAgainstSenderIdErrorResponse =
                AuthenticatedUserDoesNotMatchSenderId(inboundMessage);
            if (validateAuthenticatedUserAgainstSenderIdErrorResponse is not null)
            {
                return await _httpResponseBuilder
                    .CreateBadRequestResponseAsync(req, validateAuthenticatedUserAgainstSenderIdErrorResponse.Value)
                    .ConfigureAwait(false);
            }

            if (inboundMessage.HasErrors)
            {
                return await _httpResponseBuilder
                    .CreateBadRequestResponseAsync(req, inboundMessage.SchemaValidationError)
                    .ConfigureAwait(false);
            }

            var message = GetChargesMessage(inboundMessage.ValidatedMessage);

            ChargeCommandNullChecker.ThrowExceptionIfRequiredPropertyIsNull(message.ChargeCommands);

            await _chargesMessageHandler.HandleAsync(message).ConfigureAwait(false);

            return _httpResponseBuilder.CreateAcceptedResponse(req);
        }

        private ErrorResponse? AuthenticatedUserDoesNotMatchSenderId(SchemaValidatedInboundMessage<ChargeCommandBundle> inboundMessage)
        {
            var authorizedActor = _actorContext.CurrentActor;
            var senderIds = inboundMessage.ValidatedMessage?.ChargeCommands.Select(x => x.Document.Sender.Id);

            if (senderIds is null)
                return null;

            if (authorizedActor is null)
                return null;

            foreach (var senderId in senderIds)
            {
                if (senderId != authorizedActor.Identifier)
                    return new ErrorResponse(new List<SchemaValidationError> { new(0, 0, "Sender id does not match id of current authenticated user.") });
            }

            return null;
        }

        private async Task<SchemaValidatedInboundMessage<ChargeCommandBundle>> ValidateMessageAsync(HttpRequestData req)
        {
            return (SchemaValidatedInboundMessage<ChargeCommandBundle>)await _messageExtractor
                .ExtractAsync(req.Body)
                .ConfigureAwait(false);
        }

        private ChargesMessage GetChargesMessage(ChargeCommandBundle chargeCommandBundle)
        {
            var message = new ChargesMessage();
            message.ChargeCommands.AddRange(chargeCommandBundle.ChargeCommands);
            return message;
        }
    }
}
