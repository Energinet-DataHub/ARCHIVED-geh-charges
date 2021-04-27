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
using GreenEnergyHub.Json;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace GreenEnergyHub.Charges.ChargeCommandReceiver
{
    public class ChargeCommandEndpoint
    {
        private const string FunctionName = nameof(ChargeCommandEndpoint);
        private readonly IJsonSerializer _jsonDeserializer;
        private readonly IChargeCommandHandler _chargeCommandHandler;
        private readonly IExecutionExceptionHandler _executionExceptionHandler;

        public ChargeCommandEndpoint(
            IJsonSerializer jsonDeserializer,
            IChargeCommandHandler chargeCommandHandler,
            IExecutionExceptionHandler executionExceptionHandler)
        {
            _jsonDeserializer = jsonDeserializer;
            _chargeCommandHandler = chargeCommandHandler;
            _executionExceptionHandler = executionExceptionHandler;
        }

        [FunctionName(FunctionName)]
        public async Task RunAsync(
            [ServiceBusTrigger(
            "%COMMAND_RECEIVED_TOPIC_NAME%",
            "%COMMAND_RECEIVED_SUBSCRIPTION_NAME%",
            Connection = "COMMAND_RECEIVED_LISTENER_CONNECTION_STRING")]
            string jsonSerializedQueueItem,
            ILogger log)
        {
            var serviceBusMessage = _jsonDeserializer.Deserialize<ServiceBusMessageWrapper>(jsonSerializedQueueItem);
            var transaction = serviceBusMessage.Command;

            await _executionExceptionHandler.ExecuteChargeCommandAsync(
                    () => _chargeCommandHandler.HandleAsync(transaction), transaction)
                .ConfigureAwait(false);

            log.LogDebug("Received event with charge type mRID '{mRID}'", transaction.ChargeTypeMRid);
        }
    }
}
