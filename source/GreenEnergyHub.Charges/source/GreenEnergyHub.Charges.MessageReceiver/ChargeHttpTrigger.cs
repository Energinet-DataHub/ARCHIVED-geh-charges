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
using System.Net;
using System.Threading.Tasks;
using GreenEnergyHub.Charges.Application.ChangeOfCharges;
using GreenEnergyHub.Charges.Domain.ChangeOfCharges.Message;
using GreenEnergyHub.Charges.Domain.ChangeOfCharges.Transaction;
using GreenEnergyHub.Json;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace GreenEnergyHub.Charges.MessageReceiver
{
    public static class ChargeHttpTrigger
    {
        private const string FunctionName = "ChargeHttpTrigger";

        [Function(FunctionName)]
        public static async Task<HttpResponseData> RunAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post")] [NotNull] HttpRequestData req,
            [NotNull] FunctionContext executionContext)
        {
            var logger = executionContext.GetLogger(FunctionName);
            logger.LogInformation("Function {FunctionName} started to process a request", FunctionName);

            var jsonDeserializer = GetRequireService<IJsonSerializer>(executionContext);
            var message = await GetChangeOfChargesMessageAsync(jsonDeserializer, req).ConfigureAwait(false);

            var changeOfChargesMessageHandler = GetRequireService<IChangeOfChargesMessageHandler>(executionContext);
            var status = await changeOfChargesMessageHandler.HandleAsync(message).ConfigureAwait(false);

            // Future features/stories will specify the expected output
            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(status).ConfigureAwait(false);

            return response;
        }

        private static T GetRequireService<T>(FunctionContext functionContext)
            where T : notnull
        {
            return functionContext.InstanceServices.GetRequiredService<T>();
        }

        /// <summary>
        ///     Mimic that we receive a message that can contain multiple transactions.
        ///     In upcoming stories we'll instead receive multi-transactions eBIX messages.
        /// </summary>
        private static async Task<ChangeOfChargesMessage> GetChangeOfChargesMessageAsync(
            IJsonSerializer jsonDeserializer,
            HttpRequestData req)
        {
            var transaction = (ChangeOfChargesTransaction)await jsonDeserializer
                .DeserializeAsync(req.Body, typeof(ChangeOfChargesTransaction))
                .ConfigureAwait(false);
            var message = new ChangeOfChargesMessage();
            message.Transactions.Add(transaction);
            return message;
        }
    }
}
