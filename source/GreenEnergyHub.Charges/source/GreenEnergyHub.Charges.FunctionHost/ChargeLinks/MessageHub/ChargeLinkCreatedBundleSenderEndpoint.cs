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
using Energinet.DataHub.MessageHub.Client.Peek;
using GreenEnergyHub.Charges.Application.ChargeLinks.MessageHub;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace GreenEnergyHub.Charges.FunctionHost.ChargeLinks.MessageHub
{
    public class ChargeLinkCreatedBundleSenderEndpoint
    {
        private const string FunctionName = nameof(ChargeLinkCreatedBundleSenderEndpoint);
        private readonly IChargeLinkCreatedBundleSender _chargeLinkCreatedBundleSender;
        private readonly ILogger _log;
        private readonly IRequestBundleParser _requestBundleParser;

        public ChargeLinkCreatedBundleSenderEndpoint(
            IChargeLinkCreatedBundleSender chargeLinkCreatedBundleSender,
            ILoggerFactory loggerFactory,
            IRequestBundleParser requestBundleParser)
        {
            _chargeLinkCreatedBundleSender = chargeLinkCreatedBundleSender;
            _requestBundleParser = requestBundleParser;
            _log = loggerFactory.CreateLogger(nameof(ChargeLinkCreatedDataAvailableNotifierEndpoint));
        }

        [Function(FunctionName)]
        public async Task RunAsync(
            [ServiceBusTrigger("sbq-charges", Connection = "INTEGRATIONEVENT_LISTENER_CONNECTION_STRING")]
            byte[] data)
        {
            _log.LogInformation("Function {FunctionName} started to process a request with size {Size}", FunctionName, data.Length);

            var request = _requestBundleParser.Parse(data);
            await _chargeLinkCreatedBundleSender.SendAsync(request).ConfigureAwait(false);
        }
    }
}
