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
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using GreenEnergyHub.Charges.Application.ChargeLinks.CreateDefaultChargeLinkReplier;
using GreenEnergyHub.Charges.Contracts;
using GreenEnergyHub.Charges.Domain.DefaultChargeLinks;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeLinkCommandReceivedEvents;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeLinkCommands;
using GreenEnergyHub.Charges.Domain.Dtos.CreateLinkRequest;
using GreenEnergyHub.Charges.Domain.MeteringPoints;
using Microsoft.Extensions.Logging;
using NodaTime;

namespace GreenEnergyHub.Charges.Application.ChargeLinks.Handlers
{
    public class CreateLinkCommandRequestHandler : ICreateLinkCommandRequestHandler
    {
        private readonly IDefaultChargeLinkRepository _defaultChargeLinkRepository;
        private readonly IChargeLinkCommandFactory _chargeLinkCommandFactory;
        private readonly IMessageDispatcher<ChargeLinkCommandReceivedEvent> _messageDispatcher;
        private readonly IClock _clock;
        private readonly IMeteringPointRepository _meteringPointRepository;
        private readonly ICreateDefaultChargeLinksReplier _createDefaultChargeLinksReplier;
        private readonly IMessageMetaDataContext _messageMetaDataContext;
        private readonly ILogger _logger;
        private readonly ICorrelationContext _correlationIdContext;

        public CreateLinkCommandRequestHandler(
            IDefaultChargeLinkRepository defaultChargeLinkRepository,
            IChargeLinkCommandFactory chargeLinkCommandFactory,
            IMessageDispatcher<ChargeLinkCommandReceivedEvent> messageDispatcher,
            IClock clock,
            IMeteringPointRepository meteringPointRepository,
            ICreateDefaultChargeLinksReplier createDefaultChargeLinksReplier,
            IMessageMetaDataContext messageMetaDataContext,
            ILoggerFactory loggerFactory,
            ICorrelationContext correlationIdContext)
        {
            _defaultChargeLinkRepository = defaultChargeLinkRepository;
            _chargeLinkCommandFactory = chargeLinkCommandFactory;
            _messageDispatcher = messageDispatcher;
            _clock = clock;
            _meteringPointRepository = meteringPointRepository;
            _createDefaultChargeLinksReplier = createDefaultChargeLinksReplier;
            _messageMetaDataContext = messageMetaDataContext;
            _correlationIdContext = correlationIdContext;
            _logger = loggerFactory.CreateLogger(nameof(CreateLinkCommandRequestHandler));
        }

        public async Task HandleAsync([NotNull] CreateLinkCommandEvent createLinkCommandEvent)
        {
            if (!_messageMetaDataContext.IsReplyToSet())
            {
                _logger.LogError($"The reply queue name was absent or empty, could not handle request CreateDefaultChargeLinks on metering point with id: {createLinkCommandEvent.MeteringPointId} and correlation id: {_correlationIdContext.Id}");

                throw new ArgumentNullException(nameof(_messageMetaDataContext.ReplyTo));
            }

            var meteringPoint =
                await GetMeteringPointAsync(createLinkCommandEvent.MeteringPointId).ConfigureAwait(false);

            if (meteringPoint == null)
            {
                await ReplyWithFailedAsync(
                    createLinkCommandEvent.MeteringPointId,
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

            await CreateAndDispatchChargeLinkCommandReceivedEventIfApplicableForLinkingAsync(
                createLinkCommandEvent,
                defaultChargeLinks,
                meteringPoint);
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

        private async Task CreateAndDispatchChargeLinkCommandReceivedEventIfApplicableForLinkingAsync(
            CreateLinkCommandEvent createLinkCommandEvent,
            IEnumerable<DefaultChargeLink> defaultChargeLinks,
            MeteringPoint meteringPoint)
        {
            var chargeLinksApplicableForLinking = defaultChargeLinks
                .Where(d => d.ApplicableForLinking(
                    meteringPoint.EffectiveDate,
                    meteringPoint.MeteringPointType)).ToList();

            var chargeLinksCommand = await _chargeLinkCommandFactory.CreateAsync(
                createLinkCommandEvent, chargeLinksApplicableForLinking);

            var chargeLinkCommandReceivedEvent = new ChargeLinkCommandReceivedEvent(
                _clock.GetCurrentInstant(),
                chargeLinksCommand);

            await _messageDispatcher.DispatchAsync(chargeLinkCommandReceivedEvent).ConfigureAwait(false);
        }
    }
}
