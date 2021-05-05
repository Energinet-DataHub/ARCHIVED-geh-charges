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

using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using GreenEnergyHub.Charges.Application;
using GreenEnergyHub.Charges.Application.ChangeOfCharges;
using GreenEnergyHub.Charges.Domain.ChangeOfCharges.Fee;
using GreenEnergyHub.Charges.Domain.ChangeOfCharges.Message;
using GreenEnergyHub.Charges.Domain.ChangeOfCharges.Tariff;
using GreenEnergyHub.Charges.Domain.ChangeOfCharges.Transaction;
using GreenEnergyHub.Charges.Infrastructure.Messaging;
using GreenEnergyHub.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace GreenEnergyHub.Charges.MessageReceiver
{
    public class ChargeHttpTrigger
    {
        /// <summary>
        /// The name of the function.
        /// Function name affects the URL and thus possibly dependent infrastructure.
        /// </summary>
        private const string FunctionName = "ChargeHttpTrigger";
        private readonly IJsonSerializer _jsonDeserializer;
        private readonly IChangeOfChargesMessageHandler _changeOfChargesMessageHandler;
        private readonly ICorrelationContext _correlationContext;

        public ChargeHttpTrigger(
            IJsonSerializer jsonDeserializer,
            IChangeOfChargesMessageHandler changeOfChargesMessageHandler,
            ICorrelationContext correlationContext)
        {
            _jsonDeserializer = jsonDeserializer;
            _changeOfChargesMessageHandler = changeOfChargesMessageHandler;
            _correlationContext = correlationContext;
        }

        [FunctionName(FunctionName)]
        public async Task<IActionResult> RunAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)]
            [NotNull] HttpRequest req,
            [NotNull] ExecutionContext context,
            ILogger log)
        {
            log.LogInformation("Function {FunctionName} started to process a request", FunctionName);

            SetupCorrelationContext(context);

            var message = await GetChangeOfChargesMessageAsync(_jsonDeserializer, req)
                .ConfigureAwait(false);

            foreach (var messageTransaction in message.Transactions)
            {
                ChargeCommandNullChecker.ThrowExceptionIfRequiredPropertyIsNull(messageTransaction);
            }

            var messageResult = await _changeOfChargesMessageHandler.HandleAsync(message)
                .ConfigureAwait(false);

            return new OkObjectResult(messageResult);
        }

        private async Task<ChangeOfChargesMessage> GetChangeOfChargesMessageAsync(
            IJsonSerializer jsonDeserializer,
            HttpRequest req)
        {
            var message = new ChangeOfChargesMessage();
            var command = (ChargeCommand)await jsonDeserializer
                .DeserializeAsync(req.Body, typeof(ChargeCommand))
                .ConfigureAwait(false);

            command.ChargeEvent.CorrelationId = _correlationContext.CorrelationId;
            message.Transactions.Add(command);
            return message;
        }

        private void SetupCorrelationContext(ExecutionContext context)
        {
            _correlationContext.CorrelationId = context.InvocationId.ToString();
        }
    }
}
