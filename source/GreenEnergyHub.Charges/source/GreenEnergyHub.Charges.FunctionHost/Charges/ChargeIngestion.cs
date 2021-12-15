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
using GreenEnergyHub.Charges.Application.Charges.Handlers;
using GreenEnergyHub.Charges.Application.Charges.Handlers.Message;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommands;
using GreenEnergyHub.Charges.Infrastructure.Core.MessagingExtensions;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace GreenEnergyHub.Charges.FunctionHost.Charges
{
    public class ChargeIngestion
    {
        /// <summary>
        /// The name of the function.
        /// Function name affects the URL and thus possibly dependent infrastructure.
        /// </summary>
        private readonly IChargesMessageHandler _chargesMessageHandler;
        private readonly MessageExtractor<ChargeCommand> _messageExtractor;

        public ChargeIngestion(
            IChargesMessageHandler chargesMessageHandler,
            MessageExtractor<ChargeCommand> messageExtractor)
        {
            _chargesMessageHandler = chargesMessageHandler;
            _messageExtractor = messageExtractor;
        }

        [Function(IngestionFunctionNames.ChargeIngestion)]
        public async Task<HttpResponseData> RunAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)]
            [NotNull] HttpRequestData req)
        {
            var message = await GetChargesMessageAsync(req).ConfigureAwait(false);

            foreach (var messageTransaction in message.Transactions)
            {
                ChargeCommandNullChecker.ThrowExceptionIfRequiredPropertyIsNull(messageTransaction);
            }

            var messageResult = await _chargesMessageHandler.HandleAsync(message)
                .ConfigureAwait(false);

            return await CreateJsonResponseAsync(req, messageResult);
        }

        private async Task<ChargesMessage> GetChargesMessageAsync(
            HttpRequestData req)
        {
            var message = new ChargesMessage();
            var command = (ChargeCommand)await _messageExtractor.ExtractAsync(req.Body).ConfigureAwait(false);

            message.Transactions.Add(command);
            return message;
        }

        private static async Task<HttpResponseData> CreateJsonResponseAsync(HttpRequestData req, ChargesMessageResult messageResult)
        {
            var response = req.CreateResponse();
            await response.WriteAsJsonAsync(messageResult);

            return response;
        }
    }
}
