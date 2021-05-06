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
using GreenEnergyHub.Charges.Application;
using GreenEnergyHub.Charges.Domain.ChangeOfCharges.Transaction;
using GreenEnergyHub.Charges.Infrastructure.Messaging;
using GreenEnergyHub.Json;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace GreenEnergyHub.Charges.ChargeCommandReceiver
{
    public class ChargeCommandEndpoint
    {
        private const string FunctionName = nameof(ChargeCommandEndpoint);
        private readonly IChargeCommandHandler _chargeCommandHandler;
        private readonly ICorrelationContext _correlationContext;
        private readonly IJsonSerializer _jsonSerializer;

        public ChargeCommandEndpoint(
            IChargeCommandHandler chargeCommandHandler,
            ICorrelationContext correlationContext,
            IJsonSerializer jsonSerializer)
        {
            _chargeCommandHandler = chargeCommandHandler;
            _correlationContext = correlationContext;
            _jsonSerializer = jsonSerializer;
        }

        [FunctionName(FunctionName)]
        public async Task RunAsync(
            [ServiceBusTrigger(
            "%COMMAND_RECEIVED_TOPIC_NAME%",
            "%COMMAND_RECEIVED_SUBSCRIPTION_NAME%",
            Connection = "COMMAND_RECEIVED_LISTENER_CONNECTION_STRING")]
            byte[] message,
            ILogger log)
        {
            var jsonSerializedQueueItem = System.Text.Encoding.UTF8.GetString(message);
            var serviceBusMessage = _jsonSerializer.Deserialize<ServiceBusMessageWrapper<ChargeCommand>>(jsonSerializedQueueItem);
            var chargeCommand = serviceBusMessage.Command!;
            SetCorrelationContext(chargeCommand);
            await _chargeCommandHandler.HandleAsync(chargeCommand).ConfigureAwait(false);

            log.LogDebug("Received command with charge type mRID '{mRID}'", chargeCommand.ChargeDto.Id);
        }

        private void SetCorrelationContext(ChargeCommand command)
        {
            _correlationContext.CorrelationId = command.CorrelationId;
        }
    }
}
