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
using GreenEnergyHub.Charges.Application.Charges.MessageHub;
using GreenEnergyHub.Charges.FunctionHost.Common;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace GreenEnergyHub.Charges.FunctionHost.Charges.MessageHub
{
    /// <summary>
    /// Trigger on request from MessageHub to create a bundle
    /// and create bundle and send response to MessageHub.
    /// This is the RSM-034 CIM XML 'NotifyPriceList'.
    /// </summary>
    public class ChargeBundleSenderEndpoint
    {
        private const string FunctionName = nameof(ChargeBundleSenderEndpoint);
        private readonly IChargeBundleSender _chargeBundleSender;
        private readonly ILogger _log;
        private readonly IRequestBundleParser _requestBundleParser;

        public ChargeBundleSenderEndpoint(
            IChargeBundleSender chargeBundleSender,
            ILoggerFactory loggerFactory,
            IRequestBundleParser requestBundleParser)
        {
            _chargeBundleSender = chargeBundleSender;
            _requestBundleParser = requestBundleParser;
            _log = loggerFactory.CreateLogger(nameof(ChargeBundleSenderEndpoint));
        }

        [Function(FunctionName)]
        public async Task RunAsync(
            [ServiceBusTrigger(
                "%" + EnvironmentSettingNames.MessageHubBundleRequestQueue + "%",
                Connection = EnvironmentSettingNames.DataHubListenerConnectionString,
                IsSessionsEnabled = true)]
            byte[] data,
            FunctionContext functionContext)
        {
            _log.LogInformation("Function {FunctionName} started to process a request with size {Size}", FunctionName, data.Length);

            var request = _requestBundleParser.Parse(data);
            await _chargeBundleSender.SendAsync(request).ConfigureAwait(false);
        }
    }
}
