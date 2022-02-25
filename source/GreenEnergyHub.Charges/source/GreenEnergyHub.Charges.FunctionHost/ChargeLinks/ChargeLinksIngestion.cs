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

using System.Threading.Tasks;
using Energinet.DataHub.Core.Messaging.Transport.SchemaValidation;
using GreenEnergyHub.Charges.Application.ChargeLinks.Handlers;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeLinksCommands;
using GreenEnergyHub.Charges.Infrastructure.Core.Function;
using GreenEnergyHub.Charges.Infrastructure.Core.MessagingExtensions;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace GreenEnergyHub.Charges.FunctionHost.ChargeLinks
{
    public class ChargeLinksIngestion
    {
        private readonly IChargeLinksCommandBundleHandler _chargeLinksCommandBundleHandler;
        private readonly IHttpResponseBuilder _httpResponseBuilder;

        /// <summary>
        /// The name of the function.
        /// Function name affects the URL and thus possibly dependent infrastructure.
        /// </summary>
        private readonly ValidatingMessageExtractor<ChargeLinksCommandBundle> _messageExtractor;

        public ChargeLinksIngestion(
            IChargeLinksCommandBundleHandler chargeLinksCommandBundleHandler,
            IHttpResponseBuilder httpResponseBuilder,
            ValidatingMessageExtractor<ChargeLinksCommandBundle> messageExtractor)
        {
            _chargeLinksCommandBundleHandler = chargeLinksCommandBundleHandler;
            _httpResponseBuilder = httpResponseBuilder;
            _messageExtractor = messageExtractor;
        }

        [Function(IngestionFunctionNames.ChargeLinksIngestion)]
        public async Task<HttpResponseData> RunAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)]
            HttpRequestData req)
        {
            var inboundMessage = await ValidateMessageAsync(req).ConfigureAwait(false);
            if (inboundMessage.HasErrors)
            {
                return await _httpResponseBuilder
                    .CreateBadRequestResponseAsync(req, inboundMessage.SchemaValidationError)
                    .ConfigureAwait(false);
            }

            await _chargeLinksCommandBundleHandler
                .HandleAsync(inboundMessage.ValidatedMessage)
                .ConfigureAwait(false);

            return _httpResponseBuilder.CreateAcceptedResponse(req);
        }

        private async Task<SchemaValidatedInboundMessage<ChargeLinksCommandBundle>> ValidateMessageAsync(HttpRequestData req)
        {
            return (SchemaValidatedInboundMessage<ChargeLinksCommandBundle>)await _messageExtractor
                .ExtractAsync(req.Body)
                .ConfigureAwait(false);
        }
    }
}
