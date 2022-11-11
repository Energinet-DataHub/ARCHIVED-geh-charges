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
using System.ComponentModel;
using System.Threading.Tasks;
using Energinet.DataHub.Core.App.Common.Abstractions.Actor;
using Energinet.DataHub.Core.App.FunctionApp.Middleware.CorrelationId;
using Energinet.DataHub.Core.Messaging.Transport.SchemaValidation;
using GreenEnergyHub.Charges.Application.Charges.Handlers.ChargeInformation;
using GreenEnergyHub.Charges.Application.Charges.Handlers.ChargePrice;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeInformationCommands;
using GreenEnergyHub.Charges.Domain.Dtos.ChargePriceCommands;
using GreenEnergyHub.Charges.Domain.Dtos.Messages.Command;
using GreenEnergyHub.Charges.Infrastructure.CimDeserialization.MarketDocument;
using GreenEnergyHub.Charges.Infrastructure.Core.Function;
using GreenEnergyHub.Charges.Infrastructure.Core.MessagingExtensions;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace GreenEnergyHub.Charges.FunctionHost.Charges.Handlers
{
    public class ChargeIngestionHandler : IChargeIngestionHandler
    {
        private readonly ILogger _logger;
        private readonly ICorrelationContext _correlationContext;
        private readonly IChargeInformationCommandBundleHandler _chargeInformationCommandBundleHandler;
        private readonly IChargePriceCommandBundleHandler _chargePriceCommandBundleHandler;
        private readonly IHttpResponseBuilder _httpResponseBuilder;
        private readonly ValidatingMessageExtractor<ChargeCommandBundle> _messageExtractor;
        private readonly IActorContext _actorContext;

        public ChargeIngestionHandler(
            ILoggerFactory loggerFactory,
            ICorrelationContext correlationContext,
            IChargeInformationCommandBundleHandler chargeInformationCommandBundleHandler,
            IChargePriceCommandBundleHandler chargePriceCommandBundleHandler,
            IHttpResponseBuilder httpResponseBuilder,
            ValidatingMessageExtractor<ChargeCommandBundle> messageExtractor,
            IActorContext actorContext)
        {
            _logger = loggerFactory.CreateLogger(nameof(ChargeIngestion));
            _correlationContext = correlationContext;
            _chargeInformationCommandBundleHandler = chargeInformationCommandBundleHandler;
            _chargePriceCommandBundleHandler = chargePriceCommandBundleHandler;
            _httpResponseBuilder = httpResponseBuilder;
            _messageExtractor = messageExtractor;
            _actorContext = actorContext;
        }

        public async Task<HttpResponseData> HandleAsync(HttpRequestData request)
        {
            try
            {
                SchemaValidatedInboundMessage<ChargeCommandBundle> inboundMessage;
                try
                {
                    inboundMessage = await ValidateMessageAsync(request).ConfigureAwait(false);
                }
                catch (Exception exception) when (exception is InvalidXmlValueException or InvalidEnumArgumentException)
                {
                    _logger.LogError(
                        exception,
                        "Unable to parse request with correlation id: {CorrelationId}",
                        _correlationContext.Id);

                    return _httpResponseBuilder.CreateBadRequestB2BResponse(
                        request, B2BErrorCode.SyntaxValidationErrorMessage, exception.Message);
                }

                if (inboundMessage.HasErrors)
                {
                    return await _httpResponseBuilder
                        .CreateBadRequestResponseAsync(request, inboundMessage.SchemaValidationError)
                        .ConfigureAwait(false);
                }

                if (AuthenticatedMatchesSenderId(inboundMessage) == false)
                {
                    return _httpResponseBuilder.CreateBadRequestB2BResponse(
                        request, B2BErrorCode.ActorIsNotWhoTheyClaimToBeErrorMessage);
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

                return _httpResponseBuilder.CreateAcceptedResponse(request);
            }
            catch (SchemaValidationException exception)
            {
                _logger.LogError(
                    exception,
                    "Unable to schema validate request with correlation id: {CorrelationId}",
                    _correlationContext.Id);
                return await _httpResponseBuilder
                    .CreateBadRequestResponseAsync(request, exception.SchemaValidationError)
                    .ConfigureAwait(false);
            }
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
