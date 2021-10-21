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

            var meteringPoint = await _meteringPointRepository
                .GetOrNullAsync(createLinkCommandEvent.MeteringPointId).ConfigureAwait(false);

            if (meteringPoint == null)
            {
                await _defaultChargeLinkClient
                    .CreateDefaultChargeLinksFailedReplyAsync(
                        new CreateDefaultChargeLinksFailedDto(
                            createLinkCommandEvent.MeteringPointId,
                            ErrorCode.MeteringPointUnknown),
                        correlationId,
                        _messageMetaDataContext.ReplyTo).ConfigureAwait(false);
                return;
            }

            var defaultChargeLinks = await _defaultChargeLinkRepository
                .GetAsync(meteringPoint.MeteringPointType).ConfigureAwait(false);

            var chargeLinks = defaultChargeLinks as DefaultChargeLink[] ?? defaultChargeLinks.ToArray();

            if (!chargeLinks.Any())
            {
                await _defaultChargeLinkClient
                    .CreateDefaultChargeLinksSucceededReplyAsync(
                        new CreateDefaultChargeLinksSucceededDto(
                            createLinkCommandEvent.MeteringPointId,
                            false),
                        correlationId,
                        _messageMetaDataContext.ReplyTo).ConfigureAwait(false);
                return;
            }

            var chargeLinkCommands = new List<ChargeLinkCommand>();

            foreach (var defaultChargeLink in chargeLinks)
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
    }
}
