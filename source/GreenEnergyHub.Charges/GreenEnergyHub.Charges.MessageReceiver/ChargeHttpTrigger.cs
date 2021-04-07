using System.IO;
using System.Threading.Tasks;
using GreenEnergyHub.Charges.Application.ChangeOfCharges;
using GreenEnergyHub.Charges.Domain.ChangeOfCharges.Message;
using GreenEnergyHub.Charges.Domain.ChangeOfCharges.Transaction;
using GreenEnergyHub.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace GreenEnergyHub.Charges.MessageReceiver
{
    public class ChargeHttpTrigger
    {
        private const string FunctionName = "ChargeHttpTrigger";
        private readonly IJsonSerializer _jsonDeserializer;
        private readonly IChangeOfChargesMessageHandler _changeOfChargesMessageHandler;

        public ChargeHttpTrigger(
            IJsonSerializer jsonDeserializer,
            IChangeOfChargesMessageHandler changeOfChargesMessageHandler)
        {
            _jsonDeserializer = jsonDeserializer;
            _changeOfChargesMessageHandler = changeOfChargesMessageHandler;
        }

        [FunctionName(FunctionName)]
        public async Task<IActionResult> RunAsync(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("Function {FunctionName}started to process a request", FunctionName);
            var message = await GetChangeOfChargesMessageAsync(_jsonDeserializer, req).ConfigureAwait(false);
            var status = await _changeOfChargesMessageHandler.HandleAsync(message).ConfigureAwait(false);

            return new OkObjectResult(status);
        }

        private static async Task<ChangeOfChargesMessage> GetChangeOfChargesMessageAsync(
            IJsonSerializer jsonDeserializer,
            HttpRequest req)
        {
            var transaction = (ChangeOfChargesTransaction)await jsonDeserializer
                .DeserializeAsync(req.Body, typeof(ChangeOfChargesTransaction))
                .ConfigureAwait(false);
            transaction.CorrelationId = req.Headers["CorrelationId"];
            var message = new ChangeOfChargesMessage();
            message.Transactions.Add(transaction);
            return message;
        }
    }
}
