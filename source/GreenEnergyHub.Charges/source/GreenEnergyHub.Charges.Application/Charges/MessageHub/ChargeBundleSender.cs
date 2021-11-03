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
using Energinet.DataHub.MessageHub.Client.Model;
using GreenEnergyHub.Charges.Application.Charges.MessageHub.Infrastructure;

namespace GreenEnergyHub.Charges.Application.Charges.MessageHub
{
    public class ChargeBundleSender : IChargeBundleSender
    {
        private readonly IChargeBundleCreator _chargeBundleCreator;
        private readonly IChargeBundleReplier _chargeBundleReplier;

        public ChargeBundleSender(
            IChargeBundleCreator chargeLinkBundleCreator,
            IChargeBundleReplier chargeLinkBundleReplier)
        {
            _chargeBundleCreator = chargeLinkBundleCreator;
            _chargeBundleReplier = chargeLinkBundleReplier;
        }

        public async Task SendAsync(DataBundleRequestDto request, Stream outputStream)
        {
            await using var bundleStream = new MemoryStream();
            var streamBundledCorrectly = await _chargeBundleCreator.CreateSuccessfullyAsync(request, bundleStream);
            await _chargeBundleReplier.ReplyAsync(bundleStream, request, streamBundledCorrectly);
        }
    }
}
