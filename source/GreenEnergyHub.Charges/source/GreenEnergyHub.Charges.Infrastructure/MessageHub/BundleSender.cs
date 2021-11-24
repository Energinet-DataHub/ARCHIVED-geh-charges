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

using System.IO;
using System.Threading.Tasks;
using Energinet.DataHub.MessageHub.Model.Model;
using GreenEnergyHub.Charges.Application.MessageHub;

namespace GreenEnergyHub.Charges.Infrastructure.MessageHub
{
    public class BundleSender : IBundleSender
    {
        private readonly IBundleCreatorProvider _bundleCreatorProvider;
        private readonly IBundleReplier _bundleReplier;

        public BundleSender(
            IBundleReplier bundleReplier, IBundleCreatorProvider bundleCreatorProvider)
        {
            _bundleReplier = bundleReplier;
            _bundleCreatorProvider = bundleCreatorProvider;
        }

        public async Task SendAsync(DataBundleRequestDto request)
        {
            var bundleCreator = _bundleCreatorProvider.Get(request);
            await using var bundleStream = new MemoryStream();

            await bundleCreator.CreateAsync(request, bundleStream);
            await _bundleReplier.ReplyAsync(bundleStream, request);
        }
    }
}
