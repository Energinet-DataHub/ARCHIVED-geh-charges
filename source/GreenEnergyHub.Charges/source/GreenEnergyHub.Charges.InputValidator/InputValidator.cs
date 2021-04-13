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
using GreenEnergyHub.Charges.Application;
using GreenEnergyHub.Charges.Application.ChangeOfCharges;
using GreenEnergyHub.Charges.Domain.ChangeOfCharges.Fee;
using GreenEnergyHub.Charges.Domain.ChangeOfCharges.Tariff;
using GreenEnergyHub.Charges.Domain.ChangeOfCharges.Transaction;
using GreenEnergyHub.Json;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace GreenEnergyHub.Charges.LocalMessageServiceBusTopicTrigger
{
    public class InputValidator
    {
        private const string FunctionName = "InputValidator";
        private readonly IJsonSerializer _jsonDeserializer;
        private readonly IChangeOfChargeTransactionInputValidator _changeOfChargeTransactionInputValidator;
        private readonly IChangeOfChargesTransactionHandler _changeOfChargesTransactionHandler;

        public InputValidator(
            IJsonSerializer jsonDeserializer,
            IChangeOfChargeTransactionInputValidator changeOfChargeTransactionInputValidator,
            IChangeOfChargesTransactionHandler changeOfChargesTransactionHandler)
        {
            _jsonDeserializer = jsonDeserializer;
            _changeOfChargeTransactionInputValidator = changeOfChargeTransactionInputValidator;
            _changeOfChargesTransactionHandler = changeOfChargesTransactionHandler;
        }

        [FunctionName(FunctionName)]
        public async Task RunAsync(
            [ServiceBusTrigger(
            "%LOCAL_EVENTS_TOPIC_NAME%",
            "%LOCAL_EVENTS_SUBSCRIPTION_NAME%",
            Connection = "LOCAL_EVENTS_LISTENER_CONNECTION_STRING")]
            string jsonSerializedQueueItem,
            ILogger log)
        {
            var serviceBusMessage = _jsonDeserializer.Deserialize<ServiceBusMessageWrapper>(jsonSerializedQueueItem);
            var transaction = serviceBusMessage.Transaction;
            var result = await _changeOfChargeTransactionInputValidator.ValidateAsync(transaction).ConfigureAwait(false);

            if (!result.Errors.Any())
            {
                await _changeOfChargesTransactionHandler.HandleAsync(GetCommandFromChangeOfChargeTransactionInputValidationSucceded(transaction)).ConfigureAwait(false);
            }
            else
            {
                await _changeOfChargesTransactionHandler.HandleAsync(GetCommandFromChangeOfChargeTransactionInputValidationFailed(transaction)).ConfigureAwait(false);
            }

            log.LogDebug("Received event with charge type mRID '{mRID}'", transaction.ChargeTypeMRid);
        }

        private static ChangeOfChargesTransaction GetCommandFromChangeOfChargeTransactionInputValidationSucceded(ChangeOfChargesTransaction transaction)
        {
            return transaction.Type switch
            {
                "D01" => new FeeCreateInputValidationSucceded
                {
                    Period = transaction.Period,
                    Type = transaction.Type,
                    CorrelationId = transaction.CorrelationId,
                    MarketDocument = transaction.MarketDocument,
                    RequestDate = transaction.RequestDate,
                    LastUpdatedBy = transaction.LastUpdatedBy,
                    MktActivityRecord = transaction.MktActivityRecord,
                    ChargeTypeMRid = transaction.ChargeTypeMRid,
                    ChargeTypeOwnerMRid = transaction.ChargeTypeOwnerMRid,
                },
                _ => new TariffInputValidationSucceded
                {
                    Period = transaction.Period,
                    Type = transaction.Type,
                    CorrelationId = transaction.CorrelationId,
                    MarketDocument = transaction.MarketDocument,
                    RequestDate = transaction.RequestDate,
                    LastUpdatedBy = transaction.LastUpdatedBy,
                    MktActivityRecord = transaction.MktActivityRecord,
                    ChargeTypeMRid = transaction.ChargeTypeMRid,
                    ChargeTypeOwnerMRid = transaction.ChargeTypeOwnerMRid,
                }
            };
        }

        private static ChangeOfChargesTransaction GetCommandFromChangeOfChargeTransactionInputValidationFailed(ChangeOfChargesTransaction transaction)
        {
            return transaction.Type switch
            {
                "D01" => new FeeCreateInputValidationFailed
                {
                    Period = transaction.Period,
                    Type = transaction.Type,
                    CorrelationId = transaction.CorrelationId,
                    MarketDocument = transaction.MarketDocument,
                    RequestDate = transaction.RequestDate,
                    LastUpdatedBy = transaction.LastUpdatedBy,
                    MktActivityRecord = transaction.MktActivityRecord,
                    ChargeTypeMRid = transaction.ChargeTypeMRid,
                    ChargeTypeOwnerMRid = transaction.ChargeTypeOwnerMRid,
                },
                _ => new TariffInputValidationFailed
                {
                    Period = transaction.Period,
                    Type = transaction.Type,
                    CorrelationId = transaction.CorrelationId,
                    MarketDocument = transaction.MarketDocument,
                    RequestDate = transaction.RequestDate,
                    LastUpdatedBy = transaction.LastUpdatedBy,
                    MktActivityRecord = transaction.MktActivityRecord,
                    ChargeTypeMRid = transaction.ChargeTypeMRid,
                    ChargeTypeOwnerMRid = transaction.ChargeTypeOwnerMRid,
                }
            };
        }
    }
}
