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
using System.Net;
using System.Threading.Tasks;
using Energinet.DataHub.Core.App.Common.Abstractions.Actor;
using Energinet.DataHub.Core.Messaging.Transport.SchemaValidation;
using GreenEnergyHub.Charges.Application.Charges.Handlers;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeInformationCommands;
using GreenEnergyHub.Charges.Domain.Dtos.ChargePriceCommands;
using GreenEnergyHub.Charges.Domain.Dtos.Messages;
using GreenEnergyHub.Charges.Infrastructure.Core.Function;
using GreenEnergyHub.Charges.Infrastructure.Core.MessagingExtensions;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace GreenEnergyHub.Charges.FunctionHost.Charges
{
    public class ChargeIngestion
    {
        /// <summary>
        /// The name of the function.
        /// Function name affects the URL and thus possibly dependent infrastructure.
        /// </summary>
        private readonly ILogger _logger;
        private readonly IChargeInformationCommandBundleHandler _chargeInformationCommandBundleHandler;
        private readonly IChargePriceCommandBundleHandler _chargePriceCommandBundleHandler;
        private readonly IHttpResponseBuilder _httpResponseBuilder;
        private readonly ValidatingMessageExtractor<IMessage> _messageExtractor;
        private readonly IActorContext _actorContext;

        public ChargeIngestion(
            ILogger logger,
            IChargeInformationCommandBundleHandler chargeInformationCommandBundleHandler,
            IChargePriceCommandBundleHandler chargePriceCommandBundleHandler,
            IHttpResponseBuilder httpResponseBuilder,
            ValidatingMessageExtractor<IMessage> messageExtractor,
            IActorContext actorContext)
        {
            _logger = logger;
            _chargeInformationCommandBundleHandler = chargeInformationCommandBundleHandler;
            _chargePriceCommandBundleHandler = chargePriceCommandBundleHandler;
            _httpResponseBuilder = httpResponseBuilder;
            _messageExtractor = messageExtractor;
            _actorContext = actorContext;
        }

        [Function(IngestionFunctionNames.ChargeIngestion)]
        public async Task<HttpResponseData> RunAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)]
            HttpRequestData req)
        {
            try
            {
                var inboundMessage = await ValidateMessageAsync(req).ConfigureAwait(false);

                if (inboundMessage.HasErrors)
                {
                    return await _httpResponseBuilder
                        .CreateBadRequestResponseAsync(req, inboundMessage.SchemaValidationError)
                        .ConfigureAwait(false);
                }

                if (AuthenticatedMatchesSenderId(inboundMessage) == false)
                {
                    return _httpResponseBuilder.CreateBadRequestB2BResponse(
                        req, B2BErrorCode.ActorIsNotWhoTheyClaimToBeErrorMessage);
                }

                var bundle = inboundMessage.ValidatedMessage;
                switch (bundle)
                {
                    case ChargeInformationCommandBundle commandBundle:
                        ChargeCommandNullChecker.ThrowExceptionIfRequiredPropertyIsNull(commandBundle);
                        await _chargeInformationCommandBundleHandler.HandleAsync(commandBundle).ConfigureAwait(false);
                        break;
                    case ChargePriceCommandBundle commandBundle:
                        await _chargePriceCommandBundleHandler.HandleAsync(commandBundle).ConfigureAwait(false);
                        break;
                }

                return _httpResponseBuilder.CreateResponse(req, HttpStatusCode.Accepted);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Unable to deserialize request");
                return _httpResponseBuilder.CreateResponse(req, HttpStatusCode.BadRequest);
            }
        }

        private bool AuthenticatedMatchesSenderId(SchemaValidatedInboundMessage<IMessage> inboundMessage)
        {
            var authorizedActor = _actorContext.CurrentActor;
            var senderId = inboundMessage.ValidatedMessage?.Document.Sender.MarketParticipantId;

            return authorizedActor != null && senderId == authorizedActor.Identifier;
        }

        private async Task<SchemaValidatedInboundMessage<IMessage>> ValidateMessageAsync(HttpRequestData req)
        {
            return (SchemaValidatedInboundMessage<IMessage>)await _messageExtractor
                .ExtractAsync(req.Body)
                .ConfigureAwait(false);
        }
    }
}
