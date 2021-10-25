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
using Energinet.DataHub.Charges.Libraries.DefaultChargeLink;
using Energinet.DataHub.Charges.Libraries.Enums;
using Energinet.DataHub.Charges.Libraries.Models;
using GreenEnergyHub.Charges.Domain.ChargeLinkCommandReceivedEvents;
using GreenEnergyHub.Charges.Domain.ChargeLinkCommands;
using GreenEnergyHub.Charges.Domain.CreateLinkCommandEvents;
using GreenEnergyHub.Charges.Domain.DefaultChargeLinks;
using GreenEnergyHub.Charges.Domain.MeteringPoints;
using Microsoft.Extensions.Logging;
using NodaTime;

namespace GreenEnergyHub.Charges.Application.ChargeLinks.Handlers
{
    public class CreateLinkCommandEventHandler : ICreateLinkCommandEventHandler
    {
        private readonly IDefaultChargeLinkRepository _defaultChargeLinkRepository;
        private readonly IChargeLinkCommandFactory _chargeLinkCommandFactory;
        private readonly IMessageDispatcher<ChargeLinkCommandReceivedEvent> _messageDispatcher;
        private readonly IClock _clock;
        private readonly IMeteringPointRepository _meteringPointRepository;
        private readonly IDefaultChargeLinkClient _defaultChargeLinkClient;
        private readonly IMessageMetaDataContext _messageMetaDataContext;
        private readonly ILogger _logger;

        public CreateLinkCommandEventHandler(
            IDefaultChargeLinkRepository defaultChargeLinkRepository,
            IChargeLinkCommandFactory chargeLinkCommandFactory,
            IMessageDispatcher<ChargeLinkCommandReceivedEvent> messageDispatcher,
            IClock clock,
            IMeteringPointRepository meteringPointRepository,
            IDefaultChargeLinkClient defaultChargeLinkClient,
            IMessageMetaDataContext messageMetaDataContext,
            ILoggerFactory loggerFactory)
        {
            _defaultChargeLinkRepository = defaultChargeLinkRepository;
            _chargeLinkCommandFactory = chargeLinkCommandFactory;
            _messageDispatcher = messageDispatcher;
            _clock = clock;
            _meteringPointRepository = meteringPointRepository;
            _defaultChargeLinkClient = defaultChargeLinkClient;
            _messageMetaDataContext = messageMetaDataContext;
            _logger = loggerFactory.CreateLogger(nameof(CreateLinkCommandEventHandler));
        }

        public async Task HandleAsync([NotNull] CreateLinkCommandEvent createLinkCommandEvent, string correlationId)
        {
            if (_messageMetaDataContext.ReplyTo == null || !_messageMetaDataContext.ReplyTo.Any())
            {
                _logger.LogError("The reply queue name was absent or empty, could not handle request" +
                                 " CreateDefaultChargeLinks on metering point " +
                                 $"with id: {createLinkCommandEvent.MeteringPointId} " +
                                 $"and correlation id: {correlationId}");

                throw new ArgumentNullException(nameof(_messageMetaDataContext.ReplyTo));
            }

            var meteringPoint = await GetMeteringPointOrReplyWithFailedAsync(
                createLinkCommandEvent.MeteringPointId,
                correlationId,
                _messageMetaDataContext.ReplyTo).ConfigureAwait(false);

            if (meteringPoint == null)
                return;

            var defaultChargeLinks =
                (await GetDefaultChargeLinksOrReplyWithSucceededAsync(
                    meteringPoint,
                    correlationId,
                    _messageMetaDataContext.ReplyTo).ConfigureAwait(false)).ToList();

            if (!defaultChargeLinks.Any())
                return;

            await CreateAndDispatchCreateLinkCommandEventIfApplicableForLinkingAsync(
                createLinkCommandEvent,
                correlationId,
                defaultChargeLinks,
                meteringPoint);
        }

        private async Task CreateAndDispatchCreateLinkCommandEventIfApplicableForLinkingAsync(
            CreateLinkCommandEvent createLinkCommandEvent,
            string correlationId,
            List<DefaultChargeLink> defaultChargeLinks,
            MeteringPoint meteringPoint)
        {
            var chargeLinkCommands = new List<ChargeLinkCommand>();

            foreach (var defaultChargeLink in defaultChargeLinks)
            {
                if (defaultChargeLink.ApplicableForLinking(
                    meteringPoint.EffectiveDate,
                    meteringPoint.MeteringPointType))
                {
                    chargeLinkCommands.Add(await _chargeLinkCommandFactory.CreateAsync(
                        createLinkCommandEvent,
                        defaultChargeLink,
                        correlationId).ConfigureAwait(false));
                }
            }

            var chargeLinkCommandReceivedEvent = new ChargeLinkCommandReceivedEvent(
                _clock.GetCurrentInstant(),
                correlationId,
                chargeLinkCommands);

            await _messageDispatcher.DispatchAsync(chargeLinkCommandReceivedEvent).ConfigureAwait(false);
        }

        private async Task<MeteringPoint?> GetMeteringPointOrReplyWithFailedAsync(
            string meteringPointId,
            string correlationId,
            string replyQueueName)
        {
            var meteringPoint = await _meteringPointRepository
                .GetOrNullAsync(meteringPointId).ConfigureAwait(false);

            if (meteringPoint == null)
            {
                await _defaultChargeLinkClient
                    .CreateDefaultChargeLinksFailedReplyAsync(
                        new CreateDefaultChargeLinksFailedDto(
                            meteringPointId,
                            ErrorCode.MeteringPointUnknown),
                        correlationId,
                        replyQueueName).ConfigureAwait(false);

                return null;
            }

            return meteringPoint;
        }

        private async Task<IEnumerable<DefaultChargeLink>> GetDefaultChargeLinksOrReplyWithSucceededAsync(
            MeteringPoint meteringPoint,
            string correlationId,
            string replyQueueName)
        {
            var defaultChargeLinks = await _defaultChargeLinkRepository
                .GetAsync(meteringPoint.MeteringPointType).ConfigureAwait(false);

            var defaultChargeLinksOrReplyWithSucceededAsync =
                defaultChargeLinks as DefaultChargeLink[] ?? defaultChargeLinks.ToArray();

            if (!defaultChargeLinksOrReplyWithSucceededAsync.Any())
            {
                await _defaultChargeLinkClient
                    .CreateDefaultChargeLinksSucceededReplyAsync(
                        new CreateDefaultChargeLinksSucceededDto(
                            meteringPoint.MeteringPointId,
                            false),
                        correlationId,
                        replyQueueName).ConfigureAwait(false);
                return new List<DefaultChargeLink>();
            }

            return defaultChargeLinksOrReplyWithSucceededAsync;
        }
    }
}
