﻿// Copyright 2020 Energinet DataHub A/S
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
using Energinet.DataHub.MessageHub.Model.Peek;
using GreenEnergyHub.Charges.FunctionHost.Common;
using GreenEnergyHub.Charges.MessageHub.MessageHub;
using Microsoft.Azure.Functions.Worker;

namespace GreenEnergyHub.Charges.FunctionHost.MessageHub
{
    /// <summary>
    /// Trigger on request from MessageHub to create a bundle
    /// and create bundle and send response to MessageHub.
    /// </summary>
    public class BundleSenderEndpoint
    {
        private const string FunctionName = nameof(BundleSenderEndpoint);
        private readonly IRequestBundleParser _requestBundleParser;
        private readonly IBundleSender _bundleSender;

        public BundleSenderEndpoint(IRequestBundleParser requestBundleParser, IBundleSender bundleSender)
        {
            _requestBundleParser = requestBundleParser;
            _bundleSender = bundleSender;
        }

        [Function(FunctionName)]
        public async Task RunAsync(
            [ServiceBusTrigger(
                "%" + EnvironmentSettingNames.MessageHubRequestQueue + "%",
                Connection = EnvironmentSettingNames.DataHubListenerConnectionString,
                IsSessionsEnabled = true)]
            byte[] data)
        {
            var request = _requestBundleParser.Parse(data);
            await _bundleSender.SendAsync(request);
        }
    }
}
