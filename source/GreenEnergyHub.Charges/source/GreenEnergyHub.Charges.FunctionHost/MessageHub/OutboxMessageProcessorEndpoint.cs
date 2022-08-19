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

using System.Threading.Tasks;
using Energinet.DataHub.Core.App.FunctionApp.Middleware.CorrelationId;
using GreenEnergyHub.Charges.Application.Charges.Events;
using GreenEnergyHub.Charges.Application.Persistence;
using GreenEnergyHub.Charges.FunctionHost.Common;
using GreenEnergyHub.Charges.Infrastructure.Outbox;
using GreenEnergyHub.Charges.Infrastructure.Persistence.Repositories;
using GreenEnergyHub.Charges.MessageHub.MessageHub;
using GreenEnergyHub.Charges.MessageHub.Models.AvailableChargeReceiptData;
using GreenEnergyHub.Json;
using Microsoft.Azure.Functions.Worker;
using NodaTime;

namespace GreenEnergyHub.Charges.FunctionHost.MessageHub
{
    public class OutboxMessageProcessorEndpoint
    {
        private const string FunctionName = nameof(OutboxMessageProcessorEndpoint);
        private readonly IOutboxMessageRepository _outboxMessageRepository;
        private readonly IAvailableDataNotifier<AvailableChargeReceiptData, ChargePriceOperationsRejectedEvent> _availableDataNotifier;
        private readonly IJsonSerializer _jsonSerializer;
        private readonly IClock _clock;
        private readonly ICorrelationContext _correlationContext;
        private readonly IUnitOfWork _unitOfWork;

        public OutboxMessageProcessorEndpoint(
            IOutboxMessageRepository outboxMessageRepository,
            IAvailableDataNotifier<AvailableChargeReceiptData, ChargePriceOperationsRejectedEvent> availableDataNotifier,
            IJsonSerializer jsonSerializer,
            IClock clock,
            ICorrelationContext correlationContext,
            IUnitOfWork unitOfWork)
        {
            _outboxMessageRepository = outboxMessageRepository;
            _availableDataNotifier = availableDataNotifier;
            _jsonSerializer = jsonSerializer;
            _clock = clock;
            _correlationContext = correlationContext;
            _unitOfWork = unitOfWork;
        }

        [Function(FunctionName)]
        public async Task RunAsync(
            [TimerTrigger(TimerTriggerTimeConstants.Every10Seconds)] TimerInfo timerInfo)
        {
            OutboxMessage? outboxMessage;

            while ((outboxMessage = _outboxMessageRepository.GetNext()) != null)
            {
                var operationsRejectedEvent = _jsonSerializer.Deserialize<ChargePriceOperationsRejectedEvent>(outboxMessage.Data);
                _correlationContext.SetId(outboxMessage.CorrelationId);
                await _availableDataNotifier.NotifyAsync(operationsRejectedEvent).ConfigureAwait(false);
                outboxMessage.SetProcessed(_clock.GetCurrentInstant());
                await _unitOfWork.SaveChangesAsync().ConfigureAwait(false);
            }
        }
    }
}
