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
using Energinet.DataHub.Core.App.FunctionApp.Middleware.CorrelationId;
using Energinet.DataHub.Core.Messaging.Transport.SchemaValidation;
using GreenEnergyHub.Charges.Application.Charges.Handlers;
using GreenEnergyHub.Charges.Domain.Charges;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeInformationCommands;
using GreenEnergyHub.Charges.Domain.Dtos.ChargePriceCommands;
using GreenEnergyHub.Charges.Domain.Dtos.Messages.Command;
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
        private readonly IChargeInformationCommandBundleHandler _chargeInformationCommandBundleHandler;
        private readonly IChargePriceCommandBundleHandler _chargePriceCommandBundleHandler;
        private readonly IHttpResponseBuilder _httpResponseBuilder;
        private readonly ValidatingMessageExtractor<ChargeCommandBundle> _messageExtractor;
        private readonly IActorContext _actorContext;

        public ChargeIngestion(
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

        [Function(IngestionFunctionNames.ChargeIngestion)]
        public async Task<HttpResponseData> RunAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)]
            HttpRequestData request)
        {
            try
            {
                var inboundMessage = await ValidateMessageAsync(request).ConfigureAwait(false);

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
                        // This is a temporary fix to support "old" price flow while new is under development
                        //await SupportOldFlowAsync(commandBundle).ConfigureAwait(false);
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

        private async Task SupportOldFlowAsync(ChargePriceCommandBundle priceCommandBundle)
        {
            var document = priceCommandBundle.Document;
            var chargeInformationCommands = priceCommandBundle.Commands
                .Select(priceCommand => priceCommand.Operations
                    .Select(priceOperation => new ChargeOperationDto(
                        priceOperation.Id,
                        priceOperation.Type,
                        priceOperation.ChargeId,
                        string.Empty,
                        string.Empty,
                        priceOperation.ChargeOwner,
                        Resolution.Unknown,
                        priceOperation.Resolution,
                        TaxIndicator.Unknown,
                        TransparentInvoicing.Unknown,
                        VatClassification.Unknown,
                        priceOperation.StartDate,
                        null,
                        priceOperation.PointsStartInterval,
                        priceOperation.PointsEndInterval,
                        priceOperation.Points))
                    .ToList())
                .Select(operations => new ChargeInformationCommand(document, operations))
                .ToList();

            var chargeInformationCommandBundle =
                new ChargeInformationCommandBundle(document, chargeInformationCommands);

            await _chargeInformationCommandBundleHandler
                .HandleAsync(chargeInformationCommandBundle)
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
