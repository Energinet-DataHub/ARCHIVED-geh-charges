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
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Energinet.DataHub.Core.App.FunctionApp.Middleware.CorrelationId;
using GreenEnergyHub.Charges.Application.ChargeLinks.CreateDefaultChargeLinkReplier;
using GreenEnergyHub.Charges.Contracts;
using GreenEnergyHub.Charges.Domain.DefaultChargeLinks;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeLinksCommands;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeLinksReceivedEvents;
using GreenEnergyHub.Charges.Domain.Dtos.CreateDefaultChargeLinksRequests;
using GreenEnergyHub.Charges.Domain.MeteringPoints;
using GreenEnergyHub.Charges.MessageHub.AvailableData.Messaging;
using Microsoft.Extensions.Logging;
using NodaTime;

namespace GreenEnergyHub.Charges.Application.ChargeLinks.Handlers
{
    public class CreateLinkRequestHandler : ICreateLinkRequestHandler
    {
        private readonly IDefaultChargeLinkRepository _defaultChargeLinkRepository;
        private readonly IChargeLinksCommandFactory _chargeLinksCommandFactory;
        private readonly IMessageDispatcher<ChargeLinksReceivedEvent> _messageDispatcher;
        private readonly IClock _clock;
        private readonly IMeteringPointRepository _meteringPointRepository;
        private readonly ICreateDefaultChargeLinksReplier _createDefaultChargeLinksReplier;
        private readonly IMessageMetaDataContext _messageMetaDataContext;
        private readonly ILogger _logger;
        private readonly ICorrelationContext _correlationIdContext;

        public CreateLinkRequestHandler(
            IDefaultChargeLinkRepository defaultChargeLinkRepository,
            IChargeLinksCommandFactory chargeLinksCommandFactory,
            IMessageDispatcher<ChargeLinksReceivedEvent> messageDispatcher,
            IClock clock,
            IMeteringPointRepository meteringPointRepository,
            ICreateDefaultChargeLinksReplier createDefaultChargeLinksReplier,
            IMessageMetaDataContext messageMetaDataContext,
            ILoggerFactory loggerFactory,
            ICorrelationContext correlationIdContext)
        {
            _defaultChargeLinkRepository = defaultChargeLinkRepository;
            _chargeLinksCommandFactory = chargeLinksCommandFactory;
            _messageDispatcher = messageDispatcher;
            _clock = clock;
            _meteringPointRepository = meteringPointRepository;
            _createDefaultChargeLinksReplier = createDefaultChargeLinksReplier;
            _messageMetaDataContext = messageMetaDataContext;
            _correlationIdContext = correlationIdContext;
            _logger = loggerFactory.CreateLogger(nameof(CreateLinkRequestHandler));
        }

        public async Task HandleAsync(CreateDefaultChargeLinksRequest createDefaultChargeLinksRequest)
        {
            if (!_messageMetaDataContext.IsReplyToSet())
            {
                var errorMessage = $"Could not handle request CreateDefaultChargeLinks on metering point with id: " +
                                   $"{createDefaultChargeLinksRequest.MeteringPointId} and " +
                                   $"correlation id: {_correlationIdContext.Id}";
                _logger.LogError("The reply queue name was absent or empty: {errorMessage}", errorMessage);

                throw new ArgumentNullException(nameof(_messageMetaDataContext.ReplyTo));
            }

            var meteringPoint =
                await GetMeteringPointAsync(createDefaultChargeLinksRequest.MeteringPointId).ConfigureAwait(false);

            if (meteringPoint == null)
            {
                await ReplyWithFailedAsync(
                    createDefaultChargeLinksRequest.MeteringPointId,
                    _messageMetaDataContext.ReplyTo).ConfigureAwait(false);
                return;
            }

            var defaultChargeLinks =
                (await GetDefaultChargeLinksAsync(meteringPoint).ConfigureAwait(false)).ToList();

            if (!defaultChargeLinks.Any())
            {
                await ReplyWithSucceededAsync(
                    meteringPoint.MeteringPointId,
                    _messageMetaDataContext.ReplyTo).ConfigureAwait(false);
                return;
            }

            await CreateAndDispatchChargeLinksReceivedEventIfApplicableForLinkingAsync(
                createDefaultChargeLinksRequest,
                defaultChargeLinks,
                meteringPoint)
                .ConfigureAwait(false);
        }

        private async Task<MeteringPoint?> GetMeteringPointAsync(string meteringPointId)
        {
            var meteringPoint = await _meteringPointRepository
                .GetOrNullAsync(meteringPointId).ConfigureAwait(false);
            return meteringPoint;
        }

        private async Task<IEnumerable<DefaultChargeLink>> GetDefaultChargeLinksAsync(
            MeteringPoint meteringPoint)
        {
            var defaultChargeLinks = (await _defaultChargeLinkRepository
                .GetAsync(meteringPoint.MeteringPointType).ConfigureAwait(false)).ToList();

            return defaultChargeLinks;
        }

        private async Task ReplyWithFailedAsync(string meteringPointId, string replyTo)
        {
            await _createDefaultChargeLinksReplier
                .ReplyWithFailedAsync(
                        meteringPointId,
                        ErrorCode.MeteringPointUnknown,
                        replyTo).ConfigureAwait(false);
        }

        private async Task ReplyWithSucceededAsync(string meteringPointId, string replyTo)
        {
            await _createDefaultChargeLinksReplier
                .ReplyWithSucceededAsync(
                    meteringPointId,
                    false,
                    replyTo)
                .ConfigureAwait(false);
        }

        private async Task CreateAndDispatchChargeLinksReceivedEventIfApplicableForLinkingAsync(
            CreateDefaultChargeLinksRequest createDefaultChargeLinksRequest,
            IEnumerable<DefaultChargeLink> defaultChargeLinks,
            MeteringPoint meteringPoint)
        {
            var chargeLinksApplicableForLinking = defaultChargeLinks
                .Where(d => d.ApplicableForLinking(
                    meteringPoint.EffectiveDate,
                    meteringPoint.MeteringPointType)).ToList();

            var chargeLinksCommand = await _chargeLinksCommandFactory
                .CreateAsync(createDefaultChargeLinksRequest, chargeLinksApplicableForLinking)
                .ConfigureAwait(false);

            var chargeLinksReceivedEvent = new ChargeLinksReceivedEvent(
                _clock.GetCurrentInstant(),
                chargeLinksCommand);

            await _messageDispatcher.DispatchAsync(chargeLinksReceivedEvent).ConfigureAwait(false);
        }
    }
}
