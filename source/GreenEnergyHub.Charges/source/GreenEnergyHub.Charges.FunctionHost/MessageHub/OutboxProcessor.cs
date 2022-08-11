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
using Energinet.DataHub.Core.JsonSerialization;
using GreenEnergyHub.Charges.Application.Charges.Events;
using GreenEnergyHub.Charges.Infrastructure.Outbox;
using GreenEnergyHub.Charges.Infrastructure.Persistence.Repositories;
using GreenEnergyHub.Charges.MessageHub.MessageHub;
using GreenEnergyHub.Charges.MessageHub.Models.AvailableChargeReceiptData;
using Microsoft.Azure.Functions.Worker;

namespace GreenEnergyHub.Charges.FunctionHost.MessageHub
{
    public class OutboxProcessor
    {
        private const string FunctionName = nameof(OutboxProcessor);
        private readonly IOutboxMessageRepository _outboxMessageRepository;
        private readonly IAvailableDataNotifier<AvailableChargeReceiptData, OperationsRejectedEvent> _availableDataNotifier;
        private readonly IJsonSerializer _jsonSerializer;

        public OutboxProcessor(
            IOutboxMessageRepository outboxMessageRepository,
            IAvailableDataNotifier<AvailableChargeReceiptData, OperationsRejectedEvent> availableDataNotifier,
            IJsonSerializer jsonSerializer)
        {
            _outboxMessageRepository = outboxMessageRepository;
            _availableDataNotifier = availableDataNotifier;
            _jsonSerializer = jsonSerializer;
        }

        [Function(FunctionName)]
        public async Task RunAsync(
            [TimerTrigger("*/10 * * * * *")] TimerInfo timerInfo,
            FunctionContext context)
        {
            OutboxMessage? outboxMessage;

            while ((outboxMessage = _outboxMessageRepository.GetNext()) != null)
            {
                var operationsRejectedEvent = _jsonSerializer.Deserialize<OperationsRejectedEvent>(outboxMessage.Data);
                await _availableDataNotifier.NotifyAsync(operationsRejectedEvent).ConfigureAwait(false);
                await Task.CompletedTask.ConfigureAwait(false);
            }

            // Fetch new outbox messages
            // Create data available
            // Notify message hub
            // Mark outbox message processed
        }
    }
}
