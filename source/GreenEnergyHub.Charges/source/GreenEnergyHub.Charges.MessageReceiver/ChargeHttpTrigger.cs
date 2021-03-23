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

using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using GreenEnergyHub.Charges.Application.ChangeOfCharges;
using GreenEnergyHub.Charges.Domain.ChangeOfCharges.Message;
using GreenEnergyHub.Charges.Domain.ChangeOfCharges.Transaction;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace GreenEnergyHub.Charges.MessageReceiver
{
    public static class ChargeHttpTrigger
    {
        [Function("ChargeHttpTrigger")]
        public static async Task<HttpResponseData> RunAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestData req,
            FunctionContext executionContext)
        {
            if (executionContext == null) throw new ArgumentNullException(nameof(executionContext));
            if (req == null) throw new ArgumentNullException(nameof(req));

            var logger = executionContext.GetLogger("ChargeHttpTrigger");
            logger.LogInformation("C# HTTP trigger function processed a request");

            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "text/plain; charset=utf-8");

            var message = await GetChangeOfChargesMessageAsync(req).ConfigureAwait(false);

            var changeOfChargesMessageHandler =
                executionContext.InstanceServices.GetRequiredService<IChangeOfChargesMessageHandler>();
            var status = await changeOfChargesMessageHandler.HandleAsync(message).ConfigureAwait(false);

            // Future features/stories are expected to specify the expected output
            await response.WriteAsJsonAsync(status).ConfigureAwait(false);

            return response;
        }

        private static async Task<ChangeOfChargesMessage> GetChangeOfChargesMessageAsync(HttpRequestData req)
        {
            // Mimic that we receive a message that can contain multiple transactions.
            // In upcoming stories we'll instead receive multi-transactions eBIX messages.
            var transaction = await JsonSerializer
                .DeserializeAsync<ChangeOfChargesTransaction>(req.Body)
                .ConfigureAwait(false);
            var message = new ChangeOfChargesMessage();
            message.Transactions.Add(transaction!);
            return message;
        }
    }
}
