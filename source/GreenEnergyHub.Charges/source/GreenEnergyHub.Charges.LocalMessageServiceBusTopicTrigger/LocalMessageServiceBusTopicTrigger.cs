using System.Threading.Tasks;
using GreenEnergyHub.Charges.Application.ChangeOfCharges;
using GreenEnergyHub.Charges.Domain.ChangeOfCharges.Fee;
using GreenEnergyHub.Charges.Domain.ChangeOfCharges.Tariff;
using GreenEnergyHub.Charges.Domain.ChangeOfCharges.Transaction;
using GreenEnergyHub.Json;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace GreenEnergyHub.Charges.LocalMessageServiceBusTopicTrigger
{
    public class LocalMessageServiceBusTopicTrigger
    {
        private const string FunctionName = "LocalMessageServieBusTopicTrigger";
        private readonly IJsonSerializer _jsonDeserializer;
        private readonly IChangeOfChargeTransactionInputValidator _changeOfChargeTransactionInputValidator;
        private readonly IChangeOfChargesTransactionHandler _changeOfChargesTransactionHandler;

        public LocalMessageServiceBusTopicTrigger(
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
            "LOCAL_EVENTS_TOPIC_NAME",
            "LOCAL_EVENTS_SUBSCRIPTION_NAME",
            Connection = "LOCAL_EVENTS_CONNECTION_STRING")]
            string jsonSerializedQueueItem,
            ILogger log)
        {
            var transaction = _jsonDeserializer.Deserialize<ChangeOfChargesTransaction>(jsonSerializedQueueItem);
            var result = await _changeOfChargeTransactionInputValidator.ValidateAsync(transaction).ConfigureAwait(false);

            if (result.Success)
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
