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

using System.Linq;
using System.Threading.Tasks;
using Energinet.DataHub.Core.App.Common.Abstractions.Actor;
using Energinet.DataHub.Core.Messaging.Transport.SchemaValidation;
using GreenEnergyHub.Charges.Application.Charges.Handlers;
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
        private readonly IChargesBundleHandler _chargesBundleHandler;
        private readonly IHttpResponseBuilder _httpResponseBuilder;
        private readonly ValidatingMessageExtractor<ChargeCommandBundle> _messageExtractor;
        private readonly IActorContext _actorContext;

        public ChargeIngestion(
            IChargesBundleHandler chargesBundleHandler,
            IHttpResponseBuilder httpResponseBuilder,
            ValidatingMessageExtractor<ChargeCommandBundle> messageExtractor,
            IActorContext actorContext)
        {
            _chargesBundleHandler = chargesBundleHandler;
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
            ChargeCommandNullChecker.ThrowExceptionIfRequiredPropertyIsNull(bundle.ChargeCommands);

            await _chargesBundleHandler.HandleAsync(bundle).ConfigureAwait(false);

            return _httpResponseBuilder.CreateAcceptedResponse(req);
        }

        private bool AuthenticatedMatchesSenderId(SchemaValidatedInboundMessage<ChargeCommandBundle> inboundMessage)
        {
            var authorizedActor = _actorContext.CurrentActor;
            var senderId = inboundMessage.ValidatedMessage?.ChargeCommands.First().Document.Sender.MarketParticipantId;

            return authorizedActor != null && senderId == authorizedActor.Identifier;
        }

        private async Task<SchemaValidatedInboundMessage<ChargeCommandBundle>> ValidateMessageAsync(HttpRequestData req)
        {
            return (SchemaValidatedInboundMessage<ChargeCommandBundle>)await _messageExtractor
                .ExtractAsync(req.Body)
                .ConfigureAwait(false);
        }
    }
}
