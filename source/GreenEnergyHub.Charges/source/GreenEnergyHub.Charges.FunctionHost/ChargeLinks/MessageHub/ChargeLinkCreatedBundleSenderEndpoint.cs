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
using GreenEnergyHub.Charges.Infrastructure.SyncRequest;
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
        private readonly ISyncRequestMetaDataFactory _syncRequestMetaDataFactory;

        public ChargeLinkCreatedBundleSenderEndpoint(
            IChargeLinkCreatedBundleSender chargeLinkCreatedBundleSender,
            ILoggerFactory loggerFactory,
            IRequestBundleParser requestBundleParser,
            ISyncRequestMetaDataFactory syncRequestMetaDataFactory)
        {
            _chargeLinkCreatedBundleSender = chargeLinkCreatedBundleSender;
            _requestBundleParser = requestBundleParser;
            _syncRequestMetaDataFactory = syncRequestMetaDataFactory;
            _log = loggerFactory.CreateLogger(nameof(ChargeLinkDataAvailableNotifierEndpoint));
        }

        [Function(FunctionName)]
        public async Task RunAsync(
            [ServiceBusTrigger("%MESSAGEHUB_BUNDLEREQUEST_QUEUE%", Connection = "INTEGRATIONEVENT_LISTENER_CONNECTION_STRING", IsSessionsEnabled = true)]
            byte[] data,
            FunctionContext functionContext)
        {
            _log.LogInformation("Function {FunctionName} started to process a request with size {Size}", FunctionName, data.Length);

            var request = _requestBundleParser.Parse(data);
            var metadata = _syncRequestMetaDataFactory.Create(functionContext);
            await _chargeLinkCreatedBundleSender.SendAsync(request, metadata).ConfigureAwait(false);
        }
    }
}
