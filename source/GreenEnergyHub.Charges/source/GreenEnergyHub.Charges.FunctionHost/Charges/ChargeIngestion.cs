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
using System.ComponentModel;
using System.Threading.Tasks;
using Energinet.DataHub.Core.App.Common.Abstractions.Actor;
using Energinet.DataHub.Core.App.FunctionApp.Middleware.CorrelationId;
using Energinet.DataHub.Core.Messaging.Transport.SchemaValidation;
using Energinet.DataHub.Core.SchemaValidation.Errors;
using GreenEnergyHub.Charges.Domain.Dtos.Messages.Command;
using GreenEnergyHub.Charges.FunctionHost.Charges.Handlers;
using GreenEnergyHub.Charges.Infrastructure.CimDeserialization.MarketDocument;
using GreenEnergyHub.Charges.Infrastructure.Core.Function;
using GreenEnergyHub.Charges.Infrastructure.Core.MessagingExtensions;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace GreenEnergyHub.Charges.FunctionHost.Charges
{
    public class ChargeIngestion
    {
        private readonly ILogger _logger;
        private readonly ICorrelationContext _correlationContext;
        private readonly IHttpResponseBuilder _httpResponseBuilder;
        private readonly ValidatingMessageExtractor<ChargeCommandBundle> _messageExtractor;
        private readonly IActorContext _actorContext;
        private readonly IChargeCommandBundleHandler _chargeCommandBundleHandler;

        public ChargeIngestion(
            ILoggerFactory loggerFactory,
            ICorrelationContext correlationContext,
            IHttpResponseBuilder httpResponseBuilder,
            ValidatingMessageExtractor<ChargeCommandBundle> messageExtractor,
            IActorContext actorContext,
            IChargeCommandBundleHandler chargeCommandBundleHandler)
        {
            _logger = loggerFactory.CreateLogger(nameof(ChargeIngestion));
            _correlationContext = correlationContext;
            _httpResponseBuilder = httpResponseBuilder;
            _messageExtractor = messageExtractor;
            _actorContext = actorContext;
            _chargeCommandBundleHandler = chargeCommandBundleHandler;
        }

        [Function(IngestionFunctionNames.ChargeIngestion)]
        public async Task<HttpResponseData> RunAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)]
            HttpRequestData request)
        {
            SchemaValidatedInboundMessage<ChargeCommandBundle> inboundMessage;
            try
            {
                inboundMessage = await ValidateMessageAsync(request).ConfigureAwait(false);
                if (inboundMessage.HasErrors)
                {
                    return await CreateSchemaValidationErrorAsync(
                        request,
                        inboundMessage.SchemaValidationError).ConfigureAwait(false);
                }
            }
            catch (Exception exception) when (exception is InvalidXmlValueException or InvalidEnumArgumentException)
            {
                _logger.LogError(
                    exception,
                    "Unable to parse request with correlation id: {CorrelationId}",
                    _correlationContext.Id);
                return _httpResponseBuilder.CreateBadRequestB2BResponse(request, exception.Message);
            }
            catch (SchemaValidationException exception)
            {
                return await CreateSchemaValidationErrorAsync(
                    request,
                    exception.SchemaValidationError).ConfigureAwait(false);
            }

            if (!AuthenticatedMatchesSenderId(inboundMessage))
            {
                var errorMessage = B2BErrorMessageFactory.CreateSenderNotAuthorizedErrorMessage().WriteAsXmlString();
                return _httpResponseBuilder.CreateBadRequestB2BResponse(request, errorMessage);
            }

            var bundle = inboundMessage.ValidatedMessage;
            await _chargeCommandBundleHandler.HandleAsync(bundle).ConfigureAwait(false);
            return _httpResponseBuilder.CreateAcceptedResponse(request);
        }

        private async Task<HttpResponseData> CreateSchemaValidationErrorAsync(
            HttpRequestData request,
            ErrorResponse error)
        {
            _logger.LogError(
                "Unable to schema validate request with correlation id: {CorrelationId}",
                _correlationContext.Id);
            return await _httpResponseBuilder
                .CreateBadRequestResponseAsync(request, error)
                .ConfigureAwait(false);
        }

        private bool AuthenticatedMatchesSenderId(SchemaValidatedInboundMessage<ChargeCommandBundle> inboundMessage)
        {
            var authorizedActor = _actorContext.CurrentActor;
            var senderId = inboundMessage.ValidatedMessage?.Document.Sender.MarketParticipantId;

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
