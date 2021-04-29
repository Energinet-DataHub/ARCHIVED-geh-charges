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
using System.Linq;
using System.Threading.Tasks;
using GreenEnergyHub.Charges.Application;
using GreenEnergyHub.Charges.Application.ChangeOfCharges;
using GreenEnergyHub.Charges.Domain.ChangeOfCharges.Fee;
using GreenEnergyHub.Charges.Domain.ChangeOfCharges.Message;
using GreenEnergyHub.Charges.Domain.ChangeOfCharges.Result;
using GreenEnergyHub.Charges.Domain.ChangeOfCharges.Tariff;
using GreenEnergyHub.Charges.Domain.ChangeOfCharges.Transaction;
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
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)]
            [NotNull] HttpRequest req,
            [NotNull] ExecutionContext context,
            ILogger log)
        {
            log.LogInformation("Function {FunctionName} started to process a request", FunctionName);
            var message = await GetChangeOfChargesMessageAsync(_jsonDeserializer, req, context).ConfigureAwait(false);

            var messageResult = await _changeOfChargesMessageHandler.HandleAsync(message)
                .ConfigureAwait(false);

            return new OkObjectResult(messageResult);
        }

        private static ChargeCommand GetCommandFromChangeOfChargeTransaction(ChargeCommand command)
        {
            return command.Type switch
            {
                "D01" => new FeeCreate
                {
                    Period = command.Period,
                    Type = command.Type,
                    CorrelationId = command.CorrelationId,
                    MarketDocument = command.MarketDocument,
                    RequestDate = command.RequestDate,
                    LastUpdatedBy = command.LastUpdatedBy,
                    MktActivityRecord = command.MktActivityRecord,
                    ChargeTypeMRid = command.ChargeTypeMRid,
                    ChargeTypeOwnerMRid = command.ChargeTypeOwnerMRid,
                },
                _ => new TariffCreate
                {
                    Period = command.Period,
                    Type = command.Type,
                    CorrelationId = command.CorrelationId,
                    MarketDocument = command.MarketDocument,
                    RequestDate = command.RequestDate,
                    LastUpdatedBy = command.LastUpdatedBy,
                    MktActivityRecord = command.MktActivityRecord,
                    ChargeTypeMRid = command.ChargeTypeMRid,
                    ChargeTypeOwnerMRid = command.ChargeTypeOwnerMRid,
                }
            };
        }

        private static async Task<ChangeOfChargesMessage> GetChangeOfChargesMessageAsync(
            IJsonSerializer jsonDeserializer,
            HttpRequest req,
            ExecutionContext executionContext)
        {
            var message = new ChangeOfChargesMessage();
            var transaction = (ChargeCommand)await jsonDeserializer
                .DeserializeAsync(req.Body, typeof(ChargeCommand))
                .ConfigureAwait(false);

            var command = GetCommandFromChangeOfChargeTransaction(transaction);
            command.CorrelationId = executionContext.InvocationId.ToString();
            message.Transactions.Add(command);
            return message;
        }
    }
}
