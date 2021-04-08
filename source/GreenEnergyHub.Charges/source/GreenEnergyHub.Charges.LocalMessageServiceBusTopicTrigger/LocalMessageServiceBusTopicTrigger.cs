using System.Threading.Tasks;
using GreenEnergyHub.Charges.Application.ChangeOfCharges;
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

        public LocalMessageServiceBusTopicTrigger(
            IJsonSerializer jsonDeserializer,
            IChangeOfChargeTransactionInputValidator changeOfChargeTransactionInputValidator)
        {
            _jsonDeserializer = jsonDeserializer;
            _changeOfChargeTransactionInputValidator = changeOfChargeTransactionInputValidator;
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
            log.LogDebug("Received event with charge type mRID '{mRID}'", transaction.ChargeTypeMRid);
        }
    }
}
