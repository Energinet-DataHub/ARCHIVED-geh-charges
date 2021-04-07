using System.Threading.Tasks;
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

        public LocalMessageServiceBusTopicTrigger(IJsonSerializer jsonDeserializer)
        {
            _jsonDeserializer = jsonDeserializer;
        }

        [FunctionName(FunctionName)]
        public Task RunAsync(
            [ServiceBusTrigger(
            "LOCAL_EVENTS_TOPIC_NAME",
            "LOCAL_EVENTS_SUBSCRIPTION_NAME",
            Connection = "LOCAL_EVENTS_CONNECTION_STRING")]
            string jsonSerializedQueueItem,
            ILogger log)
        {
            var transaction = _jsonDeserializer.Deserialize<ChangeOfChargesTransaction>(jsonSerializedQueueItem);
            log.LogDebug("Received event with charge type mRID '{mRID}'", transaction.ChargeTypeMRid);

            return Task.CompletedTask;
        }
    }
}
