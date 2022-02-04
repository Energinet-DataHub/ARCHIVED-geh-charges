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
using System.IO;
using System.Threading.Tasks;
using Energinet.DataHub.MessageHub.Model.Model;
using GreenEnergyHub.Charges.MessageHub.MessageHub;
using Microsoft.Extensions.Logging;

namespace GreenEnergyHub.Charges.MessageHub.Infrastructure.Bundling
{
    public class BundleSender : IBundleSender
    {
        private readonly IBundleCreatorProvider _bundleCreatorProvider;
        private readonly IBundleReplier _bundleReplier;
        private readonly ILogger _logger;

        public BundleSender(
            IBundleReplier bundleReplier,
            IBundleCreatorProvider bundleCreatorProvider,
            ILoggerFactory loggerFactory)
        {
            _bundleReplier = bundleReplier;
            _bundleCreatorProvider = bundleCreatorProvider;
            _logger = loggerFactory.CreateLogger(nameof(BundleSender));
        }

        public async Task SendAsync(DataBundleRequestDto request)
        {
            try
            {
                var bundleCreator = _bundleCreatorProvider.Get(request);

#pragma warning disable CA2007
                await using var bundleStream = new MemoryStream();
#pragma warning restore CA2007

                await bundleCreator.CreateAsync(request, bundleStream).ConfigureAwait(false);
                await _bundleReplier.ReplyAsync(bundleStream, request).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Exception caught during bundle operation");
                await _bundleReplier.ReplyErrorAsync(e, request).ConfigureAwait(false);
            }
        }
    }
}
